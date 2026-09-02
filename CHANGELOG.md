# Changelog

## [Unreleased]

### Added

- Milestone 5 Backend für Markdown-basierte Knowledge Artifacts.
- Knowledge-Beziehungen zu Resources, Skills, Topics, Goals und Learning Paths.
- Evidence Backend mit Typ, Zeitpunkt, URL/LocalPath, Evaluation und Archivstatus.
- Evidence-Beziehungen zu Skills, Resources und Goals.
- Migration `0005_knowledge_evidence.sql`.
- Menü `Daten` mit Ressourcen-Import und -Export als portable CSV-Datei.
- dependency-free CSV-Codec mit Quoting, Escaped Quotes, Kommas und eingebetteten Zeilenumbrüchen.
- CSV-Importbericht mit zeilenbezogenen Diagnosen und Canonical-URL-Dublettenerkennung.
- automatische Anlage fehlender Provider und Wiederherstellung referenzierter archivierter Provider beim CSV-Import.
- chat-basierter Import-Testdatensatz unter `testdata/import/resources-chat-recommendations.csv`.
- Benutzeranleitung `docs/user/RESOURCE-CSV-IMPORT-EXPORT.md`.
- zusätzliche Application-Tests für CSV-Codec und Importverhalten.

- Milestone 4: Learning Paths mit hierarchischen Nodes.
- Required/Optional Nodes und Core-Progress-Berechnung.
- Goal↔LearningPath, Node↔Skill und Node↔Resource Beziehungen.
- Node-Relationen (`Requires`, `AlternativeTo`, `RecommendedBefore/After`, `Deepens`, `Related`).
- Zyklenschutz bei Parent-Wechsel.
- Node-Reordering und Subtree-Archivierung.
- Learning-Path-TreeView, Path-/Node-Editor und Relationsdialog.
- Migration `0004_learning_paths.sql`.

- Milestone 3: Goals mit Typ, Status, Priorität, Zieldatum und Next Action.
- Competency Areas und Topics inklusive many-to-many-Zuordnung.
- Skills mit Current-/Target-Level und Skill Gap.
- append-only Skill Assessment History.
- Goal↔Skill, Skill↔Area und Skill↔Topic Beziehungen.
- Goals- und Skills-Workspaces sowie Kompetenzkatalog-Dialog.
- Migration `0003_goals_skills.sql`.

- komplette Research-/Requirements-/Architecture-Baseline.
- .NET-8-Solution mit Domain/Application/Infrastructure/WinForms.
- SQLite Connection Factory und versionierter Migration Runner.
- Provider Domain/CRUD/Archivierung.
- Canonical Resource Domain.
- Resource Library mit Suche, Filter und Paging.
- Resource Editor mit Tags, URL, LocalPath, Progress, Status und Priorität.
- URL-Normalisierung und Dublettenwarnung.
- sicheres Öffnen von HTTP/HTTPS-Links.
- lokales File Logging.
- Single-Instance-Schutz.
- Domain-, Application-, Infrastructure- und Architecture-Tests.
- Windows GitHub Actions CI.
- Quick Capture mit `Ctrl+Shift+N`.
- dedizierte Inbox mit Suche und Paging.
- Inbox-Klassifikationsworkflow.
- expliziter Dublettenentscheidungsdialog.

### Fixed

- M5 Knowledge/Evidence Services verwendeten nach dem Lesbarkeits-Refactoring zunächst den falschen Named-Argument-Namen `isNew`; GitHub Actions deckte dies auf und die Aufrufe wurden an den Repository-Port `insert` angepasst.
- Chat-basierte CSV-Testdaten auf exakt 16 Spalten des produktiven Importformats korrigiert.
- Milestone 4 Hotfix 001: `TestDoubles.cs` enthielt ab der Learning-Path-Testdouble-Erweiterung versehentlich literale `\n`-Escape-Sequenzen statt echter Zeilenumbrüche. Dadurch entstanden alle M4-Compilerfehler an derselben physischen Zeile 275. Der Block wurde in regulären C#-Quelltext zurückgeführt.
- `.sql`-Dateien werden über `.gitattributes` auf LF festgelegt, damit die zeilenendungsabhängigen Migration-Checksums bei Windows-Checkouts stabil bleiben.
- Milestone-ZIP-Dateien werden künftig über `.gitignore` aus dem Repository ausgeschlossen.

### Changed

- M5 Knowledge/Evidence Domain-, DTO- und Servicecode lesbar formatiert und mit XML-Dokumentation ergänzt.
- Projektstatus und README mit dem tatsächlichen M5-Backend-Stand synchronisiert.
- CSV-Transfer läuft bewusst über bestehende Application Services statt direkter SQLite-Manipulation.
- M5 Knowledge/Evidence WinForms-Workspaces und direkte SkillAssessment↔Evidence-Zuordnung bleiben als offene M5-Arbeit dokumentiert.

### Verification

Der Import/Export-Review-Branch wurde am 02.09.2026 in GitHub Actions auf Windows erfolgreich gebaut und getestet:

```text
Build succeeded.
0 Warning(s)
0 Error(s)

Domain.Tests          27 passed
Application.Tests     33 passed
Infrastructure.Tests  12 passed
Architecture.Tests     4 passed
Total                 76 passed
```

### Security

- SQL ausschließlich parameterisiert.
- lokale Persistenz und keine Provider-Credentials.
- URL-Scheme auf HTTP/HTTPS begrenzt.
- Foreign Keys aktiviert.
- CSV-Import verwendet Domain/Application-Validierung statt direkter Datenbank-Inserts.

## Releases

Noch keine Software-Releases.
