# HTML Screensaver

A Windows screensaver that picks a random **scene** (a folder containing an `index.html`) each time
it starts and shows it full screen on every monitor. It uses the Edge WebView2 runtime, which is
built into Windows 11 and most up-to-date Windows 10 machines.

## Install

Run `HtmlScreensaver-Setup.exe`. It installs for your account only (no admin rights needed) and sets
HTML Screensaver as your screensaver. Then choose the wait time in Windows' Screen Saver Settings,
which the installer offers to open.

Running the installer again updates the screensaver and its included scenes; scenes you added
yourself are kept.

Uninstall from **Settings > Apps > Installed apps > HTML Screensaver**.

Everything is installed to `%LOCALAPPDATA%\Programs\HtmlScreensaver`:

```
HtmlScreensaver.scr   the screensaver
scenes\               one sub-folder per scene
  <scene-name>\       index.html, plus any assets it uses
```

## Scenes

- A scene is any sub-folder of `scenes\` containing an `index.html`. The folder name is the scene name.
- One scene is picked at random each time the screensaver starts (never the same twice in a row if you have 2+),
  and shown on all monitors.
- Each scene is served as its own site at `https://screensaver.local/`, so relative paths, `fetch()`, ES modules
  and local assets inside the folder all work. Keep paths relative (`./app.js`).
- Add, edit or remove scenes any time; no rebuild or reinstall needed. The **Settings** button in Screen
  Saver Settings opens the scenes folder.
- A scene can include its own `README.md` describing what it shows and how to configure it.
- It quits on a key press, click, or mouse movement of more than ~10 px (after a short grace period).

### Making a scene

1. Create an `index.html` (plus any scripts, images or other files it uses).
2. Create a sub-folder under `scenes\` named after your scene, e.g. `scenes\my-scene\`.
3. Drop the files in there. That's it: the screensaver will pick it up next time it starts.

To try it straight away, run the setup exe with `/debug` (it shows the installed scenes in a normal
window; Esc quits, F12 opens DevTools):

```powershell
.\HtmlScreensaver-Setup.exe /debug /scene:my-scene
```

Leave out `/scene:` for a random one. Use the setup exe rather than the `.scr` here: Windows ignores
arguments given to a `.scr` from a terminal and just starts the screensaver.

## Building from source

Layout of the source tree:

```
src\screensaver\     C# project: the screensaver and its built-in installer
scenes\              scenes, bundled into the installer
build.ps1            builds dist\HtmlScreensaver-Setup.exe
```

Needs the latest .NET SDK (`winget install Microsoft.DotNet.SDK.11`; any current SDK works, the script
targets whichever is installed).

```powershell
.\build.ps1                       # self-contained single file (recommended)
.\build.ps1 -FrameworkDependent   # much smaller, but target PCs need the matching .NET Desktop Runtime
```

Output: `dist\HtmlScreensaver-Setup.exe`, a single file with the scenes inside. The installer and the
screensaver are the same program: named `*.scr` it's the screensaver, under any other name it installs
itself. `/quiet` skips the installer's dialogs, and `/uninstall` removes it.

The only third-party dependency is the `Microsoft.Web.WebView2` NuGet package, which is compiled into the exe.

The installer isn't code-signed, so Windows SmartScreen may warn about it the first time it runs
(*More info > Run anyway*).

## Troubleshooting

- **"No scenes found"**: each scene needs its own sub-folder with an `index.html`, inside the installed `scenes\` folder.
- **Black screen**: run the setup exe with `/debug /scene:<name>` and press F12 for console errors.
- **Nothing happens / blank WebView**: the WebView2 Evergreen Runtime must be installed on the PC.
- **Managed PC**: Group Policy can enforce or block screensaver settings.
