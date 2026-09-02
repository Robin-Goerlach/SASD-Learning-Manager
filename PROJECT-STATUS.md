# Project Status – SASD Learning Manager

**Stand:** 2026-09-02

## Phase

Implementation / funktionale Alpha. Milestone 0 bis Milestone 4 sind vollständig im WinForms-Client nutzbar. Milestone 5 besitzt ein technisch integriertes Knowledge-/Evidence-Backend; die dedizierten WinForms-Arbeitsbereiche sowie die direkte Evidence-Zuordnung zu Skill Assessments stehen noch aus.

## Aktuell verifizierter Branch

`feature/import-export-review`

GitHub Actions / Windows / .NET 8:

```text
Build succeeded.
0 Warning(s)
0 Error(s)

Domain.Tests          27 passed
Application.Tests     34 passed
Infrastructure.Tests  12 passed
Architecture.Tests     4 passed
Total                 77 passed
Failed                 0
Skipped                0
```

Der verifizierte Stand umfasst den M5-Backend-Review, die Lesbarkeits-/XML-Dokumentationsbereinigung, den Ressourcen-CSV-Import/-Export und einen CI-Test, der den tatsächlich ausgelieferten Chat-Empfehlungsdatensatz vollständig durch den produktiven Import-Service laufen lässt. Maßgeblich ist GitHub-Actions-Run #21; nach den anschließenden reinen Dokumentationsupdates muss der PR-Head weiterhin denselben vollständigen CI-Gate bestehen.

## Implementiert

### M0 – Technische Baseline

- [x] .NET 8 / WinForms / SQLite
- [x] Domain / Application / Infrastructure / WinForms Layering
- [x] DI, File Logging, Single Instance
- [x] versionierte SQL-Migrationen
- [x] Windows GitHub Actions CI

### M1 – Provider & Resources

- [x] Providerverwaltung
- [x] Canonical Resource Library
- [x] Tags
- [x] Status, Priorität und Fortschritt
- [x] Suche, Filter und Paging
- [x] sichere HTTP/HTTPS-URL-Behandlung

### M2 – Quick Capture & Inbox

- [x] `Ctrl+Shift+N`
- [x] Inbox
- [x] URL-Normalisierung und Dublettenwarnung
- [x] Klassifikationsworkflow

### M3 – Goals & Skills

- [x] Goals
- [x] Competency Areas und Topics
- [x] Skills mit Current-/Target-Level und Gap
- [x] Skill Assessments mit Historie
- [x] Goal↔Skill / Taxonomie-Beziehungen

### M4 – Learning Paths

- [x] Learning Paths und Goal↔LearningPath
- [x] hierarchische Nodes
- [x] Required / Optional
- [x] Move Up / Down und Parent-Wechsel mit Zyklenschutz
- [x] Skill↔Node und Resource↔Node
- [x] Node Relations
- [x] Subtree Archive / Restore
- [x] Core Progress
- [x] TreeView Workspace
- [x] Migration `0004_learning_paths.sql`

### M5 – Knowledge & Evidence Backend

- [x] KnowledgeArtifact Domain/Application/Persistence
- [x] Markdown Content
- [x] Knowledge↔Resource/Skill/Topic/Goal/LearningPath
- [x] Evidence Domain/Application/Persistence
- [x] Evidence↔Skill/Resource/Goal
- [x] Migration `0005_knowledge_evidence.sql`
- [x] Domain-/Infrastructure-Tests
- [x] M5 Domain/Application-Code lesbar formatiert und zentrale öffentliche APIs XML-dokumentiert
- [ ] Knowledge WinForms Workspace
- [ ] Evidence WinForms Workspace
- [ ] direkte SkillAssessment↔Evidence-Zuordnung

### Portable Resource CSV

- [x] Menü `Daten`
- [x] Ressourcenimport aus CSV
- [x] Ressourcenexport nach CSV
- [x] UTF-8/BOM und RFC-4180-Quoting
- [x] vorhandene Canonical-URL-Dublettenerkennung wird benutzt
- [x] fehlende Provider werden kontrolliert angelegt
- [x] archivierte Provider werden bei Bedarf wiederhergestellt
- [x] zeilenbezogener Importbericht
- [x] CSV-Codec Regressionstests
- [x] Application-Tests für Provideranlage, Tags, URL-Dubletten und Fehlerfortsetzung
- [x] chat-basierter Testdatensatz unter `testdata/import/`
- [x] ausgelieferter Testdatensatz selbst ist CI-gesichert und wird vollständig importiert

## Offene Qualitäts-/Repository-Themen

- Das Repository enthält weiterhin versehentlich eingecheckte `SASD.Bewerbungsmanager.*`-Dateien. Diese Altlast sollte in einem separaten Cleanup-Commit entfernt werden, nicht zusammen mit fachlichen Features.
- Eine ältere Milestone-ZIP ist noch getrackt und sollte beim Repository-Cleanup entfernt werden.
- M5-WinForms fehlt noch.
- Backup/Restore, globales Dashboard/Search und Release-Hardening sind noch keine abgeschlossenen V1-Funktionen.

## Nächste empfohlene Schritte

1. Import/Export-PR nach erfolgreichem Review in `main` übernehmen.
2. Repository-Altlasten separat bereinigen.
3. M5 fertigstellen: **Wissen**, **Evidence**, Assessment↔Evidence.
4. Danach Dashboard/Search.
5. Danach Backup/Restore/Import-Paketformat und Release-Hardening.
