namespace HtmlScreensaver;

// The installer (and uninstaller) window: who to install for, a progress bar, and a scrolling log of
// what it's doing.
sealed class InstallerForm : Form
{
    readonly bool uninstall;
    readonly bool autoStart;
    InstallScope scope;
    bool busy;
    public bool Succeeded { get; private set; }

    readonly RadioButton justMe = new(), everyone = new();
    readonly Label folder = new(), status = new();
    readonly ProgressBar bar = new();
    readonly RichTextBox log = new();
    readonly Button primary = new(), secondary = new(), settings = new();

    static readonly Color ConsoleBack = Color.FromArgb(12, 12, 12), ConsoleText = Color.FromArgb(204, 204, 204);
    static readonly Color Good = Color.FromArgb(110, 220, 120), Bad = Color.FromArgb(255, 110, 100), Dim = Color.FromArgb(140, 140, 140);

    public InstallerForm(bool uninstall, InstallScope scope, bool autoStart)
    {
        this.uninstall = uninstall;
        this.scope = scope;
        this.autoStart = autoStart;

        Text = uninstall ? $"Uninstall {Installer.Title}" : $"{Installer.Title} Setup";
        Font = new Font("Segoe UI", 9f);
        AutoScaleDimensions = new SizeF(96f, 96f);
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(580, 470);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        try { Icon = Icon.ExtractAssociatedIcon(Environment.ProcessPath!); } catch { }

        var title = new Label
        {
            Text = Installer.Title, Font = new Font("Segoe UI Semibold", 15f), AutoSize = true, Location = new Point(18, 14),
        };
        var sub = new Label
        {
            Text = $"Version {Installer.Version}  ·  Random HTML scenes as your Windows screensaver",
            ForeColor = SystemColors.GrayText, AutoSize = true, Location = new Point(20, 48),
        };
        Controls.Add(title); Controls.Add(sub);

        int y = 80;
        if (!uninstall)
        {
            var group = new GroupBox { Text = "Install for", Location = new Point(18, y), Size = new Size(544, 82) };
            justMe.Text = "Just me  (no administrator rights needed)";
            everyone.Text = "Everyone on this PC  (asks for administrator rights)";
            justMe.AutoSize = everyone.AutoSize = true;
            justMe.Location = new Point(14, 24); everyone.Location = new Point(14, 50);
            justMe.Checked = scope == InstallScope.User; everyone.Checked = scope == InstallScope.AllUsers;
            justMe.CheckedChanged += (_, _) => { this.scope = everyone.Checked ? InstallScope.AllUsers : InstallScope.User; ShowFolder(); };
            group.Controls.Add(justMe); group.Controls.Add(everyone);
            Controls.Add(group);
            y += 92;
        }

        folder.AutoSize = false; folder.Location = new Point(20, y); folder.Size = new Size(540, 34);
        folder.ForeColor = SystemColors.GrayText;
        Controls.Add(folder);
        ShowFolder();
        y += 38;

        bar.Location = new Point(18, y); bar.Size = new Size(544, 16); bar.Style = ProgressBarStyle.Continuous;
        bar.Maximum = 1000;
        Controls.Add(bar);
        y += 24;

        log.Location = new Point(18, y); log.Size = new Size(544, ClientSize.Height - y - 58);
        log.ReadOnly = true; log.BorderStyle = BorderStyle.None; log.ScrollBars = RichTextBoxScrollBars.Vertical;
        log.BackColor = ConsoleBack; log.ForeColor = ConsoleText; log.Font = new Font("Consolas", 9f);
        log.WordWrap = true; log.DetectUrls = false; log.TabStop = false;
        Controls.Add(log);

        status.AutoSize = false; status.Location = new Point(18, ClientSize.Height - 42); status.Size = new Size(230, 30);
        status.TextAlign = ContentAlignment.MiddleLeft;
        Controls.Add(status);

        settings.Text = "Screen Saver Settings…"; settings.Size = new Size(160, 30);
        settings.Location = new Point(ClientSize.Width - 18 - 92 - 8 - 160, ClientSize.Height - 42);
        settings.Visible = false;
        settings.Click += (_, _) => { try { Installer.OpenScreenSaverSettings(); } catch { } };
        Controls.Add(settings);

        primary.Text = uninstall ? "Uninstall" : Installer.IsInstalled(scope) ? "Update" : "Install";
        primary.Size = new Size(92, 30);
        primary.Location = new Point(ClientSize.Width - 18 - 92 - 8 - 92, ClientSize.Height - 42);
        primary.Click += (_, _) => Go();
        Controls.Add(primary);

        secondary.Text = "Cancel"; secondary.Size = new Size(92, 30);
        secondary.Location = new Point(ClientSize.Width - 18 - 92, ClientSize.Height - 42);
        secondary.Click += (_, _) => Close();
        Controls.Add(secondary);

        AcceptButton = primary; CancelButton = secondary;
        FormClosing += (_, e) => { if (busy) e.Cancel = true; };   // don't stop half way

        Write(uninstall
            ? "Ready to remove HTML Screensaver, including any scenes you've added to its folder."
            : "Ready. Choose who it's for, then press the button below.", Dim);
        if (!uninstall && !Installer.IsAdmin) Write("(Installing for everyone will ask for administrator permission.)", Dim);
        if (autoStart) Shown += (_, _) => Go();
    }

