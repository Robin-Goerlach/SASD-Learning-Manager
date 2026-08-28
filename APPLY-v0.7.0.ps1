<#
.SYNOPSIS
Conservatively removes known obsolete overlay files before the v0.7.0 release-gate run.

.DESCRIPTION
Incremental ZIP deliveries do not delete files that already exist in a long-lived Git checkout.
This script therefore removes only exact paths that are known to belong to the old M0 scaffold or
the accidental FinanceControl overlay. Unknown files are never touched.
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
    'REPOSITORY-MANIFEST.md',
    'APPLY-MILESTONE-9.md',
    'MILESTONE-9-MANIFEST.md',
    'docs/architecture/MILESTONE-9-IMPLEMENTATION.md',
    'docs/data-model/MILESTONE-9-SQLITE-SCHEMA.md',
    'docs/roadmap/060_Roadmap.md',
    'src/Sasd.FinanceControl.App',
    'src/Sasd.FinanceControl.Application',
    'src/Sasd.FinanceControl.Domain',
    'src/Sasd.FinanceControl.Infrastructure',
    'tests/Sasd.FinanceControl.App.Tests',
    'tests/Sasd.FinanceControl.Application.Tests',
    'tests/Sasd.FinanceControl.Domain.Tests',
    'tests/Sasd.FinanceControl.Infrastructure.Tests'
)

foreach ($relative in $obsolete) {
    $path = Join-Path $root $relative
    if (Test-Path -LiteralPath $path) {
        Remove-Item -LiteralPath $path -Recurse -Force
        Write-Host "Entfernt: $relative"
    }
}

Write-Host 'v0.7.0 Overlay-Cleanup abgeschlossen.' -ForegroundColor Green
Write-Host 'Nächster Schritt: .\scripts\Invoke-ReleaseGate.ps1'
