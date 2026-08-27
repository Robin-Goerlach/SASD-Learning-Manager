# UI/UX Specification

**Stand:** 2026-08-27

## Ziel

Die Anwendung soll Komplexität ordnen, ohne Verwaltungsfriktion zu erzeugen.

## Main Window

```text
┌─────────────────────────────────────────────────────────────┐
│ SASD Learning Manager      [Suche] [+ Ressource] [Backup]  │
├───────────────┬─────────────────────────────────────────────┤
│ Heute         │                                             │
│ Ziele         │              Arbeitsbereich                 │
│ Lernpfade     │                                             │
│ Skills        │                                             │
│ Ressourcen    │                                             │
│ Inbox         │                                             │
│ Wissen        │                                             │
│ Evidence      │                                             │
│ Suche         │                                             │
│ Datenpflege   │                                             │
│ Einstellungen │                                             │
├───────────────┴─────────────────────────────────────────────┤
│ DB | letztes Backup | Offline-fähig | Status               │
└─────────────────────────────────────────────────────────────┘
```

## Dashboard

Zeigt aktive Ziele, Paths, Resources in Arbeit, Inbox, „Als Nächstes“, Skill Gaps, aktive Paths und zuletzt bearbeitete Resources.

## Quick Capture

Pflicht nur URL oder Titel. Provider, Skill, Tags und Path dürfen später folgen. Ziel: typischer Capture deutlich unter einer Minute.

## Resource Library

Grid: Titel, Provider, Typ, Status, Progress, Priority, Difficulty. Filter kombinierbar. Detailtabs: Übersicht, Lernstatus, Zuordnungen, Beziehungen, Wissen, Evidence, Historie.

## Skills

Grid: Skill, Ist, Ziel, Gap, letzte Bewertung, Review. Assessmentdialog zeigt alte/neue Stufe, qualitative Bedeutung, Begründung und Evidence.

## Learning Path

V1 TreeView + Detailpanel. Alle Kernaktionen müssen ohne Drag & Drop funktionieren.

## Completion / Mastery / Retention

Resource 100 % ist sichtbar getrennt von Skill 3/5 und „zuletzt vor 8 Monaten genutzt“.

## Accessibility

- Tastatur
- logische TabOrder
- AccessibleName/Description
- DPI 100/125/150/200 %
- Status nicht nur Farbe
- Tree keyboard-bedienbar

## Shortcuts

`Ctrl+N`, `Ctrl+Shift+N`, `Ctrl+F`, `Ctrl+S`, `Esc`.

## Fehler

Validierung feldnah; unerwartete Fehler mit Error ID statt roher Exception.

## Visuelle Referenz

`assets/sasd-learning-manager-dashboard.png` ist Richtungs-Mockup, keine Pixelvorgabe.
