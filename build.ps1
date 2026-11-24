# Builds the redistributable package:
#   dist\HtmlScreensaver\            <- copy/zip this folder anywhere
#       HtmlScreensaver.scr          single file, self-contained (no .NET install needed on the target PC)
#       scenes\                      your scenes (one sub-folder per scene, each with an index.html)
#       install.ps1 / uninstall.ps1 / README.md
#   dist\HtmlScreensaver-win-x64.zip
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
$pkg  = Join-Path $PSScriptRoot "dist\$name"
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

# Assemble the package
New-Item -ItemType Directory -Force $pkg | Out-Null
Copy-Item (Join-Path $tmp "$name.exe") (Join-Path $pkg "$name.scr")
Copy-Item (Join-Path $PSScriptRoot 'scenes') (Join-Path $pkg 'scenes') -Recurse
foreach ($f in 'install.ps1', 'uninstall.ps1') {
    Copy-Item (Join-Path $PSScriptRoot "src\installer\$f") $pkg
}
Copy-Item (Join-Path $PSScriptRoot 'README.md') $pkg

$size = [math]::Round((Get-Item (Join-Path $pkg "$name.scr")).Length / 1MB, 1)
Write-Host "`nPackage: $pkg  ($size MB .scr)" -ForegroundColor Green
Write-Host "Zip:     $PSScriptRoot\dist\$name-win-x64.zip"
Write-Host "Test:    & '$pkg\$name.scr' /debug"
