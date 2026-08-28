param(
    [string]$Version = "0.7.0",
    [string]$Runtime = "win-x64",
    [bool]$SelfContained = $true,
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$project = Join-Path $repoRoot "src\SASD.Bewerbungsmanager.WinForms\SASD.Bewerbungsmanager.WinForms.csproj"
$releaseRoot = Join-Path $repoRoot "artifacts\release\$Version"
$publishDir = Join-Path $releaseRoot "publish"
$zipPath = Join-Path $releaseRoot "SASD-Bewerbungsmanager-$Version-$Runtime.zip"
$hashPath = "$zipPath.sha256"

if (Test-Path $releaseRoot) {
    Remove-Item $releaseRoot -Recurse -Force
}
New-Item $publishDir -ItemType Directory -Force | Out-Null

Write-Host "Restore für $Runtime ..." -ForegroundColor Cyan
& dotnet restore $project -r $Runtime
if ($LASTEXITCODE -ne 0) {
    throw "dotnet restore ist fehlgeschlagen."
}

Write-Host "Publish $Version ($Runtime) ..." -ForegroundColor Cyan
& dotnet publish $project `
    -c $Configuration `
    -r $Runtime `
    --self-contained $SelfContained `
    --no-restore `
    -o $publishDir `
    -p:PublishSingleFile=false `
    -p:PublishTrimmed=false `
    -p:DebugSymbols=false `
    -p:DebugType=None
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish ist fehlgeschlagen."
}

$readme = Join-Path $repoRoot "README.md"
if (Test-Path $readme) {
    Copy-Item $readme (Join-Path $publishDir "README.md")
}

Compress-Archive -Path (Join-Path $publishDir "*") -DestinationPath $zipPath -CompressionLevel Optimal
$hash = (Get-FileHash $zipPath -Algorithm SHA256).Hash.ToLowerInvariant()
"$hash  $([System.IO.Path]::GetFileName($zipPath))" | Set-Content $hashPath -Encoding ascii

Write-Host "`nReleasepaket erzeugt:" -ForegroundColor Green
Write-Host $zipPath
Write-Host "SHA-256: $hash"
Write-Host "Hinweis: Dieses Skript signiert keine Binärdateien und erzeugt noch keinen Installer."
