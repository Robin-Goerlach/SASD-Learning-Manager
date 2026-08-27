# Project Initialization Record

**Datum:** 2026-08-27  
**Status:** Prepared / vor Code-Repository

## Produktidentität

| Feld | Wert |
|---|---|
| Name | SASD Learning Manager |
| Repository | `SASD-Learning-Manager` |
| Namespace | `SASD.LearningManager` |
| Sprache | C# |
| Runtime | .NET 8 |
| UI | WinForms |
| Persistenz | SQLite |

## Solution

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

## M0 Quality Gate

```text
restore → success
build Release → 0 errors
unit/integration tests → green
MainForm → starts
SQLite DB → created
Migration 0001 → applied
Architecture Tests → green
```

## Vor Schema Freeze zu akzeptieren

1. `null + 1..5` Skill-Level.
2. fachliche Enums als TEXT.
3. UTC ISO-8601 TEXT.
4. GUID als Entity ID.
5. WAL erst nach Backup-PoC.

**Ready for Milestone 0 after documentation review.**
