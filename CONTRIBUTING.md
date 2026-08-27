# Contributing

## Vor jeder Änderung

1. Requirement-ID bestimmen.
2. fachlichen Owner/Modul bestimmen.
3. Datenmodell-/Migrationseffekt prüfen.
4. ADR-Bedarf prüfen.
5. Tests definieren.
6. Security-/Privacy-Effekt prüfen.
7. Backup/Export-Auswirkung prüfen.

## Codeprinzipien

- Code englisch, UI deutsch.
- XML-Kommentare für öffentliche fachliche APIs.
- Business Logic nicht in Forms.
- SQL nur in Infrastructure.
- Queries parameterisiert.
- Archive vor Hard Delete.
- neue Dependency nur nach Nutzen-, Lizenz- und Security-Prüfung.

## PR-Inhalt

- Problem/Ziel
- Requirement IDs
- Lösung
- Tests
- Migration/Backup-Auswirkung
- UI-Screenshot bei UI-Änderung
- ADR-Link bei Architekturänderung
