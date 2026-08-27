# SASD Core Compliance – SASD Learning Manager

**Stand:** 2026-08-27  
**Status:** M0/M1 Implementation Baseline

## Definition

- [x] Project Brief
- [x] Scope/Non-Goals
- [x] Requirements
- [x] Architecture
- [x] Roadmap
- [x] Risks

## Repository/Build

- [x] Solution- und Repositorystruktur vorbereitet
- [x] Restore-/Build-/Testbefehle dokumentiert
- [x] GitHub Actions CI definiert
- [x] ProjectReferences strukturell verifiziert
- [ ] realer `dotnet restore` in der Erstellungsumgebung
- [ ] realer Release-Build in der Erstellungsumgebung
- [ ] reale xUnit-Ausführung in der Erstellungsumgebung

Die drei offenen Punkte sind keine behaupteten Fehlschläge: In der verwendeten Erstellungsumgebung ist kein .NET SDK installiert. `BUILD-VERIFY.md` und die Windows-CI erzeugen den echten Nachweis.

## Quality

- [x] Teststrategie
- [x] Domain/Application/Infrastructure/Architecture-Testcode
- [x] Traceability vorbereitet
- [x] SQL-Migrationen praktisch gegen SQLite ausgeführt
- [x] Foreign-Key-/Integrity-Check
- [ ] Compiler-/Test-Evidence
- [ ] Pilot Evidence

## Security

- [x] Security Plan
- [x] Privacy Flow
- [x] SQL parameterisiert
- [x] HTTP/HTTPS-Scheme-Grenze
- [x] keine Provider-Credentials
- [ ] vollständige Security-Testausführung via .NET Test Runner

## Maintenance

- [x] Maintenance Plan
- [x] versionierte Migrationen
- [x] Migrationschecksums
- [ ] Backup/Restore-Implementierung (M7)
- [ ] reale Upgrade-Evidenz

**Bewertung:** Die M0/M1-Codebaseline ist strukturell und auf SQLite-Ebene geprüft. Der Compiler-/xUnit-Nachweis ist der nächste technische Gate.
