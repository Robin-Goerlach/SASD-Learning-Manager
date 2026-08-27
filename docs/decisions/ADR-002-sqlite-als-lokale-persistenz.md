# ADR-002: SQLite als lokale Persistenz

**Status:** Proposed  
**Datum:** 2026-08-27

## Context

Relationale Integrität wird ohne Server benötigt.

## Considered Options

- SQLite
- JSON
- SQL Server Express
- PostgreSQL
- LiteDB

## Decision

**SQLite**

Begründung: Transaktionen, Foreign Keys und sehr gute Single-User-Eignung.

## Consequences

Cloud/Multiuser ist später eigene Architektur.

## Validation

Vor der ersten davon abhängigen Implementierung wird diese ADR auf `Accepted` gesetzt oder durch eine neue Entscheidung ersetzt.

## Related

- `docs/architecture/ARCHITECTURE.md`
- `docs/requirements/Pflichtenheft.md`
