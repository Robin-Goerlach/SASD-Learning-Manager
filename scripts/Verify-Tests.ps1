param(
    [string]$Configuration = "Release",
    [switch]$NoBuild
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$testsRoot = Join-Path $repoRoot "tests"
$resultsRoot = Join-Path $repoRoot "artifacts\test-results"

if (-not (Test-Path $testsRoot)) {
    throw "Testverzeichnis nicht gefunden: $testsRoot"
}

if (Test-Path $resultsRoot) {
    Remove-Item $resultsRoot -Recurse -Force
}
New-Item $resultsRoot -ItemType Directory -Force | Out-Null

$projects = Get-ChildItem $testsRoot -Recurse -Filter "*.csproj" | Sort-Object FullName
if ($projects.Count -eq 0) {
    throw "Keine Testprojekte gefunden."
}

$totalTests = 0
foreach ($project in $projects) {
    $name = [System.IO.Path]::GetFileNameWithoutExtension($project.Name)
    $projectResults = Join-Path $resultsRoot $name
    New-Item $projectResults -ItemType Directory -Force | Out-Null

    $arguments = @(
        "test",
        $project.FullName,
        "-c", $Configuration,
        "--logger", "trx;LogFileName=results.trx",
        "--results-directory", $projectResults
    )

    if ($NoBuild) {
        $arguments += "--no-build"
        $arguments += "--no-restore"
    }

    Write-Host "`n=== $name ===" -ForegroundColor Cyan
    & dotnet @arguments
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet test ist für $name mit Exitcode $LASTEXITCODE fehlgeschlagen."
    }

    $trxPath = Join-Path $projectResults "results.trx"
    if (-not (Test-Path $trxPath)) {
        throw "TRX-Ergebnis für $name fehlt."
    }

    [xml]$trx = Get-Content $trxPath -Raw
    $resultNodes = $trx.SelectNodes("//*[local-name()='UnitTestResult']")
    $count = if ($null -eq $resultNodes) { 0 } else { $resultNodes.Count }
    if ($count -eq 0) {
        throw "Testprojekt $name wurde ausgeführt, aber es wurde kein Test entdeckt."
    }

    $totalTests += $count
    Write-Host "${name}: $count Test(s) entdeckt." -ForegroundColor Green
}

Write-Host "`nAlle $($projects.Count) Testprojekte enthalten entdeckte Tests. Gesamt: $totalTests." -ForegroundColor Green
