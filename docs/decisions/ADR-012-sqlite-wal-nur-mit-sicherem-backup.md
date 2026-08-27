# ADR-012: SQLite WAL nur mit sicherem Backup

**Status:** Proposed  
**Datum:** 2026-08-27

## Context

WAL beeinflusst Backups.

## Considered Options

- Rollback Journal
- WAL

## Decision

**WAL nach Backup-PoC**

Begründung: gute lokale Laufzeit.

## Consequences

kein blindes File Copy.

## Validation

Vor der ersten davon abhängigen Implementierung wird diese ADR auf `Accepted` gesetzt oder durch eine neue Entscheidung ersetzt.

## Related

- `docs/architecture/ARCHITECTURE.md`
- `docs/requirements/Pflichtenheft.md`
