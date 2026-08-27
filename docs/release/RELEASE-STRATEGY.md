# Release Strategy

## Versionierung

Semantic Versioning. Richtung: 0.1.x Milestones, 0.5.x kompletter Kern, 0.7.x Pilot/Hardening, 0.9.0 RC Baseline, 1.0.0 stabile V1.

## Gate

Clean source state, Build/Test, Migration, Changelog, Security Findings, Known Issues; ab Pilot zusätzlich Backup/Restore und Smoke Test.

## Pre-1.0

Breaking Changes möglich, aber Pilotdaten möglichst migrieren.

## Ab 1.0

Schemaänderungen benötigen getesteten Migrationspfad.

## RC

Realer Pilot, keine kritischen Datenverlustbugs, Backup/Restore, DPI/Accessibility, Security Review und Dokumentationswalkthrough.

Release Records werden erst für reale Releases erstellt.
