# Changelog

## [Unreleased]

### Added

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

- Projektstatus auf Milestone 0 + Milestone 1 + Milestone 2 aktualisiert.
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
