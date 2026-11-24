# Activates the screensaver from where it sits (no copying, no admin rights needed).
# The scenes folder stays next to the .scr, so keep the package folder somewhere permanent.
# Set the wait time in Windows' Screen Saver Settings.
$ErrorActionPreference = 'Stop'

$scr = Join-Path $PSScriptRoot 'HtmlScreensaver.scr'
if (-not (Test-Path $scr)) { throw "HtmlScreensaver.scr not found next to this script." }
if (-not (Get-ChildItem (Join-Path $PSScriptRoot 'scenes') -Directory -ErrorAction SilentlyContinue |
        Where-Object { Test-Path (Join-Path $_.FullName 'index.html') })) {
    Write-Warning "No scenes found yet. Add scenes\<name>\index.html before the screensaver triggers."
}

$reg = 'HKCU:\Control Panel\Desktop'
Set-ItemProperty $reg -Name 'SCRNSAVE.EXE'    -Value $scr
Set-ItemProperty $reg -Name 'ScreenSaveActive' -Value '1'

# Ask Windows to re-read the setting now
rundll32.exe user32.dll, UpdatePerUserSystemParameters 1, True

$t = [int](Get-ItemProperty $reg).ScreenSaveTimeOut / 60
Write-Host "Activated: $scr" -ForegroundColor Green
Write-Host "Wait time: $t minute(s) (change it in Screen Saver Settings)"
