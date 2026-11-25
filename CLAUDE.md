# CLAUDE.md

Guidance for AI agents working in this repo. Read this first; the README is the user-facing doc.

## What this is

**HTML Screensaver**: a Windows screensaver that shows a random **scene** full screen on every monitor.
A scene is a folder with an `index.html`. Most scenes are slow, relaxing, procedurally generated pixel-art
pages drawn on a canvas. It's a personal project; the README has an AI disclaimer, so keep it.

- `src/screensaver/` is one C# WinForms program (.NET, WebView2) that is **both the screensaver and its
  own installer**. Named `*.scr` it's the screensaver; under any other name (e.g. `HtmlScreensaver-Setup.exe`)
  with no arguments it opens the installer window.
- `scenes/` holds the scenes. They're embedded into the exe at build time and unpacked by the installer.
- `build.ps1` publishes a single self-contained `dist\HtmlScreensaver-Setup.exe`.
- `.github/workflows/release.yml` builds that on every push to `main` and publishes it as release
  `v<Version>`. The version is `<Version>` in `src/screensaver/HtmlScreensaver.csproj`. Bump it for a new
  release; pushes without a bump replace the current release's exe and move its tag.

## Layout

```
scenes/<name>/index.html   the scene (self-contained: inline JS/CSS, no build step, no dependencies)
scenes/<name>/config.js    user-editable settings (window.<NAME>_CONFIG), loaded by index.html
scenes/<name>/README.md    what the scene shows, its settings and test keys
src/screensaver/Program.cs         entry point: argument parsing, scene picking (Config class)
src/screensaver/ScreensaverForm.cs one borderless window per monitor hosting WebView2
src/screensaver/Installer.cs       install / uninstall logic (per-user or all-users)
src/screensaver/InstallerForm.cs   installer window: scope choice, progress bar, console-style log
src/screensaver/make-icon.py       draws app.ico (pixel-art monitor); stdlib only: python make-icon.py
build.ps1                          dotnet publish + copy to dist\HtmlScreensaver-Setup.exe
.claude/launch.json                "scenes" preview: python http.server on port 8123 at the repo root
```

## The screensaver program

- Windows calls the `.scr` with `/s` (run), `/c` (configure) or `/p <hwnd>` (preview, which does nothing).
  Extra flags: `/debug` opens a normal window (Esc quits, F12 opens DevTools), `/scene:<name>` forces a scene.
- Each scene folder is served at `https://screensaver.local/` (WebView2 virtual host mapping), so relative
  paths, `fetch()` and modules work. The page gets an injected script that quits on a key press, click or a
  mouse move over ~10 px, after an 800 ms grace period.
- Scenes folder lookup (`Config.ScenesDir`): `scenes\` next to the exe, else the per-user install, else the
  all-users install. Per-user data (WebView2 profile, last scene) lives in `%LOCALAPPDATA%\HtmlScreensaver`.
- A scene is picked at random and never repeats twice in a row.

## Installer

- **Just me**: `%LOCALAPPDATA%\Programs\HtmlScreensaver`, HKCU uninstall key, no admin.
- **Everyone**: `%ProgramFiles%\HtmlScreensaver` plus a copy of the `.scr` in `System32`, so it's in every
  user's Screen Saver list. HKLM uninstall key, needs admin. The installer relaunches itself with `runas`
  and `/install /allusers /auto`. The screensaver is a per-user setting, so it's only switched on for the
  installing user.
- Flags: `/install`, `/uninstall`, `/allusers`, `/quiet` (no window), `/auto` (start immediately).
- Updating overwrites the bundled scene files (including their `config.js`) and keeps scenes the user added.
- Windows **ignores arguments passed to a `.scr` through the shell** (ShellExecute: double-click,
  `Start-Process`, PowerShell `& x.scr /args`). The shell just starts it with `/S`. Test with the setup exe
  (`HtmlScreensaver-Setup.exe /debug /scene:x`) or use `ProcessStartInfo` with `UseShellExecute = false`.
  For the same reason, an elevated all-users uninstall runs from a temporary `.exe` copy.
- The everyone install and uninstall need someone to approve a UAC prompt, so an agent can't test them.

## Building and testing

- `.\build.ps1` targets whichever .NET SDK is installed (currently 11). The output exe is about 50 MB.
- Scenes: start the `scenes` preview (`.claude/launch.json`) and open
  `http://localhost:8123/scenes/<name>/#<seed>`. Changing only the `#` doesn't reload the page, so use
  `location.reload()`. Each scene exposes a global (`cityscape`, `harbour`, `cabin`, `aquarium`, `rainy`)
  with `scene`, `draw(t)`, `setup()`, `regenerate()` and spawn helpers, for poking from the console. `draw(t)`
  with a made-up `t` lets you step time forward to test timed events without waiting.
