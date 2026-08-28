<#
.SYNOPSIS
Removes obsolete M0 overlay files that can remain in a long-lived checkout when incremental ZIPs were applied.

.DESCRIPTION
The files listed below belong to the pre-Milestone-1 scaffold and are not part of the current
ApplicationTrackerDbContext / operational WinForms shell. The script is intentionally conservative:
it removes only exact known paths and leaves every unknown file untouched.
#>
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path

$obsolete = @(
    'src/SASD.Bewerbungsmanager.Infrastructure/DependencyInjection/ServiceCollectionExtensions.cs',
    'src/SASD.Bewerbungsmanager.Infrastructure/Persistence/ApplicationDbContext.cs',
    'src/SASD.Bewerbungsmanager.Infrastructure/Persistence/SystemMetadataRecord.cs',
    'src/SASD.Bewerbungsmanager.Infrastructure/Persistence/Migrations/20260824180000_InitialOperationalSchema.cs',
    'tests/SASD.Bewerbungsmanager.Infrastructure.Tests/SqliteMigrationTests.cs',
    'src/SASD.Bewerbungsmanager.WinForms/MainForm.cs',
    'src/SASD.Bewerbungsmanager.WinForms/Presentation/MainShell',
    'REPOSITORY-MANIFEST.md'
)

foreach ($relative in $obsolete) {
    $path = Join-Path $root $relative
    if (Test-Path -LiteralPath $path) {
        Remove-Item -LiteralPath $path -Recurse -Force
        Write-Host "Entfernt: $relative"
    }
}

Write-Host 'v0.6.0 Overlay-Cleanup abgeschlossen.'
Write-Host 'Bitte anschließend: git status; dotnet clean; dotnet build; dotnet test.'
