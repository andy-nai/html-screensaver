using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace HtmlScreensaver;

class ScreensaverForm : Form
{
    readonly WebView2 web = new();
    readonly string folder;
    readonly bool debug;
    readonly System.Windows.Forms.Timer poll = new() { Interval = 100 };
    readonly DateTime created = DateTime.UtcNow;
    Point? startCursor;
    static bool exiting;

    public ScreensaverForm(Rectangle bounds, string folder, bool debug)
    {
        this.folder = folder;
        this.debug = debug;

        BackColor = Color.Black;
        StartPosition = FormStartPosition.Manual;
        KeyPreview = true;

        if (debug)
        {
            Text = "HTML Screensaver (debug - press Esc to quit)";
            FormBorderStyle = FormBorderStyle.Sizable;
            Bounds = new Rectangle(bounds.X + 100, bounds.Y + 100, 1280, 720);
        }
        else
        {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            TopMost = true;
            Bounds = bounds;
        }

        web.Dock = DockStyle.Fill;
        web.DefaultBackgroundColor = Color.Black;
        Controls.Add(web);

        KeyDown += (_, e) => { if (!debug || e.KeyCode == Keys.Escape) ExitSaver(); };
        FormClosed += (_, _) => { if (debug) ExitSaver(); };
        Load += async (_, _) => await InitWebAsync();
        Shown += (_, _) => { startCursor = Cursor.Position; poll.Start(); };
        poll.Tick += OnPoll;
    }

    async Task InitWebAsync()
    {
        var env = await CoreWebView2Environment.CreateAsync(null, Config.UserDataDir);
        await web.EnsureCoreWebView2Async(env);
        var core = web.CoreWebView2;

        core.Settings.AreDefaultContextMenusEnabled = debug;
        core.Settings.AreDevToolsEnabled = debug;
        core.Settings.AreBrowserAcceleratorKeysEnabled = debug;
        core.Settings.IsStatusBarEnabled = false;
        core.Settings.IsZoomControlEnabled = false;
        core.Settings.IsPinchZoomEnabled = false;
        core.Settings.IsSwipeNavigationEnabled = false;

        // Serve the folder over https://screensaver.local/ so fetch(), modules, etc. behave like a real site
        core.SetVirtualHostNameToFolderMapping(
            "screensaver.local", folder, CoreWebView2HostResourceAccessKind.Allow);

        await core.AddScriptToExecuteOnDocumentCreatedAsync(InputScript(debug));
        core.WebMessageReceived += (_, e) =>
        {
            if (e.TryGetWebMessageAsString() == "exit") ExitSaver();
        };

        core.Navigate("https://screensaver.local/index.html");
    }

    // The WebView2 control swallows input, so the page reports it back to us.
    static string InputScript(bool debug) => $$"""
        (() => {
          const debug = {{(debug ? "true" : "false")}};
          const quit = () => window.chrome.webview.postMessage('exit');
          let ready = false, x0 = null, y0 = null;
          setTimeout(() => ready = true, 800);   // grace period so start-up jitter doesn't dismiss us

          addEventListener('mousemove', e => {
            if (debug || !ready) return;
            if (x0 === null) { x0 = e.screenX; y0 = e.screenY; return; }
            if (Math.abs(e.screenX - x0) + Math.abs(e.screenY - y0) > 10) quit();
          }, true);
          addEventListener('mousedown',  () => { if (!debug && ready) quit(); }, true);
          addEventListener('touchstart', () => { if (!debug && ready) quit(); }, true);
          addEventListener('keydown', e => {
            if (debug ? e.key === 'Escape' : ready) quit();
          }, true);

          if (!debug) {
            document.addEventListener('DOMContentLoaded', () => {
              const s = document.createElement('style');
              s.textContent = '*{cursor:none !important}';
              document.head.appendChild(s);
            });
          }
        })();
        """;

    // Backup for the other monitors' forms: if the mouse moves anywhere, quit.
    void OnPoll(object? sender, EventArgs e)
    {
        if (debug || startCursor is null) return;
        if ((DateTime.UtcNow - created).TotalMilliseconds < 800) return;

        var p = Cursor.Position;
        if (Math.Abs(p.X - startCursor.Value.X) + Math.Abs(p.Y - startCursor.Value.Y) > 10)
            ExitSaver();
    }

    static void ExitSaver()
    {
        if (exiting) return;
        exiting = true;
        Application.Exit();
    }
}
