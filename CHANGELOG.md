# Changelog

## [Unreleased]

### Added

- Milestone 4: Learning Paths mit hierarchischen Nodes
- Required/Optional Nodes und Core-Progress-Berechnung
- Goal↔LearningPath, Node↔Skill und Node↔Resource Beziehungen
- Node-Relationen (`Requires`, `AlternativeTo`, `RecommendedBefore/After`, `Deepens`, `Related`)
- Zyklenschutz bei Parent-Wechsel
- Node-Reordering und Subtree-Archivierung
- Learning-Path-TreeView, Path-/Node-Editor und Relationsdialog
- Migration `0004_learning_paths.sql`

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

### Fixed

- Milestone 4 Hotfix 001: `TestDoubles.cs` enthielt ab der Learning-Path-Testdouble-Erweiterung versehentlich literale `\n`-Escape-Sequenzen statt echter Zeilenumbrüche. Dadurch entstanden alle M4-Compilerfehler an derselben physischen Zeile 275. Der Block wurde in regulären C#-Quelltext zurückgeführt.
- `.sql`-Dateien werden über `.gitattributes` auf LF festgelegt, damit die zeilenendungsabhängigen Migration-Checksums bei Windows-Checkouts stabil bleiben.
- Milestone-ZIP-Dateien werden künftig über `.gitignore` aus dem Repository ausgeschlossen.

### Changed

- Projektstatus auf Milestone 0 bis Milestone 4 aktualisiert.
- Milestone 3 als Windows-verifiziert dokumentiert: 0 Warnungen, 0 Fehler, 48/48 Tests.
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
