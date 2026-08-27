# ADR-011: Fachliche Enums als TEXT

**Status:** Proposed  
**Datum:** 2026-08-27

## Context

Persistenz soll lesbar und stabil sein.

## Considered Options

- INTEGER
- TEXT

## Decision

**TEXT**

Begründung: Debugbarkeit und Enum-Reihenfolge unabhängig.

## Consequences

minimal mehr Speicher.

## Validation

Vor der ersten davon abhängigen Implementierung wird diese ADR auf `Accepted` gesetzt oder durch eine neue Entscheidung ersetzt.

## Related

- `docs/architecture/ARCHITECTURE.md`
- `docs/requirements/Pflichtenheft.md`
