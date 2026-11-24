using System.Diagnostics;

namespace HtmlScreensaver;

static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        // Windows passes: /s (run), /c or /c:<hwnd> (configure), /p <hwnd> (preview), or nothing (configure).
        // /debug runs a normal resizable window that only closes on Esc, so you can't lock yourself out.
        // /scene:<name> forces a particular scene folder (handy for testing).
        bool debug = args.Any(a => a.Equals("/debug", StringComparison.OrdinalIgnoreCase));
        string? forcedScene = args
            .FirstOrDefault(a => a.StartsWith("/scene:", StringComparison.OrdinalIgnoreCase))?[7..];
        // /install and /uninstall are handled by Installer, with /allusers (everyone on this PC rather than
        // just you) and /quiet (no window). Run with no arguments under any name other than *.scr
        // (e.g. HtmlScreensaver-Setup.exe), it's the installer.
        bool Flag(string f) => args.Any(a => a.Equals(f, StringComparison.OrdinalIgnoreCase));
        bool quiet = Flag("/quiet"), allUsers = Flag("/allusers"), autoStart = Flag("/auto");
        string? tempCopy = args.FirstOrDefault(a => a.StartsWith("/tempcopy:", StringComparison.OrdinalIgnoreCase))?[10..].Trim('"');
        bool isScr = Path.GetExtension(Environment.ProcessPath ?? "").Equals(".scr", StringComparison.OrdinalIgnoreCase);
        string mode = isScr ? "c" : "install";

        if (args.Length > 0)
        {
            mode = args[0].ToLowerInvariant().TrimStart('/', '-');
            int colon = mode.IndexOf(':');
            if (colon >= 0) mode = mode[..colon];
            if (mode == "debug" || mode == "scene") mode = "s";
            if (mode is "quiet" or "allusers" or "auto") mode = isScr ? "c" : "install";
        }

        switch (mode)
        {
            case "install":
                Installer.Install(quiet, allUsers, autoStart);
                break;
            case "uninstall":
                Installer.Uninstall(quiet, allUsers, tempCopy);
                break;
            case "s":
                RunScreensaver(debug, forcedScene);
                break;
            case "p":
                // Tiny preview pane in the Settings dialog. Nothing to draw; just exit.
                break;
            default:
                Configure();
                break;
        }
    }

    static void RunScreensaver(bool debug, string? forcedScene)
    {
        // Only one instance at a time
        using var mutex = new Mutex(true, "HtmlScreensaver.SingleInstance", out bool isNew);
        if (!isNew) return;

        string? scene = Config.PickScene(forcedScene);
        if (scene is null)
        {
            MessageBox.Show(
                $"No scenes found in:\n{Config.ScenesDir}\n\n" +
                "A scene is a sub-folder of 'scenes' containing an index.html.",
                "HTML Screensaver", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var screens = debug ? new[] { Screen.PrimaryScreen! } : Screen.AllScreens;
        // Same scene on every monitor
        var forms = screens.Select(s => new ScreensaverForm(s.Bounds, scene, debug)).ToList();
        foreach (var f in forms) f.Show();

        Application.Run();
    }

    static void Configure()
    {
        int count = Config.ListScenes().Count;
        var answer = MessageBox.Show(
            $"HTML Screensaver\n\nScenes folder:\n{Config.ScenesDir}\n\n" +
            $"{count} scene(s) found. Each scene is a sub-folder containing an index.html; " +
            "one is picked at random each time the screensaver starts.\n\nOpen the scenes folder?",
            "HTML Screensaver", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

        if (answer == DialogResult.Yes)
        {
            Directory.CreateDirectory(Config.ScenesDir);
            Process.Start(new ProcessStartInfo(Config.ScenesDir) { UseShellExecute = true });
        }
    }
}

static class Config
{
    // The scenes folder lives next to the executable. Run from anywhere else (the copy in System32 for an
    // install for everyone, or the setup exe with /debug), it uses an installed copy's scenes: yours if
    // you have one, otherwise the one installed for everyone.
    public static string AppDir => Path.GetDirectoryName(Environment.ProcessPath) ?? AppContext.BaseDirectory;
    public static string ScenesDir
    {
        get
        {
            string local = Path.Combine(AppDir, "scenes");
            if (Directory.Exists(local)) return local;
            string mine = Path.Combine(Installer.InstallDir(InstallScope.User), "scenes");
            return Directory.Exists(mine) ? mine : Path.Combine(Installer.InstallDir(InstallScope.AllUsers), "scenes");
        }
    }

    // Per-user writable data (WebView2 profile, last-used scene). The app folder may not be writable.
    public static string DataDir => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HtmlScreensaver");
    public static string UserDataDir => Path.Combine(DataDir, "WebView2");
    static string LastSceneFile => Path.Combine(DataDir, "last-scene.txt");

    // A scene is any sub-folder of the scenes folder that contains an index.html
    public static List<string> ListScenes()
    {
        try
        {
            return Directory.GetDirectories(ScenesDir)
                .Where(d => File.Exists(Path.Combine(d, "index.html")))
                .OrderBy(d => d, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
        catch { return new List<string>(); }
    }

    // Picks a random scene, avoiding the one used last time when there's more than one.
    public static string? PickScene(string? forced = null)
    {
        var scenes = ListScenes();
        if (scenes.Count == 0) return null;

        if (forced is not null)
        {
            var match = scenes.FirstOrDefault(s =>
                Path.GetFileName(s).Equals(forced, StringComparison.OrdinalIgnoreCase));
            if (match is not null) return match;
        }

        string? last = null;
        try { if (File.Exists(LastSceneFile)) last = File.ReadAllText(LastSceneFile).Trim(); } catch { }

        var pool = scenes.Count > 1
            ? scenes.Where(s => !Path.GetFileName(s).Equals(last, StringComparison.OrdinalIgnoreCase)).ToList()
            : scenes;

        string pick = pool[Random.Shared.Next(pool.Count)];
        try
        {
            Directory.CreateDirectory(DataDir);
            File.WriteAllText(LastSceneFile, Path.GetFileName(pick));
        }
        catch { }
        return pick;
    }
}
