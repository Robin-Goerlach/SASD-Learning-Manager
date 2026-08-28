# SASD Learning Manager

> Persönlicher, anbieterunabhängiger Learning-Portfolio- und Competency-Manager für strukturierte Weiterbildung.

**Status:** Milestone 0 bis Milestone 4 – implementierter Code-Stand (M4 Hotfix 001)  
**Stand:** 2026-08-28  
**Zielplattform:** Windows 11  
**Technologie:** C# / .NET 8 / Windows Forms / SQLite  
**Entwicklungsstandard:** SASD Development Standard

## GUI-Konzept

![SASD Learning Manager – Dashboard Mockup](assets/sasd-learning-manager-dashboard.png)

> Der Screenshot ist ein realistisches UI-Konzept für die geplante WinForms-Anwendung. Er ist eine visuelle Referenz, keine pixelgenaue Implementierungsvorgabe.

## Produktidee

Der SASD Learning Manager verbindet Lernziele, Kompetenzen und Skills mit strukturierten Learning Paths und beliebigen externen Lernressourcen. Er dokumentiert Lernfortschritt, Knowledge Artifacts, Evidence und Kompetenzentwicklung, ohne an einen einzelnen Kursanbieter gebunden zu sein.

```text
Goal
  ↓
Competency / Skill
  ↓
Learning Path
  ↓
Resource
  ↓
Learning Activity
  ↓
Knowledge / Evidence
  ↓
Skill Assessment
  ↓
Review / Retention
```

## Produktprinzipien

- **Skill before Course** – ein Kurs ist Lernmittel, nicht das Lernziel.
- **Canonical Resource** – eine Ressource wird einmal gepflegt und mehrfach referenziert.
- **Provider Neutral** – O’Reilly, LinkedIn Learning, YouTube, Udemy und andere Quellen sind gleichberechtigt.
- **Capture now, classify later** – neue Links landen schnell in der Inbox und werden später eingeordnet.
- **Completion ≠ Mastery ≠ Retention** – Abschluss, Kompetenzstand und Wissensaktualität sind getrennt.
- **Local first** – der fachliche Kern funktioniert offline und ohne verpflichtende Cloud.
- **Human in control** – spätere AI-Funktionen liefern Vorschläge, keine ungeprüften fachlichen Änderungen.

## V1-Kern

V1 soll ermöglichen:

1. Lernziel anlegen.
2. Skills und Ziel-Level definieren.
3. Learning Path strukturieren.
4. Ressourcen providerunabhängig erfassen.
5. Resources mehreren Skills und Paths zuordnen.
6. Fortschritt dokumentieren.
7. Knowledge und Evidence festhalten.
8. Skill neu bewerten.
9. Daten suchen, sichern und wiederherstellen.

Nicht V1: eigener PDF-/Video-Reader, Cloud Sync, Mehrbenutzerbetrieb, Provider-Login, AI als Kernabhängigkeit, vollständiges SRS oder öffentliche Community.

## Repository-Struktur

```text
/
├── .github/
│   └── ISSUE_TEMPLATE/
├── assets/
├── docs/
│   ├── research/
│   ├── requirements/
│   ├── architecture/
│   ├── decisions/
│   ├── testing/
│   ├── security/
│   ├── operations/
│   ├── governance/
│   ├── release/
│   └── user/
├── src/                  # Domain / Application / Infrastructure / WinForms
├── tests/                # Domain / Application / Infrastructure / Architecture Tests
├── PROJECT-BRIEF.md
├── PROJECT-STATUS.md
├── ROADMAP.md
├── MILESTONES.md
└── GITHUB-SETUP.md
```

## Dokumente

Siehe [`DOCUMENTATION-INDEX.md`](DOCUMENTATION-INDEX.md).

## Aktueller Implementierungsstand

Milestone 0 bis Milestone 4 sind als Code-Stand enthalten. **M4 Hotfix 001** behebt einen beschädigten Testdouble-Quelltext aus der ersten M4-ZIP; die produktiven M4-Assemblies hatten im gemeldeten Windows-Build bereits kompiliert. Der M3-Stand wurde auf Windows mit 0 Warnungen, 0 Fehlern und 48/48 grünen Tests bestätigt. Implementiert sind derzeit:

