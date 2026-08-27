# Security Policy

Das Projekt befindet sich vor der ersten Softwareversion.

## Grundsätze

- keine Provider-Passwörter V1
- keine Pflichttelemetrie
- SQL parameterisiert
- Imports/Backups als untrusted behandeln
- externe Links nur über Scheme-Allowlist
- keine eingebettete Ausführung fremder Webseiten
- keine Secrets in Logs
- Backup/Restore sicherheitstechnisch testen

## Reporting

Sicherheitsprobleme sollen zunächst dem Maintainer direkt gemeldet werden. Ein formaler öffentlicher Security-Contact wird vor einem öffentlichen stabilen Release ergänzt.

Ausführlicher Plan: `docs/security/SECURITY-PLAN.md`.
