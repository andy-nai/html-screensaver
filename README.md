# HTML Screensaver

A Windows screensaver that picks a random **scene** (a folder containing an `index.html`) each time
it starts and shows it full screen on every monitor. It uses the Edge WebView2 runtime, which is
built into Windows 11 and most up-to-date Windows 10 machines.

## The package

```
HtmlScreensaver\
  HtmlScreensaver.scr   the screensaver (single self-contained file, no .NET install needed)
  scenes\               one sub-folder per scene, each with an index.html
    <scene-name>\        index.html, plus any assets it uses
  install.ps1           activate (no admin needed)
  uninstall.ps1
  README.md
```

The `scenes` folder always sits next to the `.scr`.

## Install (on any PC)

1. Put the folder somewhere permanent (e.g. `D:\Screensavers\HtmlScreensaver`). Don't move it afterwards without re-running install.
2. Right-click `install.ps1` > *Run with PowerShell*, or run `.\install.ps1` from a terminal.
   Set the wait time in Windows' Screen Saver Settings.
3. Test without waiting: `.\HtmlScreensaver.scr /debug` (normal window, Esc quits, F12 opens DevTools).
   Force a scene: `.\HtmlScreensaver.scr /debug /scene:<scene-name>`

Uninstall: run `uninstall.ps1`, then delete the folder.

## Scenes

- A scene is any sub-folder of `scenes\` containing an `index.html`. The folder name is the scene name.
- One scene is picked at random each time the screensaver starts (never the same twice in a row if you have 2+),
  and shown on all monitors.
- Each scene is served as its own site at `https://screensaver.local/`, so relative paths, `fetch()`, ES modules
  and local assets inside the folder all work. Keep paths relative (`./app.js`).
- Add, edit or remove scenes any time; no rebuild or reinstall needed.
- A scene can include its own `README.md` describing what it shows and how to configure it.
- It quits on a key press, click, or mouse movement of more than ~10 px (after a short grace period).

### Making a scene

1. Create an `index.html` (plus any scripts, images or other files it uses).
2. Create a sub-folder under `scenes\` named after your scene, e.g. `scenes\my-scene\`.
3. Drop the files in there. That's it: the screensaver will pick it up next time it starts.

To try it straight away: `.\HtmlScreensaver.scr /debug /scene:my-scene`

## Building from source

Layout of the source tree:

```
src\screensaver\     C# project (HtmlScreensaver.csproj, Program.cs, ScreensaverForm.cs)
src\installer\       install.ps1, uninstall.ps1 (copied into the package)
scenes\              scenes, copied into the package as-is
build.ps1            builds the project and assembles dist\
README.md            copied into the package
```

Needs the latest .NET SDK (`winget install Microsoft.DotNet.SDK.11`; any current SDK works, the script
targets whichever is installed).

```powershell
.\build.ps1                       # self-contained single file (recommended)
.\build.ps1 -FrameworkDependent   # much smaller, but target PCs need the matching .NET Desktop Runtime
```

Output: `dist\HtmlScreensaver\`, the package folder.

The only third-party dependency is the `Microsoft.Web.WebView2` NuGet package, which is compiled into the exe.

## Troubleshooting

- **"No scenes found"**: each scene needs its own sub-folder with an `index.html`, inside `scenes\` next to the `.scr`.
- **Black screen**: run with `/debug /scene:<name>` and press F12 for console errors.
- **Nothing happens / blank WebView**: the WebView2 Evergreen Runtime must be installed on the PC.
- **Managed PC**: Group Policy can enforce or block screensaver settings.
