# Project Initialization Record

**Datum:** 2026-08-27  
**Status:** Milestone 0 implemented / Milestone 1 code baseline prepared

## Produktidentität

| Feld | Wert |
|---|---|
| Name | SASD Learning Manager |
| Repository | `SASD-Learning-Manager` |
| Namespace | `SASD.LearningManager` |
| Sprache | C# 12 |
| Runtime | .NET 8 |
| UI | WinForms |
| Persistenz | SQLite |

## Initialisierte Solution

```text
SASD.LearningManager.sln
src/
├── SASD.LearningManager.Domain/
├── SASD.LearningManager.Application/
├── SASD.LearningManager.Infrastructure/
└── SASD.LearningManager.WinForms/
tests/
├── SASD.LearningManager.Domain.Tests/
├── SASD.LearningManager.Application.Tests/
├── SASD.LearningManager.Infrastructure.Tests/
└── SASD.LearningManager.Architecture.Tests/
```

## M0-Baseline

Implementiert:

- Generic Host und DI
- lokales Logging
- SQLite Connection Factory
- Migration Runner mit Checksums
- MainForm Shell
- Single Instance
- Architecture Tests
- GitHub Actions CI

## Qualitätsprüfung in der Erstellungsumgebung

- Projekt-/XML-Struktur: PASS
- ProjectReference-Auflösung: PASS
- SQL Migration 0001/0002: PASS
- SQLite Foreign Keys: PASS
- SQLite `integrity_check`: PASS
- C# struktureller Delimiter-Check: PASS
- echter `dotnet build/test`: nicht in dieser Umgebung ausführbar; siehe `BUILD-VERIFY.md`

## Schemaentscheidungen M0/M1

- fachliche Enums: TEXT
- UTC-Zeitwerte: ISO-8601 TEXT
- GUID: Entity IDs
- WAL: noch nicht aktiviert; erst nach Backup-PoC in M7
- Skill-Level: wird in M3 final wirksam

**Next:** Milestone 2 – Quick Capture & Inbox nach lokalem Compiler-/CI-Nachweis des M1-Stands.