- Providerverwaltung
- Ressourcenbibliothek
- Ressource anlegen und bearbeiten
- Tags
- Suche, Filter und Paging
- Fortschritt, Status und Priorität
- sichere HTTP/HTTPS-URL-Öffnung
- URL-Dublettenwarnung
- Archivieren und Wiederherstellen
- SQLite-Migrationen und ActivityLog
- lokale Logs und Single-Instance-Schutz
- Goals mit Skill-Zuordnung
- Competency Areas und Topics
- Skills mit Current-/Target-Level und Skill Gap
- append-only Skill Assessments mit Historie
- Learning Paths mit TreeView-basierter Hierarchie
- Required/Optional Nodes und Core Progress
- Node↔Skill- und Node↔Resource-Zuordnungen
- Node-Relationen und Zyklenschutz

Ziele, Lernpfade, Skills, Ressourcen und Inbox sind aktiv. Wissen, Evidence und weitere Bereiche bleiben bis zu ihren Milestones sichtbar, aber deaktiviert.

Details zu den implementierten Ständen stehen unter `docs/development/`, zuletzt in [`MILESTONE-4-IMPLEMENTATION.md`](docs/development/MILESTONE-4-IMPLEMENTATION.md).

## Milestone 2 – Quick Capture & Inbox

Aktuell zusätzlich nutzbar:

- `Ctrl+Shift+N` für schnelles URL-Capture
- optionale Titel-/Capture-Notiz
- automatische Inbox
- URL-Dublettenprüfung mit expliziter Konfliktentscheidung
- Inbox-Suche und Paging
- Klassifikation in den vollständigen Resource Editor
- reversibles Verwerfen/Archivieren

## Milestone 3 – Goals & Skills

Neu in diesem Stand:

- Lernziele mit Typ, Status, Priorität, Zieldatum und Next Action
- Competency Areas und Topics
- Skills mit qualitativer 1..5-Skala
- getrenntes Current-/Target-Level
- berechneter Skill Gap
- explizite Skillbewertung mit Historie
- Goal↔Skill ohne automatische Mastery-Änderung

Details: [`docs/development/MILESTONE-3-IMPLEMENTATION.md`](docs/development/MILESTONE-3-IMPLEMENTATION.md).

## Milestone 4 – Learning Paths

Neu in diesem Stand:

- Learning-Path-Library und Editor
- hierarchische Nodes mit Parent/Child
- Required/Optional
- Skill- und Resource-Zuordnung
- Move Up/Down und Parent-Wechsel mit Zyklenschutz
- Subtree-Archivierung
- Node-Relationen wie `Requires`, `AlternativeTo` und `Deepens`
- Core Progress getrennt von optionalen Inhalten

Details: [`docs/development/MILESTONE-4-IMPLEMENTATION.md`](docs/development/MILESTONE-4-IMPLEMENTATION.md).

## Build und Tests

```powershell
dotnet restore .\SASD.LearningManager.sln
dotnet build .\SASD.LearningManager.sln -c Release --no-restore
dotnet test .\SASD.LearningManager.sln -c Release --no-build
```

Ziel: **0 Fehler, 0 Warnungen, alle Tests grün**.

Eine ausführliche Verifikation steht in [`BUILD-VERIFY.md`](BUILD-VERIFY.md). GitHub Actions führt denselben Restore-/Build-/Test-Flow auf Windows aus.

## Datenpfad

```text
%LOCALAPPDATA%\SASD\LearningManager\
├── data\learning-manager.db
├── logs\
├── backups\
└── settings.json
```

## GitHub-Repository

Die empfohlene Repository-Konfiguration, Topics und Branch-Protection-Hinweise stehen in [`GITHUB-SETUP.md`](GITHUB-SETUP.md).

Die Produktlizenz ist in der Spezifikationsphase noch bewusst offen und wird vor einem öffentlichen Release entschieden.

## Entwicklungsstandard

<https://github.com/Robin-Goerlach/SASD-Development-Standard>

Die Dokumentation folgt dem Prinzip der progressiven Vertiefung: nur Artefakte erzeugen, die für Risiko, Wartbarkeit, Nachvollziehbarkeit oder reale Nachweise sinnvoll sind.
