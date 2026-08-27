# Project Status – SASD Learning Manager

**Stand:** 2026-08-27

## Phase

Implementation. **Milestone 0, Milestone 1 und Milestone 2 sind als Code-Stand umgesetzt.**

## Implementiert

- [x] Solution / Layered Modular Monolith
- [x] Domain / Application / Infrastructure / WinForms
- [x] SQLite + Migration Runner
- [x] Logging / DI / Generic Host
- [x] Single Instance
- [x] Providerverwaltung
- [x] Resource Library
- [x] Tags
- [x] Search / Filter / Paging
- [x] URL-Dublettenwarnung
- [x] Archive / Restore
- [x] Quick Capture
- [x] `Ctrl+Shift+N`
- [x] dedizierte Inbox
- [x] Inbox-Klassifikation
- [x] Domain/Application/Infrastructure/Architecture Tests
- [x] GitHub Actions CI

## Build-Korrekturen aus den Windows-Tests

- [x] Namespace-Konflikt bei `Application.Run` korrigiert
- [x] xUnit1051 durch `TestContext.Current.CancellationToken` korrigiert
- [x] xUnit2017 in `ResourceServiceTests.cs` korrigiert: `Assert.True(collection.Contains(...))` wurde durch die passende `Assert.Contains(..., comparer)`-Assertion ersetzt.

Der Windows-Build vom 27.08.2026 erreichte bereits erfolgreich Domain, Application, Infrastructure, WinForms sowie die übrigen Testassemblies; der gemeldete Abbruch lag ausschließlich an diesem Analyzerfehler im Application-Testprojekt. Ein erneuter vollständiger Build/Test-Lauf ist nach dem Hotfix weiterhin erforderlich.

## Lokale Verifikation in der Erstellungsumgebung

- SQLite-Migrationen: **PASS**
- M2 Inbox Query gegen SQLite: **PASS**
- Foreign-Key-Check: **PASS**
- SQLite Integrity Check: **PASS**
- Projekt-/XML-/Referenzstruktur: geprüft
- echter .NET-Build: in dieser Umgebung weiterhin nicht möglich (kein SDK / kein DNS zur Nachinstallation)

## Als Nächstes

Milestone 3: **Goals & Skills**.
