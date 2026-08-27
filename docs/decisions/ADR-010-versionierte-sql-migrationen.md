# ADR-010: Versionierte SQL-Migrationen

**Status:** Proposed  
**Datum:** 2026-08-27

## Context

Schema verändert sich.

## Considered Options

- manuell
- ORM Migration
- SQL Migration

## Decision

**SQL Migration**

Begründung: transparent/reproduzierbar.

## Consequences

sorgfältige Tests nötig.

## Validation

Vor der ersten davon abhängigen Implementierung wird diese ADR auf `Accepted` gesetzt oder durch eine neue Entscheidung ersetzt.

## Related

- `docs/architecture/ARCHITECTURE.md`
- `docs/requirements/Pflichtenheft.md`
