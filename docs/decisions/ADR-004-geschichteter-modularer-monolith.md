# ADR-004: Geschichteter modularer Monolith

**Status:** Proposed  
**Datum:** 2026-08-27

## Context

Mehrere Fachmodule, aber ein User/Deployment.

## Considered Options

- ungegliederter Monolith
- modularer Monolith
- Microservices

## Decision

**Modularer Monolith**

Begründung: klare Grenzen ohne operative Verteilung.

## Consequences

Module sind keine Deploymenteinheiten.

## Validation

Vor der ersten davon abhängigen Implementierung wird diese ADR auf `Accepted` gesetzt oder durch eine neue Entscheidung ersetzt.

## Related

- `docs/architecture/ARCHITECTURE.md`
- `docs/requirements/Pflichtenheft.md`
