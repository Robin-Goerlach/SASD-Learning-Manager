# Changelog

## [Unreleased]

### Added

- Milestone 3: Goals mit Typ, Status, Priorität, Zieldatum und Next Action
- Competency Areas und Topics inklusive many-to-many-Zuordnung
- Skills mit Current-/Target-Level und Skill Gap
- append-only Skill Assessment History
- Goal↔Skill, Skill↔Area und Skill↔Topic Beziehungen
- Goals- und Skills-Workspaces sowie Kompetenzkatalog-Dialog
- Migration `0003_goals_skills.sql`

- komplette Research-/Requirements-/Architecture-Baseline
- .NET-8-Solution mit Domain/Application/Infrastructure/WinForms
- SQLite Connection Factory und versionierter Migration Runner
- Provider Domain/CRUD/Archivierung
- Canonical Resource Domain
- Resource Library mit Suche, Filter und Paging
- Resource Editor mit Tags, URL, LocalPath, Progress, Status und Priorität
- URL-Normalisierung und Dublettenwarnung
- sicheres Öffnen von HTTP/HTTPS-Links
- lokales File Logging
- Single-Instance-Schutz
- Domain-, Application-, Infrastructure- und Architecture-Tests
- Windows GitHub Actions CI
- Quick Capture mit `Ctrl+Shift+N`
- dedizierte Inbox mit Suche und Paging
- Inbox-Klassifikationsworkflow
- expliziter Dublettenentscheidungsdialog

### Changed

- Projektstatus auf Milestone 0 bis Milestone 3 aktualisiert.
- M1 Buildfehler `Application.Run` und xUnit1051 korrigiert.
- M2 Hotfix: xUnit2017 in `ResourceServiceTests` durch die spezialisierte `Assert.Contains`-Assertion behoben.
- URL-Dubletten sind bewusst eine fachliche Warnung statt eines harten `UNIQUE`-Constraints.

### Security

- SQL ausschließlich parameterisiert.
- lokale Persistenz und keine Provider-Credentials.
- URL-Scheme auf HTTP/HTTPS begrenzt.
- Foreign Keys aktiviert.

## Releases

Noch keine Software-Releases.
