# Builds the installer: dist\HtmlScreensaver-Setup.exe
# It's a single self-contained file (no .NET install needed on the target PC) with the scenes folder
# bundled inside. Running it installs the screensaver for the current user; uninstall from
# Settings > Apps > Installed apps.
#
# Usage:  .\build.ps1                      (self-contained, recommended)
#         .\build.ps1 -FrameworkDependent  (tiny, but target PC needs the matching .NET Desktop Runtime)
param([switch]$FrameworkDependent)
$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw "No .NET SDK found. Install the latest one with:  winget install Microsoft.DotNet.SDK.11"
}

# Target whichever SDK is installed (e.g. SDK 11.0.100 -> net11.0-windows)
$major = [int]((dotnet --version).Split('.')[0])
$tfm = "net$major.0-windows"
Write-Host "SDK $(dotnet --version) -> targeting $tfm" -ForegroundColor Cyan

$name = 'HtmlScreensaver'
$proj = Join-Path $PSScriptRoot 'src\screensaver\HtmlScreensaver.csproj'
$tmp  = Join-Path $PSScriptRoot 'src\screensaver\obj\publish'
$setup = Join-Path $PSScriptRoot "dist\$name-Setup.exe"
if (-not (Test-Path $proj)) { throw "Project not found: $proj" }
# Start clean so stale restore data from an older SDK/target can't interfere
Remove-Item (Join-Path $PSScriptRoot 'dist'), (Join-Path $PSScriptRoot 'src\screensaver\obj'), (Join-Path $PSScriptRoot 'src\screensaver\bin') -Recurse -Force -ErrorAction SilentlyContinue

$props = @(
    "-p:TfmOverride=$tfm",
    '-p:PublishSingleFile=true',
    '-p:IncludeNativeLibrariesForSelfExtract=true'
)
if ($FrameworkDependent) {
    $selfContained = 'false'
} else {
    $selfContained = 'true'
    $props += '-p:EnableCompressionInSingleFile=true'
}

dotnet publish $proj -c Release -r win-x64 --self-contained $selfContained @props -o $tmp
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed" }

# The exe already carries the scenes; run under any name but *.scr it installs itself
New-Item -ItemType Directory -Force (Split-Path $setup) | Out-Null
Copy-Item (Join-Path $tmp "$name.exe") $setup

$size = [math]::Round((Get-Item $setup).Length / 1MB, 1)
Write-Host "`nInstaller: $setup  ($size MB)" -ForegroundColor Green
Write-Host "Test:      & '$setup' /quiet   (installs without prompts)"