    void ShowFolder()
    {
        folder.Text = uninstall ? $"Installed in {Installer.InstallDir(scope)}."
            : scope == InstallScope.AllUsers
                ? $"Installs to {Installer.InstallDir(scope)}, sets it as your screensaver and adds it to every user's Screen Saver Settings list."
                : $"Installs to {Installer.InstallDir(scope)} and sets it as your screensaver.";
        if (!uninstall && primary.IsHandleCreated) primary.Text = Installer.IsInstalled(scope) ? "Update" : "Install";
    }

    void Go()
    {
        if (Succeeded) { Close(); return; }
        // installing for everyone needs administrator rights: start again elevated, carrying on from here
        if (!uninstall && scope == InstallScope.AllUsers && !Installer.IsAdmin)
        {
            Write("Asking for administrator permission...", Dim);
            if (Installer.RelaunchElevated(Environment.ProcessPath!, "/install /allusers /auto")) { Close(); return; }
            Write("Administrator permission wasn't given, so nothing was installed.", Bad);
            Write("Choose \"Just me\" to install without it.", Dim);
            return;
        }

        busy = true;
        primary.Enabled = secondary.Enabled = justMe.Enabled = everyone.Enabled = false;
        status.Text = uninstall ? "Removing…" : "Installing…";
        log.Clear();
        var work = Task.Run(() =>
        {
            Action<string> line = s => Write(s, null);
            Action<double> step = p => BeginInvoke(() => bar.Value = (int)Math.Round(Math.Clamp(p, 0, 1) * bar.Maximum));
            if (uninstall) Installer.RunUninstall(scope, line, step);
            else Installer.RunInstall(scope, line, step);
        });
        work.ContinueWith(t => BeginInvoke(() =>
        {
            busy = false;
            if (t.Exception is { } ex)
            {
                Write("", null);
                Write("Error: " + (ex.InnerException ?? ex).Message, Bad);
                status.Text = "Something went wrong.";
                primary.Text = "Try again";
                primary.Enabled = secondary.Enabled = justMe.Enabled = everyone.Enabled = true;
                return;
            }
            Succeeded = true;
            bar.Value = bar.Maximum;
            Write("", null);
            if (uninstall)
            {
                Write("HTML Screensaver has been removed.", Good);
                status.Text = "Removed.";
            }
            else
            {
                Write("All done. HTML Screensaver is now your screensaver.", Good);
                Write("Set how long Windows waits before starting it in Screen Saver Settings.", Dim);
                if (scope == InstallScope.AllUsers) Write("Other people on this PC can choose it from their own Screen Saver Settings.", Dim);
                status.Text = "Installed.";
                settings.Visible = true;
            }
            primary.Text = "Close";
            primary.Enabled = true;
            secondary.Visible = false;
            primary.Location = secondary.Location;
            settings.Location = new Point(primary.Left - 8 - settings.Width, settings.Top);
            primary.Focus();
        }));
    }

    // Adds a line to the log (safe from any thread) and keeps it scrolled to the bottom
    void Write(string text, Color? color)
    {
        if (InvokeRequired) { BeginInvoke(() => Write(text, color)); return; }
        log.SelectionStart = log.TextLength;
        log.SelectionLength = 0;
        log.SelectionColor = color ?? ConsoleText;
        log.AppendText(text + Environment.NewLine);
        log.SelectionColor = log.ForeColor;
        log.ScrollToCaret();
    }
}
