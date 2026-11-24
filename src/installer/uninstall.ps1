# Deactivates the screensaver and removes its per-user data. Delete the package folder yourself afterwards.
# Also removes the System32 copy left by earlier versions, if present (asks for admin rights only in that case).
$ErrorActionPreference = 'Stop'

$scr = Join-Path $PSScriptRoot 'HtmlScreensaver.scr'
$reg = 'HKCU:\Control Panel\Desktop'
$current = (Get-ItemProperty $reg -ErrorAction SilentlyContinue).'SCRNSAVE.EXE'

if ($current -and ($current -like '*HtmlScreensaver.scr')) {
    Remove-ItemProperty $reg -Name 'SCRNSAVE.EXE' -ErrorAction SilentlyContinue
    Set-ItemProperty $reg -Name 'ScreenSaveActive' -Value '0'
    rundll32.exe user32.dll, UpdatePerUserSystemParameters 1, True
    Write-Host "Screensaver deactivated." -ForegroundColor Green
}

Remove-Item (Join-Path $env:LOCALAPPDATA 'HtmlScreensaver') -Recurse -Force -ErrorAction SilentlyContinue

$legacy = Join-Path $env:windir 'System32\HtmlScreensaver.scr'
if (Test-Path $legacy) {
    $isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole('Administrator')
    if (-not $isAdmin) {
        Write-Host "Removing the old System32 copy (needs admin)..."
        Start-Process powershell -Verb RunAs -ArgumentList "-NoProfile -ExecutionPolicy Bypass -Command Remove-Item '$legacy' -Force"
    } else {
        Remove-Item $legacy -Force
    }
}
Write-Host "Done."
