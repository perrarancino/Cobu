# Builds the release files:  dist\CoBu-Setup.exe  (installer)  and  dist\CoBu-Portable.zip
# Usage:  powershell -ExecutionPolicy Bypass -File installer\build-installer.ps1
# Needs:  .NET 10 SDK and Inno Setup 6.3+ (https://jrsoftware.org/isinfo.php  /  winget install JRSoftware.InnoSetup)
$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$proj = Join-Path $root 'src\CoBu\CoBu.csproj'
$dist = Join-Path $root 'dist'
$publishDir = Join-Path $dist 'publish'

[xml]$csproj = Get-Content $proj
$version = ($csproj.Project.PropertyGroup | Where-Object { $_.Version } | Select-Object -First 1).Version
if (-not $version) { $version = '1.0.0' }
Write-Host "CoBu $version" -ForegroundColor Green

# 1. wizard artwork (only when missing)
if (-not (Test-Path (Join-Path $PSScriptRoot 'assets\wizard-large.bmp'))) {
    & powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'make-wizard-images.ps1')
}

# 2. clean outputs and publish (framework-dependent: the installer takes care of the .NET runtime)
if (Test-Path -LiteralPath $dist) { Get-ChildItem -LiteralPath $dist -File | Remove-Item -Force }
if (Test-Path -LiteralPath $publishDir) { Remove-Item -LiteralPath $publishDir -Recurse -Force }
dotnet publish $proj -c Release -r win-x64 --self-contained false -o $publishDir -p:DebugType=None -p:DebugSymbols=false --nologo
if ($LASTEXITCODE -ne 0) { throw 'dotnet publish failed' }

# 3. Inno Setup compiler
$candidates = @(
    "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
    "$env:ProgramFiles\Inno Setup 6\ISCC.exe",
    "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe"
)
$iscc = $candidates | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $iscc) { $cmd = Get-Command iscc -ErrorAction SilentlyContinue; if ($cmd) { $iscc = $cmd.Source } }
if (-not $iscc) { throw 'ISCC.exe not found. Install Inno Setup 6:  winget install JRSoftware.InnoSetup' }

& $iscc "/DAppVersion=$version" "/DSourceDir=$publishDir" (Join-Path $PSScriptRoot 'CoBu.iss')
if ($LASTEXITCODE -ne 0) { throw 'ISCC failed' }

# 4. portable zip (needs the .NET 10 Desktop Runtime installed)
$zip = Join-Path $dist 'CoBu-Portable.zip'
if (Test-Path -LiteralPath $zip) { Remove-Item -LiteralPath $zip -Force }
Compress-Archive -Path (Join-Path $publishDir '*') -DestinationPath $zip

Write-Host ''
Write-Host "Done:" -ForegroundColor Green
Get-ChildItem $dist -File | ForEach-Object { '  {0}  ({1:N1} MB)' -f $_.FullName, ($_.Length / 1MB) }
