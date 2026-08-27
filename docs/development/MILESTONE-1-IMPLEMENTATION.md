# Milestone 1 – Implementation Record

**Stand:** 27. August 2026  
**Umfang:** Milestone 0 + Milestone 1

## Implementiert – Milestone 0

- Solution mit Domain/Application/Infrastructure/WinForms
- vier Testprojekte
- .NET 8 / C# 12
- Central Package Management
- Generic Host / Dependency Injection
- lokales Logging
- Single-Instance-Schutz
- lokale Datenpfade
- SQLite Connection Factory
- eingebetteter Migration Runner mit SHA-256-Checksum
- ActivityLog-Baseline
- MainForm Shell
- Architecture Tests
- GitHub Actions CI

## Implementiert – Milestone 1

### Provider

- Provider Domain Entity
- Provider Types und Lifecycle
- Anlegen / Bearbeiten
- Archivieren / Wiederherstellen
- case-insensitive Namensduplikatprüfung
- editierbare Seed-Provider

### Resources

- Canonical Resource Domain Entity
- Ressourcentyp, Status, Priorität, Schwierigkeit
- URL / normalisierte URL
- LocalPath
- Beschreibung / „Warum gespeichert?“
- Creator / Sprache / Version / Lernaufwand
- Progress
- Start-/Completion-/Archive-Zeitstempel
- Tags mit relationaler Many-to-Many-Zuordnung
- Resource Grid
- Suche und Provider-/Statusfilter
- Pagination
- Create/Edit Dialog
- URL im Standardbrowser öffnen
- Archivieren/Wiederherstellen

### Canonical Resource / Dubletten

Normalisierte URL wird fachlich geprüft. Eine mögliche Dublette löst eine Warnung aus. Ein bewusster Ausnahmefall kann trotzdem als zweiter Datensatz angelegt werden. Die Datenbank erzwingt deshalb absichtlich keinen `UNIQUE`-Constraint auf `NormalizedUrl`.

## Datenbankmigrationen

- `0001_baseline.sql`
- `0002_resource_library.sql`

Die Migrationen wurden gegen SQLite ausgeführt und auf Foreign-Key-Integrität geprüft.

## Tests

Domain:

- Resource Completion
- Progress Validation
- Archive/Restore
- URL-Scheme
- Provider Lifecycle

Application:

- URL Normalization
- Tag Normalization
- Duplicate Resource Policy
- Duplicate Override
- Archive/Restore
- Safe External Link Launch

Infrastructure:

- Migration + Seed Provider
- Resource Roundtrip
- Tag Roundtrip
- Search/Pagination

Architecture:

- verbotene Projektabhängigkeiten

## Bewusst noch nicht Teil von M1

- Quick Capture/Inbox (M2)
- Goals/Skills (M3)
- Learning Paths (M4)
- Knowledge/Evidence (M5)
- Dashboard/Search global (M6)
- Backup/Restore (M7)

## Verifikationsstatus

SQL-/Strukturprüfung: **erfolgreich**.

Ein echter .NET Compile-/Testlauf konnte in der Erstellungsumgebung nicht ausgeführt werden, weil dort kein .NET SDK installiert ist und kein externer DNS-Zugriff zum Nachinstallieren verfügbar war. Die Solution enthält deshalb zusätzlich eine Windows-GitHub-Actions-Pipeline und `BUILD-VERIFY.md` für den unmittelbaren Compiler-/Testnachweis.
