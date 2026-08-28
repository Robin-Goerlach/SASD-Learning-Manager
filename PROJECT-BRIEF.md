# Projektbrief: SASD Learning Manager

## Metadaten

| Feld | Wert |
|---|---|
| Owner | SASD / Product Owner |
| Status | Active – Implementation / Milestone 3 |
| Qualitätsstufe | Recommended; Production-Ziel für V1 |
| Profile | Core, DotNet, Desktop; Security/Operations proportional |
| Startdatum | 2026-08-27 |
| Nächster Review | vor Milestone 0 Coding Freeze |
| Zielplattform | Windows 11 |
| Primärer Nutzer | Einzelbenutzer |
| Source of Truth | lokale SQLite-Datenbank |

## Problem

Berufliche Weiterbildung verteilt sich auf viele Anbieter und Medien. Plattformbibliotheken kennen nur ihren eigenen Bestand; Bookmark-Manager kennen keine Kompetenzen; Notizsysteme steuern keine Lernpfade. Dadurch gehen Prioritäten, Voraussetzungen, Fortschritt, Evidenz und langfristige Kompetenzentwicklung verloren.

## Ziel und Nutzen

Der Learning Manager soll die Fragen beantworten:

1. Was möchte ich lernen und warum?
2. Welche Skills brauche ich?
3. Was kann ich bereits und wo besteht ein Gap?
4. Welche Ressourcen helfen wirklich?
5. Welche Resources überschneiden sich?
6. Was ist der nächste sinnvolle Schritt?
7. Welche Evidence belegt die Entwicklung?
8. Welches Wissen sollte aufgefrischt werden?
9. Wie bleibt die Lernhistorie exportierbar und sicherbar?

## Scope V1

- Goals
- Competency Areas / Topics / Skills
- Current-/Target-Level und Skill Gap
- Learning Paths mit hierarchischen Nodes
- providerunabhängige Resource Library
- Canonical Resource
- Quick Capture und Inbox
- Status, Progress, Priority und Next Action
- Tags und fachliche Relationen
- Knowledge Artifacts
- Evidence
- Skill Assessment History
- Dashboard und Suche
- Archivierung
- Backup/Restore
- lokaler/offline Kern
- exportierbare Datenbasis

## Nicht-Ziele V1

- eigener Reader oder Video-Player
- Cloud Sync
- Mehrbenutzer-/HR-Funktionen
- Provider-Passwörter und Progress-Import
- AI als Pflichtfunktion
- vollständiges Flashcard-/SRS-System
- öffentliche Community oder Kursmarktplatz
- komplexe Graphengine

## Annahmen und offene Fragen

| ID | Annahme oder Frage | Status |
|---|---|---|
| A-001 | V1 ist Single User. | Accepted |
| A-002 | Provider werden zunächst über normale URLs eingebunden. | Accepted |
| A-003 | Manuelle Progresspflege reicht für V1. | Accepted |
| A-004 | Local-first ist wichtiger als Cloud-Komfort. | Accepted |
| Q-001 | Skill-Level `null + 1..5` final bestätigen. | Open |
| Q-002 | SQLite WAL nach Backup-Test final bestätigen. | Open |
| Q-003 | FTS5 nur bei gemessenem Bedarf. | Open |
| Q-004 | Installer erst vor 1.0 entscheiden. | Open |

## Risiken

| ID | Risiko | Wkt. | Auswirkung | Maßnahme |
|---|---|---:|---:|---|
| R-001 | Scope Creep | Hoch | Hoch | V1 Scope Freeze |
| R-002 | Verwaltungsaufwand zu groß | Mittel | Hoch | Quick Capture, progressive Felder |
| R-003 | Skillmodell zu abstrakt | Mittel | Hoch | Pilot und qualitative Stufen |
| R-004 | Datenverlust bei Migration/Backup | Niedrig/Mittel | Sehr hoch | Restore-Tests |
| R-005 | Business Logic in Forms | Mittel | Hoch | Layergrenzen + Architecture Tests |
| R-006 | externe URL-/Provideränderungen | Hoch | Mittel | providerneutraler Kern |

## Meilensteine

| M | Ergebnis | Status |
|---|---|---|
| M0 | technische Baseline | Planned |
| M1 | Provider + Resource Library | Planned |
| M2 | Quick Capture + Inbox | Planned |
| M3 | Goals + Skills | Planned |
| M4 | Learning Paths | Planned |
| M5 | Knowledge + Evidence | Planned |
| M6 | Dashboard + Search | Planned |
| M7 | Backup/Restore + V1-Härtung | Planned |

## Erfolgskriterien

- [ ] dieselbe Resource kann mehrfach referenziert werden, ohne dupliziert zu werden
- [ ] Provider können in einem Path gemischt werden
- [ ] Resource Completion verändert Skill Mastery nicht
- [ ] URL kann schnell in Inbox gespeichert werden
- [ ] Skill Gap ist sichtbar
- [ ] Evidence belegt Skills
- [ ] Offline-Kern funktioniert
- [ ] Backup/Restore ist getestet
- [ ] Export verhindert Lock-in
- [ ] Build/Test reproduzierbar

## Entscheidung

**Projekt fortsetzen.** Die Produktanalyse zeigt eine sinnvolle Lücke; die V1 ist fachlich klar begrenzbar und mit einer lokalen Desktop-Architektur realistisch umsetzbar.
