# ADR-003: Explizites SQL und spezialisierte Repositories

**Status:** Proposed  
**Datum:** 2026-08-27

## Context

Persistenz soll transparent bleiben.

## Considered Options

- EF Core
- Dapper
- Microsoft.Data.Sqlite + SQL

## Decision

**Explizites SQL**

Begründung: kontrollierbare Queries und geringe Abhängigkeiten.

## Consequences

mehr Mapping-Code, durch Tests beherrschbar.

## Validation

Vor der ersten davon abhängigen Implementierung wird diese ADR auf `Accepted` gesetzt oder durch eine neue Entscheidung ersetzt.

## Related

- `docs/architecture/ARCHITECTURE.md`
- `docs/requirements/Pflichtenheft.md`
