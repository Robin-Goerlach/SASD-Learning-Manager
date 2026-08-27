# Sicherheitsplan

**Stand:** 2026-08-27

## Scope

Lokale WinForms-App, SQLite, Import/Export, Backup/Restore, Dateireferenzen, externe URLs und optionaler Metadata Fetch.

## Assets

| Asset | C | I | A |
|---|---|---|---|
| SQLite Lernhistorie | Hoch | Sehr hoch | Hoch |
| Skillbewertungen | Hoch | Hoch | Mittel/Hoch |
| Knowledge/Notizen | Hoch | Hoch | Mittel/Hoch |
| Evidence | Mittel/Hoch | Hoch | Mittel |
| Backups | Hoch | Sehr hoch | Hoch |
| Logs | Mittel | Hoch | Mittel |

## Bedrohungen

| ID | Risiko | Maßnahme |
|---|---|---|
| SEC-001 | SQL Injection | Parameter |
| SEC-002 | Zip Slip | Canonical Path Check |
| SEC-003 | Active URI | Scheme Allowlist |
| SEC-004 | manipuliertes Importformat | Staging/Validation |
| SEC-005 | DB Corruption | Transaction/Backup/Integrity |
| SEC-006 | Datenverlust Hard Delete | Archive Default |
| SEC-007 | Sensitive Logging | Datenminimierung |
| SEC-008 | Dependency Risk | Review/Scan |
| SEC-009 | Doppelinstanz | Mutex |

## Identity

V1 nutzt Windows-Benutzerkontext; keine eigene Rollen-/Loginverwaltung.

## Secrets

Keine Provider-Secrets V1. Später nicht in Settings/Logs; Credential Manager/DPAPI separat evaluieren.

## Input Security

- http/https Allowlist
- SQL parameterisiert
- Importgrößen-/Strukturprüfung
- ZIP Traversal verhindern
- externe HTML-Daten als untrusted/plain text

## Supply Chain

Neue Dependencies auf Lizenz, CVEs, Maintenance, Transitives und Nutzen prüfen. CI später mit Dependency-/Secret-Checks.

## Privacy

Local first, keine Pflichttelemetrie, keine automatische Cloudübertragung. Externe AI erst später opt-in.

## Release Security Gate

SQL/URI/Zip/Import/Backup Tests, Dependency Review, Logging Review und Privacy Data Flow müssen vor stabiler V1 geprüft sein.
