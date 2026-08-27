# Maintenance Plan

## Ziele

Daten lesbar halten, Security-Dependencies pflegen, Migrationen/Backups reproduzierbar halten und Architekturdrift vermeiden.

## Nach Releases

Dependency Review, Migrationstest, Backup/Restore, Known Issues, Changelog, Security Findings, Dokumentlinks.

## DB

Integrity Check; VACUUM nur bewusst. DB-Größe und Indizes messen.

## Deprecation

Warnen → Migration/Export planen → Release Notes → später entfernen. Persistierte Daten nie überraschend verlieren.

## Architekturreview

Nach M4, M7 und vor 1.0: ADRs, Layering, Integrationen, Privacy-Flows und Technical Debt prüfen.