- **Never run install or uninstall tests over the user's real install.** Check
  `%LOCALAPPDATA%\Programs\HtmlScreensaver` and the HKCU/HKLM uninstall keys first. This went wrong once:
  a test uninstall wiped the user's install. Also save and restore `HKCU\Control Panel\Desktop` `SCRNSAVE.EXE` /
  `ScreenSaveActive` around any test that installs.
- Close any running `HtmlScreensaver-Setup.exe` before rebuilding, or the copy to `dist\` fails. Don't kill a
  window the user opened; ask.

## Scene conventions

Every scene follows the same pattern; copy an existing one (e.g. `harbour-dusk`) when making a new one.

- **Pixel art at low resolution.** Draw into an offscreen "art" canvas about `display.targetRows` pixels
  (200, or 240 for rainy-window) on the screen's shorter side, then scale it up with
  `imageSmoothingEnabled = false`. `k = H / 200` (or `/ 240`) scales sizes, and `ps = round(k)` is the
  sprite pixel size. Dithered gradients use a 4×4 Bayer matrix.
- **Seeded and repeatable.** The layout comes from a seed (mulberry32), taken from `display.seed`, then the URL
  `#seed`, then random, and written back to the `#`. R or a click makes a new one. Random things that happen
  over time use `Math.random()`.
- **Time-based, not frame-based.** Everything animates from `performance.now()`, so the `display.frameRate`
  cap (default 24) only changes how much work is done. Keep each frame cheap: about 1–2 ms at 1080p today.
- **Config.** Each `index.html` has a `DEFAULTS` object; `config.js` sets `window.<NAME>_CONFIG` and is
  deep-merged over it (`mergeConfig`, which warns about unknown keys). Each config section has its settings,
  then a `colors` block. Anything deleted from `config.js` falls back to the default. Keep `config.js`
  commented and its values in sync with `DEFAULTS`.
- **Clock.** Every scene has the same clock code (fonts `pixel` / `small` / `segment`, `drawClock`,
  `clockAnchor`), pasted into each `index.html`. If you change it, change it in all five. Settings live in
  each config's `clock` section: 24-hour by default, `position: 'auto'` picks a spot that suits the scene, and
  `drift` moves it a pixel or two each minute so it can't burn in.
- **Test keys** are listed in each scene's README and at the top of its `config.js`. R and click are always
  "new layout".

### Rare visitors (keep these rules)

Each scene has one rare visitor and a rarer variant of it:

| Scene | Visitor | Variant (Shift+X) |
|---|---|---|
| cityscape-night | UFO | abducts a pedestrian |
| harbour-dusk | whale spouting and diving | mother and calf |
| snowy-cabin | Santa crossing the moon | stops to drop a present down the chimney |
| aquarium | sea turtle | swims up to the surface for air |
| rainy-window | cat jumps onto the sill and watches the rain | paws at raindrops on the glass |

- **Same timing everywhere:** `RARE_CHECK_MS = 60 s` with `RARE_CHANCE = 1/30` (about once every 30 minutes
  at random), and `RARE_VARIANT = 1/10` of visits. Cityscape uses `UFO_PERIOD` / `UFO_CHANCE` /
  `ABDUCT_CHANCE` with the same values.
- **Keys:** X calls up the visitor (with the natural 1-in-10 chance of the variant), and Shift+X forces the
  variant. Spawn functions take the variant as an optional argument that defaults to the random roll.
- **Keep them secret:** rare visitors are **not** in `config.js`, and READMEs **don't name them or their keys**.
  READMEs end the description with a gentle hint such as "and, very occasionally, something else…".

### Scene README template

A title, then a paragraph describing what you see. Then `**Settings** live in [config.js](config.js): …`,
a line on the 24-hour clock, how to try it (`HtmlScreensaver-Setup.exe /debug /scene:<name>`), the `#seed`
note, and a key table. The main README stays scene-agnostic: scene details belong in the scene's own README.

## Working with the user

- Commit in logical chunks (often one per scene or feature) and only commit or push when asked. End commit
  messages with the attribution line the session provides.
- They like options run past them before bigger design choices (e.g. variants, installer approach), and
  pixel-art results checked visually in the browser before calling something done.
- British English in user-facing text ("colour").
