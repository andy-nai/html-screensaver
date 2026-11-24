using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.Win32;

namespace HtmlScreensaver;

enum InstallScope { User, AllUsers }

// The same executable is both the installer and the screensaver:
//   HtmlScreensaver-Setup.exe              -> the installer window
//   HtmlScreensaver-Setup.exe /allusers    -> ...with "Everyone on this PC" already chosen
//   HtmlScreensaver.scr /uninstall         -> what Windows runs from Settings > Apps > Installed apps
// Add /quiet to install or uninstall with no window (for everyone, run it from an administrator prompt).
//
// Just me:  %LOCALAPPDATA%\Programs\HtmlScreensaver, no administrator rights needed.
// Everyone: %ProgramFiles%\HtmlScreensaver, plus a copy of the .scr in System32 so it's in every user's
//           Screen Saver Settings list. A screensaver is a per-user setting, so it's switched on for the
//           person installing; everyone else can pick it from their list.
static class Installer
{
    public const string Title = "HTML Screensaver";
    const string ScrName = "HtmlScreensaver.scr";
    const string DesktopKey = @"Control Panel\Desktop";
    const string UninstallKey = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\HtmlScreensaver";

    public static string Version =>
        Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0";

    public static string InstallDir(InstallScope s) => s == InstallScope.User
        ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "HtmlScreensaver")
        : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "HtmlScreensaver");
    static string SystemScr => Path.Combine(Environment.SystemDirectory, ScrName);
    static RegistryKey Root(InstallScope s) => s == InstallScope.User ? Registry.CurrentUser : Registry.LocalMachine;

    public static bool IsInstalled(InstallScope s)
    {
        using var key = Root(s).OpenSubKey(UninstallKey);
        return key is not null;
    }

    public static bool IsAdmin =>
        new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

    // Which install a copy of the program belongs to, judged by where it lives
    public static InstallScope ScopeOf(string path)
    {
        string full = Path.GetFullPath(path);
        return full.StartsWith(InstallDir(InstallScope.AllUsers) + "\\", StringComparison.OrdinalIgnoreCase) ||
               full.Equals(SystemScr, StringComparison.OrdinalIgnoreCase)
            ? InstallScope.AllUsers : InstallScope.User;
    }

    /* ------------------------------------------------------------------
     * Entry points
     * ------------------------------------------------------------------ */
    public static void Install(bool quiet, bool allUsers, bool autoStart)
    {
        var scope = allUsers ? InstallScope.AllUsers : InstallScope.User;
        if (!quiet)
        {
            // preselect whichever install is already there, so running a newer setup updates it
            if (!allUsers && IsInstalled(InstallScope.AllUsers) && !IsInstalled(InstallScope.User)) scope = InstallScope.AllUsers;
            Application.Run(new InstallerForm(uninstall: false, scope, autoStart));
            return;
        }
        if (scope == InstallScope.AllUsers && !IsAdmin) { Environment.ExitCode = 5; return; }   // access denied
        try { RunInstall(scope, _ => { }, _ => { }); }
        catch { Environment.ExitCode = 1; }
    }

    public static void Uninstall(bool quiet, bool allUsers, string? tempCopy)
    {
        var scope = allUsers ? InstallScope.AllUsers : ScopeOf(Environment.ProcessPath!);
        if (scope == InstallScope.AllUsers && !IsAdmin)
        {
            if (quiet) { Environment.ExitCode = 5; return; }
            // Windows won't run a .scr "as administrator" with arguments, so run a temporary .exe copy
            // instead; it also means the install folder isn't in use while it's being deleted.
            string temp = Path.Combine(Path.GetTempPath(), $"HtmlScreensaver-Uninstall-{Guid.NewGuid():N}.exe");
            File.Copy(Environment.ProcessPath!, temp);
            if (!RelaunchElevated(temp, $"/uninstall /allusers /tempcopy:\"{temp}\""))
            {
                TryDelete(temp);
                MessageBox.Show("Removing HTML Screensaver for everyone needs administrator permission.",
                    Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            return;
        }
        if (quiet)
        {
            try { RunUninstall(scope, _ => { }, _ => { }); } catch { Environment.ExitCode = 1; }
            ScheduleCleanup(scope, tempCopy);
            return;
        }
        var form = new InstallerForm(uninstall: true, scope, autoStart: false);
        Application.Run(form);
        if (form.Succeeded || tempCopy is not null) ScheduleCleanup(form.Succeeded ? scope : null, tempCopy);
    }

    // Starts a program as administrator; false if the person said no to the prompt
    public static bool RelaunchElevated(string exe, string args)
    {
        try
        {
            Process.Start(new ProcessStartInfo(exe, args) { UseShellExecute = true, Verb = "runas" });
            return true;
        }
        catch (System.ComponentModel.Win32Exception) { return false; }   // the UAC prompt was declined
    }

    /* ------------------------------------------------------------------
     * The work, reported step by step (log lines, and progress from 0 to 1)
     * ------------------------------------------------------------------ */
    public static void RunInstall(InstallScope scope, Action<string> log, Action<double> progress)
    {
        string dir = InstallDir(scope), scr = Path.Combine(dir, ScrName);
        string self = Path.GetFullPath(Environment.ProcessPath!);
        var scenes = SceneResources();
        bool otherInstall = scope == InstallScope.AllUsers && IsInstalled(InstallScope.User);
        int total = scenes.Count + 5 + (scope == InstallScope.AllUsers ? 1 : 0) + (otherInstall ? 1 : 0), done = 0;
        void Step(string text) { log(text); progress(Math.Min(1, ++done / (double)total)); }

        log($"Installing {Title} {Version} for {(scope == InstallScope.User ? "you only" : "everyone on this PC")}");
        log($"Folder: {dir}");
        if (IsInstalled(scope)) log("Found an earlier install: updating it (scenes you've added are kept).");
        Directory.CreateDirectory(dir);
        Step("Created the folder");

        if (!string.Equals(self, scr, StringComparison.OrdinalIgnoreCase))
        {
            log($"Copying {ScrName} ({new FileInfo(self).Length / (1024 * 1024)} MB)...");
            try { File.Copy(self, scr, overwrite: true); }
            catch (IOException) { throw new IOException("The screensaver is running or in use. Close it and try again."); }
        }
        Step($"Copied {ScrName}");

        // The repo's scenes are embedded at build time (see the .csproj). Files already on disk are
        // overwritten with the bundled version; scenes you added yourself are left alone.
        string scenesDir = Path.Combine(dir, "scenes");
        var asm = Assembly.GetExecutingAssembly();
        log("Unpacking scenes:");
        foreach (var (name, rel) in scenes)
        {
            string dest = Path.GetFullPath(Path.Combine(scenesDir, rel));
            if (!dest.StartsWith(Path.GetFullPath(scenesDir) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) continue;
            Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
            using (var src = asm.GetManifestResourceStream(name)!)
            using (var dst = File.Create(dest)) src.CopyTo(dst);
            Step($"  scenes\\{rel.Replace('/', '\\')}");
        }

        string active = scr;
        if (scope == InstallScope.AllUsers)
        {
            File.Copy(scr, SystemScr, overwrite: true);
            active = SystemScr;
            Step($"Copied {ScrName} to {Environment.SystemDirectory}, so it's listed for every user");
        }

        using (var desk = Registry.CurrentUser.CreateSubKey(DesktopKey))
            desk.SetValue("SCRNSAVE.EXE", active);
        SetScreensaverActive(true);
        Step("Set as your screensaver");

        RegisterUninstaller(scope);
        Step("Added to Settings > Apps > Installed apps");

        if (otherInstall)
        {
            RemoveFiles(InstallDir(InstallScope.User), log);
            Registry.CurrentUser.DeleteSubKeyTree(UninstallKey, throwOnMissingSubKey: false);
            Step("Removed your earlier just-for-you install");
        }
        progress(1);
        log("Done.");
    }

    public static void RunUninstall(InstallScope scope, Action<string> log, Action<double> progress)
    {
        string dir = InstallDir(scope);
        log($"Removing {Title} ({(scope == InstallScope.User ? "installed for you only" : "installed for everyone")})");
        log($"Folder: {dir}");
        progress(.1);

        // only switch the screensaver off if it's still ours
        using (var desk = Registry.CurrentUser.OpenSubKey(DesktopKey, writable: true))
        {
            if (desk?.GetValue("SCRNSAVE.EXE") is string current && current.EndsWith(ScrName, StringComparison.OrdinalIgnoreCase))
            {
                desk.DeleteValue("SCRNSAVE.EXE", throwOnMissingValue: false);
                SetScreensaverActive(false);
                log("Switched off as your screensaver");
            }
        }
        progress(.25);

        Root(scope).DeleteSubKeyTree(UninstallKey, throwOnMissingSubKey: false);
        log("Removed from Settings > Apps > Installed apps");
        progress(.4);

        if (TryDeleteDir(Config.DataDir)) log("Removed your settings and browser cache");
        progress(.5);

        if (scope == InstallScope.AllUsers && File.Exists(SystemScr))
        {
            log(TryDelete(SystemScr) ? $"Removed the copy in {Environment.SystemDirectory}"
                                     : $"Couldn't remove {SystemScr} (in use?); delete it after a restart");
        }
        progress(.6);

        RemoveFiles(dir, log);
        progress(1);
        log("Done.");
    }

    // Deletes an install folder, logging as it goes. The running .scr can't delete itself, so if it's in
    // there it's left for ScheduleCleanup to take away once this program has exited.
    static void RemoveFiles(string dir, Action<string> log)
    {
        if (!Directory.Exists(dir)) return;
        string self = Path.GetFullPath(Environment.ProcessPath!);
        log("Removing files:");
        foreach (var f in Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories).ToList())
        {
            if (string.Equals(f, self, StringComparison.OrdinalIgnoreCase)) continue;
            if (TryDelete(f)) log("  " + Path.GetRelativePath(dir, f));
        }
        foreach (var d in Directory.EnumerateDirectories(dir, "*", SearchOption.AllDirectories).OrderByDescending(d => d.Length).ToList())
            try { Directory.Delete(d); } catch { }
        try { Directory.Delete(dir); } catch { }
    }

    // After uninstalling: once this process has exited, remove what it couldn't delete while running
    // (its own .scr and the install folder, or the temporary copy used to get administrator rights)
    static void ScheduleCleanup(InstallScope? scope, string? tempCopy)
    {
        var targets = new List<string>();
        if (scope is InstallScope s && Directory.Exists(InstallDir(s))) targets.Add($"rmdir /s /q \"{InstallDir(s)}\"");
        if (tempCopy is not null) targets.Add($"del /f /q \"{tempCopy}\"");
        if (targets.Count == 0) return;
        Process.Start(new ProcessStartInfo("cmd.exe", $"/c ping -n 3 127.0.0.1 >nul & {string.Join(" & ", targets)}")
            { CreateNoWindow = true, UseShellExecute = false, WindowStyle = ProcessWindowStyle.Hidden });
    }

    // The scenes embedded in this executable: (resource name, path inside scenes\)
    static List<(string name, string rel)> SceneResources() =>
        Assembly.GetExecutingAssembly().GetManifestResourceNames()
            .Select(n => (name: n, norm: n.Replace('\\', '/')))
            .Where(r => r.norm.StartsWith("scenes/", StringComparison.Ordinal))
            .Select(r => (r.name, r.norm["scenes/".Length..]))
            .OrderBy(r => r.Item2, StringComparer.OrdinalIgnoreCase)
            .ToList();

    static void RegisterUninstaller(InstallScope scope)
    {
        string dir = InstallDir(scope), scr = Path.Combine(dir, ScrName);
        long sizeKb = new DirectoryInfo(dir).EnumerateFiles("*", SearchOption.AllDirectories).Sum(f => f.Length) / 1024;
        string allUsers = scope == InstallScope.AllUsers ? " /allusers" : "";
        using var key = Root(scope).CreateSubKey(UninstallKey);
        key.SetValue("DisplayName", Title);
        key.SetValue("DisplayVersion", Version);
        key.SetValue("DisplayIcon", scr);
        key.SetValue("InstallLocation", dir);
        key.SetValue("UninstallString", $"\"{scr}\" /uninstall{allUsers}");
        key.SetValue("QuietUninstallString", $"\"{scr}\" /uninstall{allUsers} /quiet");
        key.SetValue("EstimatedSize", (int)Math.Min(sizeKb, int.MaxValue), RegistryValueKind.DWord);
        key.SetValue("NoModify", 1, RegistryValueKind.DWord);
        key.SetValue("NoRepair", 1, RegistryValueKind.DWord);
    }

    public static void OpenScreenSaverSettings() =>
        Process.Start(new ProcessStartInfo("control.exe", "desk.cpl,,@screensaver") { UseShellExecute = true });

    static bool TryDelete(string file) { try { File.Delete(file); return true; } catch { return false; } }
    static bool TryDeleteDir(string dir)
    {
        try { if (!Directory.Exists(dir)) return false; Directory.Delete(dir, recursive: true); return true; }
        catch { return false; }
    }

    // Writes ScreenSaveActive and tells running apps (and the Settings dialog) about the change
    const uint SPI_SETSCREENSAVEACTIVE = 0x0011;
    const uint SPIF_UPDATEINIFILE = 0x01, SPIF_SENDCHANGE = 0x02;

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool SystemParametersInfo(uint action, uint param, IntPtr vparam, uint winIni);

    static void SetScreensaverActive(bool on) =>
        SystemParametersInfo(SPI_SETSCREENSAVEACTIVE, on ? 1u : 0u, IntPtr.Zero, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
}
