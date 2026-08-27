# Teststrategie

**Stand:** 2026-08-27

## Ziel

Schutz vor Datenverlust, Relationenkorruption, falscher Skill-/Progress-Logik, Migration-/Restore-Fehlern und Security-Problemen.

## Risiko → Test

| Risiko | Test |
|---|---|
| Resource-Dublette | Integration |
| Completion setzt Mastery | Unit/Application |
| Path-Zyklus | Domain/Application |
| Migration verliert Daten | Migration Integration |
| Restore unvollständig | E2E Integration |
| SQL Injection | Security Integration |
| Zip Slip | Security |
| DPI-Probleme | manuell |
| langsame Search | Performance |

## Testarten

- Domain Unit
- Application Use Case
- Infrastructure Integration mit echter temp SQLite DB
- Architecture Tests
- System/E2E
- UI Smoke/Explorativ
- Security
- Migration/Recovery
- Performance

## Kritische Domain Tests

Statusübergänge, Progress 0..100, Skill-Level, Gap, Tree-Zyklen, Required/Optional Progress, Relations, Completion/Mastery, Archive.

## Infrastructure

Migration, FK, CRUD, Joins, Pagination, Filter, Backup/Restore, Import/Export, Integrity.

## Security Cases

SQL-Metazeichen, `javascript:`, `data:`, lange URL, manipuliertes JSON, ZIP `../`, falscher Hash, beschädigte DB, HTTP Timeout, Redirect Loop, große Response, fehlende Datei.

## Traceability

Kritische Tests referenzieren Requirement IDs, z. B. `[Trait("Requirement", "REQ-F-ACT-011")]`.

## Release Exit

- Release Build 0 Errors
- Ziel 0 Warnings
- Pflicht-Tests grün
- Migration grün
- Backup/Restore grün
- Offline-Smoke
- keine offenen kritischen Security Findings
- DPI/Accessibility durchgeführt
