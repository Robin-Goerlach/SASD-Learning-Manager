# SASD Learning Manager – Roadmap

**Stand:** 2026-08-27

> Diese Roadmap beschreibt Richtung und Reihenfolge, keine Terminversprechen. Kalenderdaten werden erst gesetzt, wenn reale Kapazität und Planung vorliegen.

## Current Objective

Eine erste produktiv nutzbare Version entwickeln, die den vollständigen persönlichen Weiterbildungsworkflow providerunabhängig abbildet und die Lernhistorie lokal, nachvollziehbar und sicher verwaltet.

```text
Goal → Skill / Target Level → Learning Path → Resource
→ Progress → Knowledge / Evidence → Skill Assessment
→ Backup / Restore
```

## Planungsprinzipien

1. V1 Scope Freeze.
2. Jeder Milestone liefert testbaren Produktwert.
3. Data Safety before Comfort.
4. Exit Criteria statt erfundener Termine.
5. Evidence before Claims.
6. keine unnötigen Frameworks.
7. Pilot vor 1.0.

## M0 – Technical Baseline

**Ergebnis:** Solution, DI/Host, Logging, SQLite, Migration Runner, MainForm, CI, Tests, Architecture Tests.

**Stop:** Restore/Build/Test reproduzierbar; DB-Migration 0001 läuft.

## M1 – Resource Library

**Ergebnis:** Provider, Resource CRUD, Tags, Status, Progress, Archive, Canonical Resource.

**Stop:** reale Resource kann vollständig verwaltet werden.

## M2 – Quick Capture & Inbox

**Ergebnis:** Quick Capture, URL Normalization, Duplicate Warning, Inbox, Klassifikation.

**Stop:** gefundene URL ist in wenigen Schritten gesichert.

## M3 – Goals & Skills

**Ergebnis:** Goals, Competency Areas, Topics, Skills, Current/Target, Assessment History, Gap.

**Stop:** Benutzer kann Ziel und Kompetenzlücke nachvollziehbar abbilden.

## M4 – Learning Paths

**Ergebnis:** Tree Nodes, Required/Optional, Reihenfolge, Skills, Resources, Relations, Progress.

**Stop:** providerübergreifender echter Lernpfad funktioniert.

## M5 – Knowledge & Evidence

**Ergebnis:** Markdown Knowledge, Evidence, Assessments mit Begründung.

**Stop:** Course Completion und Skill Mastery bleiben nachweislich getrennt.

## M6 – Dashboard & Search

**Ergebnis:** Dashboard, globale Suche, Filter, Skill Gaps, „Als Nächstes“, Maintenance Views, Pagination.

**Stop:** auch bei realistischem Bestand bleibt Arbeit handlungsorientiert.

## M7 – Reliability & V1 Hardening

**Ergebnis:** Backup/Restore, Integrity, Migration, Performance, Security, DPI/Accessibility, Pilotbasis.

**Stop:** keine bekannten datenverlustkritischen Defekte; Backup/Restore und Upgrade sind nachgewiesen.

## V1.x

- Learning Needs
- Saved Views
- FTS5 bei Bedarf
- Sessions/Lernzeit
- Review/Retention
- Metadatenkomfort
- JSON/CSV/Markdown Import/Export
- persönliche Reports
- Zertifikatsablauf
- Batch Operations

## V2

- Browser Extension
- optionale Provider APIs
- Broken-Link-Prüfung
- Webarchivierung
- visuelle Graphansicht
- Anki/RemNote
- Kalender
- AI Suggestions/Summary/Overlap
- semantische Suche

## Later

- Cloud Sync
- mobile App
- Team/HR
- öffentliche Pfadbibliothek
- Social/LMS/Marketplace
- eigener Reader
