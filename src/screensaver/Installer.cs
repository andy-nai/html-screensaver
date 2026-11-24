using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace HtmlScreensaver;

// The same executable is both the installer and the screensaver:
//   HtmlScreensaver-Setup.exe            -> installs (copies itself as a .scr, unpacks the bundled scenes)
//   HtmlScreensaver.scr /uninstall       -> what Windows runs from Settings > Apps > Installed apps
// Add /quiet to either to skip the dialogs.
// Everything is per-user (no admin rights): files go to %LOCALAPPDATA%\Programs\HtmlScreensaver.
static class Installer
{
    const string Title = "HTML Screensaver";
    const string ScrName = "HtmlScreensaver.scr";
    const string DesktopKey = @"Control Panel\Desktop";
    const string UninstallKey = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\HtmlScreensaver";

    public static string InstallDir => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "HtmlScreensaver");
    static string InstalledScr => Path.Combine(InstallDir, ScrName);

    static string Version =>
        Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0";

    public static void Install(bool quiet)
    {
        if (!quiet && MessageBox.Show(
                $"Install {Title} {Version} and set it as your screensaver?\n\n" +
                $"It will be installed for your account only, in:\n{InstallDir}",
                Title, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
            return;

        try
        {
            Directory.CreateDirectory(InstallDir);
            string self = Environment.ProcessPath!;
            if (!string.Equals(Path.GetFullPath(self), InstalledScr, StringComparison.OrdinalIgnoreCase))
                File.Copy(self, InstalledScr, overwrite: true);

            int scenes = UnpackScenes(Path.Combine(InstallDir, "scenes"));

            using (var desk = Registry.CurrentUser.CreateSubKey(DesktopKey))
                desk.SetValue("SCRNSAVE.EXE", InstalledScr);
            SetScreensaverActive(true);

            RegisterUninstaller();

            if (quiet) return;
            if (MessageBox.Show(
                    $"{Title} is installed and set as your screensaver ({scenes} scene file(s) unpacked).\n\n" +
                    "Open Screen Saver Settings to choose the wait time or preview it?",
                    Title, MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                Process.Start(new ProcessStartInfo("control.exe", "desk.cpl,,@screensaver") { UseShellExecute = true });
        }
        catch (IOException ex)
        {
            Fail($"Couldn't copy the files. If the screensaver is running, close it and try again.\n\n{ex.Message}", quiet);
        }
        catch (Exception ex)
        {
            Fail($"Installation failed.\n\n{ex.Message}", quiet);
        }
    }

    public static void Uninstall(bool quiet)
    {
        if (!quiet && MessageBox.Show(
                $"Remove {Title}?\n\nThis deletes {InstallDir}, including any scenes you added there.",
                Title, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
            return;

        // Only switch the screensaver off if it's still ours
        using (var desk = Registry.CurrentUser.OpenSubKey(DesktopKey, writable: true))
        {
            if (desk?.GetValue("SCRNSAVE.EXE") is string current &&
                current.EndsWith(ScrName, StringComparison.OrdinalIgnoreCase))
            {
                desk.DeleteValue("SCRNSAVE.EXE", throwOnMissingValue: false);
                SetScreensaverActive(false);
            }
        }

        Registry.CurrentUser.DeleteSubKeyTree(UninstallKey, throwOnMissingSubKey: false);
        TryDeleteDir(Config.DataDir);   // WebView2 profile and last-used scene

        // This process is usually the installed .scr, which can't delete itself while running:
        // remove everything else now and leave the folder to a short-lived cmd after we exit.
        string self = Path.GetFullPath(Environment.ProcessPath!);
        bool runningFromInstall = self.StartsWith(InstallDir + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
        if (runningFromInstall)
        {
            TryDeleteDir(Path.Combine(InstallDir, "scenes"));
            Process.Start(new ProcessStartInfo("cmd.exe",
                $"/c ping -n 3 127.0.0.1 >nul & rmdir /s /q \"{InstallDir}\"")
            { CreateNoWindow = true, UseShellExecute = false, WindowStyle = ProcessWindowStyle.Hidden });
        }
        else
        {
            TryDeleteDir(InstallDir);
        }

        if (!quiet)
            MessageBox.Show($"{Title} has been removed.", Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    // The repo's scenes folder is embedded at build time (see the .csproj). Files already on disk are
    // overwritten with the bundled version; scenes you added yourself are left alone.
    static int UnpackScenes(string targetDir)
    {
        var asm = Assembly.GetExecutingAssembly();
        int count = 0;
        foreach (string name in asm.GetManifestResourceNames())
        {
            string rel = name.Replace('\\', '/');
            if (!rel.StartsWith("scenes/", StringComparison.Ordinal)) continue;
            rel = rel["scenes/".Length..];

            string dest = Path.GetFullPath(Path.Combine(targetDir, rel));
            if (!dest.StartsWith(Path.GetFullPath(targetDir) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                continue;

            Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
            using var src = asm.GetManifestResourceStream(name)!;
            using var dst = File.Create(dest);
            src.CopyTo(dst);
            count++;
        }
        return count;
    }

    static void RegisterUninstaller()
    {
        long sizeKb = new DirectoryInfo(InstallDir)
            .EnumerateFiles("*", SearchOption.AllDirectories).Sum(f => f.Length) / 1024;

        using var key = Registry.CurrentUser.CreateSubKey(UninstallKey);
        key.SetValue("DisplayName", Title);
        key.SetValue("DisplayVersion", Version);
        key.SetValue("DisplayIcon", InstalledScr);
        key.SetValue("InstallLocation", InstallDir);
        key.SetValue("UninstallString", $"\"{InstalledScr}\" /uninstall");
        key.SetValue("QuietUninstallString", $"\"{InstalledScr}\" /uninstall /quiet");
        key.SetValue("EstimatedSize", (int)Math.Min(sizeKb, int.MaxValue), RegistryValueKind.DWord);
        key.SetValue("NoModify", 1, RegistryValueKind.DWord);
        key.SetValue("NoRepair", 1, RegistryValueKind.DWord);
    }

    static void TryDeleteDir(string dir)
    {
        try { if (Directory.Exists(dir)) Directory.Delete(dir, recursive: true); } catch { }
    }

    static void Fail(string message, bool quiet)
    {
        if (quiet) Environment.ExitCode = 1;
        else MessageBox.Show(message, Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    // Writes ScreenSaveActive and tells running apps (and the Settings dialog) about the change
    const uint SPI_SETSCREENSAVEACTIVE = 0x0011;
    const uint SPIF_UPDATEINIFILE = 0x01, SPIF_SENDCHANGE = 0x02;

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool SystemParametersInfo(uint action, uint param, IntPtr vparam, uint winIni);

    static void SetScreensaverActive(bool on) =>
        SystemParametersInfo(SPI_SETSCREENSAVEACTIVE, on ? 1u : 0u, IntPtr.Zero, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
}
