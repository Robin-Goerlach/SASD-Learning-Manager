# Milestone 3 – Goals & Skills

**Stand:** 2026-08-27  
**Status:** Implemented source code – Windows build/test verification pending

## Ziel

Milestone 3 erweitert den bisherigen Resource-/Inbox-Manager zum eigentlichen Learning Manager. Lernziele werden mit expliziten Skills verbunden; Skills besitzen ein separates Current-/Target-Level und append-only Assessment History.

## Implementiert

### Goals

- Goal Domain Entity
- GoalType: Learning, Career, Certification, Project, Interest, Other
- GoalStatus: Planned, Active, Paused, Achieved, Archived
- GoalPriority
- Zieldatum
- Motivation/Beschreibung
- Next Action + Due Date
- Goal ↔ Skill many-to-many
- Archivieren/Wiederherstellen
- Suche, Statusfilter, Paging
- WinForms Goals Workspace

### Competency Catalog

- Competency Areas
- Topics
- CompetencyArea ↔ Topic many-to-many
- Active/Inactive/Archived
- Name-Dublettenprüfung
- Archivieren/Wiederherstellen
- Katalogdialog mit Bereich-/Topic-Verwaltung

### Skills

- Skill Domain Entity
- CurrentLevel und TargetLevel getrennt
- qualitative Skala 1..5; `null` = noch nicht bewertet
- Skill Gap
- Skill ↔ CompetencyArea
- Skill ↔ Topic
- Active/Inactive/Archived
- Suche, Paging und Archivfilter
- WinForms Skills Workspace

### Skill Assessments

- immutable/append-only SkillAssessment
- AssessmentType
- Begründung
- Assessment-Historie
- expliziter „Bewerten …“-Dialog
- CurrentLevel-Snapshot wird mit Assessment in derselben SQLite-Transaktion aktualisiert

## Zentrale fachliche Invariante

```text
Resource Completed
       ✕
       └──── setzt KEIN Skill-Level

Skill Assessment
       ↓
CurrentLevel
```

Goal↔Skill bedeutet ebenfalls nur „dieser Skill wird für dieses Ziel benötigt“. Die Verknüpfung verändert keinen Kompetenzstand.

## Datenbank

Neue Migration:

```text
0003_goals_skills.sql
```

Neue Tabellen:

- CompetencyAreas
- Topics
- CompetencyAreaTopics
- Skills
- SkillAssessments
- CompetencyAreaSkills
- TopicSkills
- Goals
- GoalSkills

M1-/M2-Migrationen wurden **nicht verändert**, damit bereits angewandte Migration-Checksums gültig bleiben.

## Tests

Gesamter Testbestand nach M3:

- Domain: 17 Facts
- Application: 19 Facts
- Infrastructure: 8 Facts
- Architecture: 4 Facts
- **Gesamt: 48 Facts**

Neue Tests prüfen u. a.:

- Skill Gap
- Level-Grenzen
- Assessment-Historie
- Current-Level-Snapshot
- Goal↔Skill ohne Mastery-Seiteneffekt
- Taxonomiebeziehungen
- archivierte historische Beziehungen
- M3 Repository Roundtrip
- M3 Search Projection
- Foreign Keys

## Manuelle M3-Smoke-Route

1. Anwendung starten.
2. `Skills` öffnen.
3. `Kompetenzkatalog …` öffnen.
4. Bereich `Linux` anlegen.
5. Topic `systemd` anlegen und Linux zuordnen.
6. Skill `Services diagnostizieren` mit Ziel-Level 4 anlegen.
7. Skill über `Bewerten …` auf Level 2 bewerten.
8. Prüfen: Grid zeigt `Ist 2`, `Ziel 4`, `Gap +2`.
9. `Ziele` öffnen.
10. Ziel `Linux vertiefen` anlegen und Skill verknüpfen.
11. Prüfen: Skill-Level bleibt 2.
12. App schließen/starten und Persistenz prüfen.
