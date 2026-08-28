param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
Push-Location $repoRoot
try {
    dotnet clean .\SASD.Bewerbungsmanager.sln
    if ($LASTEXITCODE -ne 0) { throw "dotnet clean fehlgeschlagen." }

    dotnet restore .\SASD.Bewerbungsmanager.sln
    if ($LASTEXITCODE -ne 0) { throw "dotnet restore fehlgeschlagen." }

    dotnet build .\SASD.Bewerbungsmanager.sln -c $Configuration --no-restore
    if ($LASTEXITCODE -ne 0) { throw "dotnet build fehlgeschlagen." }

    & (Join-Path $PSScriptRoot "Verify-Tests.ps1") -Configuration $Configuration -NoBuild

    Write-Host "`nRepository Release-Gate erfolgreich." -ForegroundColor Green
}
finally {
    Pop-Location
}
