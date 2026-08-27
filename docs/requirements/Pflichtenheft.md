# SASD Learning Manager – Pflichtenheft


**Produkt:** SASD Learning Manager  
**Produkttyp:** lokale Windows-Desktop-Anwendung  
**Produktoberfläche:** Windows Forms (WinForms)  
**Technologie-Baseline:** C# / .NET 8 / Windows Forms / SQLite  
**Dokumenttyp:** Pflichtenheft / technische Umsetzungsspezifikation  
**Dokumentstatus:** Entwurf zur technischen und strategischen Prüfung  
**Version:** 0.1  
**Stand:** 27. August 2026  
**Bezugsdokument:** `SASD-Learning-Manager-Lastenheft.md`  
**Research-Grundlage:** `SASD-Learning-Manager-Vorlagen-Funktionsanalyse.md`  
**Normativer Projektbezug:** SASD Development Standard, aktueller `main`-Stand  
**Standard:** <https://github.com/Robin-Goerlach/SASD-Development-Standard>

---

# 0. Dokumentkontrolle

## 0.1 Zweck

Dieses Pflichtenheft beschreibt, **wie** die im Lastenheft definierten fachlichen Anforderungen des SASD Learning Manager technisch umgesetzt werden sollen. Es ist die technische Arbeitsgrundlage für Architektur, Implementierung, Datenmodell, UI, Persistenz, Backup/Restore, Sicherheit, Tests, Build, Release und Wartung.

Das Lastenheft definiert **was** das Produkt leisten soll. Dieses Dokument definiert **wie** diese Anforderungen umgesetzt werden.

Die fachlichen Requirement-IDs des Lastenhefts bleiben erhalten. Technische Entscheidungen erhalten zusätzlich IDs der Form:

- `DES-ARCH-*`
- `DES-UI-*`
- `DES-DATA-*`
- `DES-APP-*`
- `DES-SEC-*`
- `DES-TEST-*`
- `DES-OPS-*`

## 0.2 Interpretation von `{{Produkt_Oberfläche}}`

Für dieses Pflichtenheft wird `{{Produkt_Oberfläche}}` als **Windows-Desktop-Anwendung mit Windows Forms** konkretisiert.

Die Anwendung wird in V1 nicht als Webanwendung, Electron-Anwendung, mobile Anwendung oder Cloud-Service entworfen.

## 0.3 Dokumentstatus

| Feld | Wert |
|---|---|
| Version | 0.1 |
| Status | Proposed |
| Freigabe | offen |
| Ziel | technische Baseline für V1 |
| nächster Gate | Review → ADRs → Milestone-Plan → Implementierung |

---

# 1. Technische Zielsetzung

## 1.1 Primärziel

Es soll eine robuste, wartbare, lokale Windows-Desktop-Anwendung entstehen, die langfristig mehrere tausend Lernressourcen, Skills, Learning Paths, Notizen und Evidenzen verwalten kann, ohne für ihre Kernfunktionen eine permanente Netzwerkverbindung zu benötigen.

## 1.2 Technische Leitprinzipien

### TP-01 – Local First

Fachliche Kerndaten werden lokal gespeichert und müssen offline verfügbar bleiben.

### TP-02 – Klare Schichtengrenzen

Fachlogik darf nicht in Formularen, Datenbankzugriffen oder UI-Events verstreut werden.

### TP-03 – UI ist Client des Application Layers

WinForms stellt Daten und Aktionen dar; es ist nicht der Ort der zentralen Geschäftslogik.

### TP-04 – Persistenz ist austauschbar

SQLite ist die V1-Persistenz, das Domain Model darf nicht von SQLite abhängig sein.

### TP-05 – Beziehung statt Kopie

Mehrfachverwendung einer Ressource wird über Relationen modelliert, nicht durch Datensatzkopien.

### TP-06 – Archivieren vor Löschen

Historisch relevante Objekte werden archiviert. Permanentes Löschen ist eine getrennte, bewusst auszulösende Operation.

### TP-07 – Fehler müssen nachvollziehbar sein

Unerwartete Fehler werden protokolliert; Benutzer erhalten verständliche Meldungen und eine Fehler-ID.

### TP-08 – Testbarer Kern

Domänenregeln und Use Cases müssen ohne WinForms-UI testbar sein.

### TP-09 – Progressive Komplexität

Einfache Erfassung verlangt wenige Felder. Fortgeschrittene Angaben werden optional ergänzt.

### TP-10 – Providerneutralität

O’Reilly, LinkedIn Learning, YouTube, Udemy und andere Anbieter sind Datenquellen, keine Architekturgrenzen.

---

# 2. Zielplattform und Technologie

## 2.1 Plattform

**Primär:** Windows 11 x64.

Die Anwendung soll auf aktuellen Windows-Desktop-Systemen mit .NET 8 laufen. Windows 10 ist kein langfristiges strategisches Ziel, soll aber nicht unnötig künstlich ausgeschlossen werden, sofern Runtime und WinForms dort funktionieren.

## 2.2 Kerntechnologien

| Bereich | Entscheidung |
|---|---|
| Sprache | C# |
| Runtime | .NET 8 |
| UI | Windows Forms |
| Datenbank | SQLite |
| SQLite Provider | `Microsoft.Data.Sqlite` |
| Dependency Injection | `Microsoft.Extensions.DependencyInjection` |
| Hosting/Lifecycle | `Microsoft.Extensions.Hosting` |
| Configuration | `Microsoft.Extensions.Configuration` |
| Logging-Abstraktion | `Microsoft.Extensions.Logging` |
| HTTP | `HttpClient` / `IHttpClientFactory` |
| JSON | `System.Text.Json` |
| Tests | xUnit |
| Textformat Knowledge | Markdown-kompatibler Plain Text |

## 2.3 Persistenzentscheidung

### DES-DATA-001

V1 verwendet **kein schwergewichtiges ORM als zwingende Basis**.

Datenzugriffe erfolgen über:

- Repository-Interfaces,
- `Microsoft.Data.Sqlite`,
- parameterisierte SQL-Statements,
- explizite Mapper,
- versionierte SQL-Migrationen.

Begründung:

- transparente Datenzugriffe,
- geringe Abhängigkeiten,
- gute Debugbarkeit,
- einfache Inspektion,
- kontrollierbare SQL-Struktur,
- reproduzierbare Migrationen.

### DES-DATA-002 – SQL-Sicherheitsregel

Alle SQL-Abfragen müssen:

- parameterisiert sein,
- Benutzereingaben niemals per String-Konkatenation in SQL einbauen,
- im Infrastructure Layer liegen,
- Transaktionen verwenden, wenn mehrere Änderungen fachlich zusammengehören.

---

# 3. Solution- und Projektstruktur

## 3.1 Solution

```text
SASD.LearningManager.sln
```

## 3.2 Projekte

```text
src/
├── SASD.LearningManager.Domain/
├── SASD.LearningManager.Application/
├── SASD.LearningManager.Infrastructure/
└── SASD.LearningManager.WinForms/

tests/
├── SASD.LearningManager.Domain.Tests/
├── SASD.LearningManager.Application.Tests/
├── SASD.LearningManager.Infrastructure.Tests/
└── SASD.LearningManager.Architecture.Tests/

docs/
├── requirements/
├── architecture/
├── decisions/
├── testing/
├── security/
├── operations/
└── user/
```

## 3.3 Abhängigkeitsrichtung

### DES-ARCH-001

```text
WinForms
   │
   ▼
Application
   │
   ▼
Domain

Infrastructure
   │
   ├──────► Application-Abstraktionen
   └──────► Domain
```

Nicht zulässig:

```text
Domain → Infrastructure
Domain → WinForms
Application → WinForms
Infrastructure → WinForms
```

WinForms darf keine SQL-Abfragen enthalten.

---

# 4. Verantwortlichkeiten der Schichten

## 4.1 Domain

Enthält:

- Entities,
- Value Objects,
- Enums,
- Domain Rules,
- fachliche Statusübergänge,
- Relationstypen,
- Domain Exceptions,
- fachliche Berechnungen.

Kernobjekte:

```text
Goal
CompetencyArea
Topic
Skill
SkillAssessment
LearningNeed
LearningPath
LearningPathNode
Resource
Provider
KnowledgeArtifact
Evidence
Tag
LearningSession
```

## 4.2 Application

Enthält:

- Use Cases,
- Commands und Queries,
- DTOs,
- Application Services,
- Repository-Interfaces,
- Infrastruktur-Abstraktionen,
- Use-Case-Validierung,
- Orchestrierung.

Beispiele:

```text
CreateResource
CaptureResource
ClassifyInboxResource
UpdateResourceProgress
CreateLearningPath
AssignResourceToPathNode
AssessSkill
CreateEvidence
Search
BuildDashboard
CreateBackup
RestoreBackup
```

## 4.3 Infrastructure

Enthält:

- SQLite Connection Factory,
- Repositories,
- SQL Queries,
- Schema-Migrationen,
- Backup und Restore,
- Import/Export,
- Dateisystem,
- URL-Metadaten,
- Logging-Sinks,
- spätere Provider-Adapter.

## 4.4 WinForms

Enthält:

- MainForm,
- Views/UserControls,
- Dialoge,
- Navigation,
- Presenter/Presentation Logic,
- UI-Binding,
- Accessibility-Metadaten,
- UI-Ressourcen.

Keine zentrale Fachlogik.

---

# 5. Startup und Application Lifecycle

## 5.1 Startup

Ablauf:

```text
WinForms initialisieren
        ↓
Host erstellen
        ↓
Konfiguration laden
        ↓
Logging aktivieren
        ↓
DI registrieren
        ↓
Datenordner prüfen/erstellen
        ↓
SQLite öffnen
        ↓
Migrationen anwenden
        ↓
Basisintegrität prüfen
        ↓
MainForm starten
```

## 5.2 Generic Host

`Microsoft.Extensions.Hosting` wird verwendet, um:

- DI,
- Konfiguration,
- Logging,
- Lifecycle

einheitlich aufzubauen.

## 5.3 Single Instance

### DES-OPS-001

V1 soll standardmäßig nur eine Instanz pro Benutzer/Datenbank zulassen.

Empfehlung:

- Named Mutex.

Beim zweiten Start:

- erste Instanz fokussieren, wenn pragmatisch umsetzbar,
- sonst verständliche Meldung, dass die Anwendung bereits läuft.

---

# 6. Hauptoberfläche

## 6.1 MainForm

### DES-UI-001

Die Anwendung verwendet ein zentrales `MainForm`.

```text
┌─────────────────────────────────────────────────────────────────────┐
│ SASD Learning Manager                    [globale Suche]     [⚙]    │
├───────────────┬─────────────────────────────────────────────────────┤
│ Heute         │                                                     │
│ Ziele         │                                                     │
│ Lernpfade     │                                                     │
│ Skills        │             aktueller Arbeitsbereich                │
│ Ressourcen    │                                                     │
│ Inbox         │                                                     │
│ Wissen        │                                                     │
│ Evidence      │                                                     │
│ Suche         │                                                     │
│ Datenpflege   │                                                     │
│ Einstellungen │                                                     │
├───────────────┴─────────────────────────────────────────────────────┤
│ Status | DB | letzter Backup-Status | Hintergrundvorgänge          │
└─────────────────────────────────────────────────────────────────────┘
```

## 6.2 Primäre Navigation

1. Heute / Dashboard
2. Ziele
3. Lernpfade
4. Skills
5. Ressourcen
6. Inbox
7. Wissen
8. Evidence
9. Suche
10. Datenpflege
11. Einstellungen

## 6.3 Navigation Service

```csharp
public interface INavigationService
{
    void NavigateTo(AppPage page);
    void NavigateToGoal(Guid id);
    void NavigateToSkill(Guid id);
    void NavigateToLearningPath(Guid id);
    void NavigateToResource(Guid id);
}
```

Views erzeugen nicht willkürlich gegenseitig Forms.

---

# 7. UI-Grundmuster

## 7.1 Listen-Detail-Muster

Für CRUD-Bereiche:

```text
┌──────────────── Liste / Filter ──────────────────────────┐
│ Suche [____________] Status [____]       [+ Neu]         │
├───────────────────────────────────────────────────────────┤
│ Titel        Status        Priorität      ...            │
│ ...                                                       │
└───────────────────────────────────────────────────────────┘
                         ↓
┌──────────────── Detail ──────────────────────────────────┐
│ Titel                                                     │
│ Beschreibung                                              │
│ Eigenschaften                                             │
│ Beziehungen                                               │
│ Historie                                                  │
│ [Speichern] [Archivieren]                                 │
└───────────────────────────────────────────────────────────┘
```

## 7.2 Editierverhalten

Komplexe Formulare verwenden:

- Dirty State,
- explizites Speichern,
- Warnung beim Schließen ungespeicherter Änderungen.

Einfache Statusänderungen dürfen direkt gespeichert werden.

## 7.3 Standardaktionen

- Neu
- Öffnen
- Speichern
- Abbrechen
- Archivieren
- Wiederherstellen
- Verknüpfen
- Verknüpfung entfernen
- dauerhaft löschen nur in Datenpflege

---

# 8. Dashboard

## 8.1 Zweck

Das Dashboard beantwortet:

> „Woran arbeite ich gerade und was ist als Nächstes sinnvoll?“

## 8.2 V1-Inhalt

- aktive Ziele,
- aktive Learning Paths,
- begonnene Ressourcen,
- Inbox-Anzahl,
- nächste Aktionen,
- Skills unter Zielniveau.

Beispiel:

```text
Aktive Ziele: 4         Aktive Lernpfade: 3
Inbox: 17               In Arbeit: 6

Nächste Aktionen
1. Docker Compose Kapitel 4 fortsetzen
2. Proxmox Ceph Lab aufbauen
3. Wazuh Detection Rule dokumentieren

Skill Gaps
Docker Networking       2 → 4
Ceph Operations          1 → 3
Incident Response        2 → 3
```

## 8.3 Dashboard-Service

`DashboardService` verwendet gezielte Projektionen/Aggregate und lädt nicht sämtliche Entities in den Speicher.

---

# 9. Ziele

## 9.1 GoalsView

Spalten:

- Titel
- Typ
- Status
- Priorität
- Zieltermin
- Anzahl Skills
- Anzahl Learning Paths
- Fortschrittsindikator

## 9.2 GoalDetail

Tabs:

1. Übersicht
2. Skills
3. Learning Paths
4. Learning Needs
5. Historie

## 9.3 Status

```csharp
public enum GoalStatus
{
    Planned,
    Active,
    Paused,
    Achieved,
    Archived
}
```

## 9.4 Typ

```csharp
public enum GoalType
{
    Learning,
    Career,
    Certification,
    Project,
    Interest,
    Other
}
```

## 9.5 Fachregeln

- `Achieved` setzt `AchievedAt`.
- Zielabschluss verändert Skills nicht automatisch.
- Zielabschluss archiviert Paths nicht automatisch.
- archivierte Ziele behalten alle historischen Beziehungen.

---

# 10. Kompetenzbereiche, Topics und Skills

## 10.1 Trennung

**Competency Area**
→ grobe fachliche Domäne.

**Topic**
→ Wissens-/Themenbereich.

**Skill**
→ bewertbare Fähigkeit.

Beispiel:

```text
Competency Area: Container Platforms
Topic: Docker Networking
Skill: Bridge-Netzwerke konfigurieren und diagnostizieren
```

## 10.2 SkillsView

Filter:

- Kompetenzbereich,
- Topic,
- Status,
- Ist-Level,
- Ziel-Level,
- Gap,
- Review-Fälligkeit.

Spalten:

- Skill
- Kompetenzbereich
- Ist
- Ziel
- Gap
- letzte Bewertung
- letzte Nutzung
- nächster Review

## 10.3 SkillDetail

Tabs:

1. Übersicht
2. Bewertungen
3. Evidence
4. Ressourcen
5. Learning Paths
6. Knowledge
7. Historie

## 10.4 Skill-Level

V1 verwendet fachlich eine fünfstufige Skala. Technisch wird empfohlen:

- `null` = noch nicht bewertet,
- 1 = Grundverständnis,
- 2 = mit Unterstützung arbeitsfähig,
- 3 = selbstständig anwendbar,
- 4 = sicher und vertieft,
- 5 = Experten-/Erklärniveau.

Damit wird „nicht bewertet“ nicht mit „Level 0“ verwechselt.

## 10.5 SkillAssessment

Jede neue Bewertung erzeugt Historie:

```text
SkillAssessment
- Id
- SkillId
- Level
- AssessmentType
- Reason
- AssessedAtUtc
- CreatedAtUtc
```

Der aktuelle Wert wird aus der neuesten gültigen Bewertung bestimmt oder zusätzlich als Cache am Skill gehalten.

## 10.6 Skill Gap

```text
Gap = TargetLevel - CurrentLevel
```

Nur wenn beide Werte vorhanden sind.

Darstellung:

```text
Docker Networking     2 → 4    Gap +2
Linux Basics          4 → 4    erreicht
Ceph                  ? → 3    nicht bewertet
```

Kein scheinpräziser Gesamtkompetenzscore über heterogene Skills.

---

# 11. Learning Needs

`LearningNeed` wird im Datenmodell vorgesehen und kann je nach Milestone erst nach dem ersten Kernworkflow UI-seitig aktiviert werden.

```text
LearningNeed
- Id
- Title
- Description
- Priority
- Status
- CreatedAtUtc
- UpdatedAtUtc
```

Beziehungen:

- Goal,
- Skill,
- Topic,
- Resource,
- Learning Path.

Status:

```text
Open
Planned
Addressed
Closed
Archived
```

Eine bloße Ressourcenzuordnung schließt einen Need nicht automatisch.

---

# 12. Learning Paths – Oberfläche

## 12.1 Path View

Bestandteile:

- Path-Liste,
- hierarchischer Node-Baum,
- Node-Detail,
- Resources,
- Skills,
- Progress.

## 12.2 Hierarchischer Editor

V1 verwendet eine robuste TreeView-basierte Darstellung.

```text
Cyber Security – Blue Team
├── 1 Grundlagen
│   ├── Security Concepts
│   └── Networking Refresh
├── 2 Detection
│   ├── Log Analysis
│   ├── SIEM
│   └── Detection Engineering
├── 3 Incident Response
└── 4 Forensics
```

## 12.3 Node-Aktionen

- Node hinzufügen
- Child hinzufügen
- bearbeiten
- hoch/runter
- Parent wechseln
- Pflicht/Optional
- Skill zuordnen
- Resource zuordnen
- Relation anlegen
- Status ändern
- Teilbaum archivieren

Drag & Drop ist wünschenswert, aber kein V1-Blocker.

## 12.4 Node

```text
LearningPathNode
- Id
- LearningPathId
- ParentNodeId?
- Title
- Description
- NodeType
- SortOrder
- IsRequired
- Status
- CreatedAtUtc
- UpdatedAtUtc
- ArchivedAtUtc?
```

Node Types:

```text
Module
Topic
SkillCheckpoint
Activity
Project
Milestone
Other
```

## 12.5 Path Resource Picker

Beim Hinzufügen einer Resource:

1. vorhandene Resource suchen,
2. auswählen,
3. alternativ neue Resource erfassen.

So bleibt das Canonical-Resource-Prinzip erhalten.

---

# 13. Learning-Path-Fortschritt

## 13.1 Kernfortschritt

V1 zeigt mindestens:

- Required Nodes completed / total,
- Optional Nodes completed / total.

Berechnung:

```text
CoreCompletion =
  CompletedRequiredNodes / RequiredNodes
```

Falls keine Required Nodes existieren:

```text
CompletedNodes / TotalNodes
```

## 13.2 Ressourcenfortschritt

Ressourcenprozent wird separat gezeigt.

**Nicht zulässig:**

```text
Course 100 % → Node automatisch zwingend Complete
```

Ein Node kann auch eine praktische Aufgabe repräsentieren, die zusätzliche Bewertung braucht.

---


# 14. Ressourcenbibliothek

## 14.1 ResourcesView

DataGridView-Spalten:

- Titel
- Typ
- Provider
- Status
- Fortschritt
- Priorität
- Schwierigkeit
- Dauer
- Sprache
- aktualisiert

Filter:

- Provider
- Ressourcentyp
- Status
- Priorität
- Skill
- Topic
- Tag
- Learning Path
- archiviert/aktiv.

## 14.2 ResourceDetail

Tabs:

1. Übersicht
2. Lernstatus
3. Zuordnungen
4. Beziehungen
5. Notizen/Wissen
6. Evidence
7. Historie

## 14.3 ResourceType

```csharp
public enum ResourceType
{
    Course,
    Video,
    Book,
    Article,
    Document,
    Documentation,
    Lab,
    Project,
    Podcast,
    PracticeExam,
    Event,
    Repository,
    Other
}
```

Die Liste kann nach V1 erweitert werden; die Datenmigration muss ältere Werte weiter verstehen.

## 14.4 ResourceStatus

```csharp
public enum ResourceStatus
{
    Inbox,
    Planned,
    Started,
    Paused,
    Deferred,
    Completed,
    Abandoned,
    Archived
}
```

## 14.5 Priorität

```text
Low
Normal
High
VeryHigh
```

Die UI verwendet verständliche deutsche Bezeichnungen.

## 14.6 Schwierigkeit

```text
Unknown
Beginner
Intermediate
Advanced
Expert
```

Resource Difficulty ist **nicht** Skill Mastery.

---

# 15. Canonical Resource

## 15.1 Grundsatz

### DES-DOM-RES-001

Eine Lernressource existiert als ein kanonischer Datensatz.

Beispiel:

```text
Resource R-00427
Linux Performance Optimization
Provider: O'Reilly

Zuordnungen:
→ Linux Performance
→ Troubleshooting
→ EX442 Preparation
→ Linux Diagnostics Path
```

Es werden keine Kopien erzeugt.

## 15.2 Join-Tabellen

Mehrfachbeziehungen:

```text
ResourceSkill
ResourceTopic
ResourceTag
PathNodeResource
ResourceEvidence
ResourceKnowledgeArtifact
```

## 15.3 Rückverweise

ResourceDetail zeigt:

- zugeordnete Skills,
- Topics,
- Paths/Nodes,
- Goals indirekt über Paths,
- Evidence,
- Knowledge Artifacts.

Damit wird sichtbar, warum die Ressource noch relevant ist.

---

# 16. Resource Relations

## 16.1 Modell

```text
ResourceRelation
- Id
- SourceResourceId
- TargetResourceId
- RelationType
- Note?
- CreatedAtUtc
```

## 16.2 RelationType

```text
AlternativeTo
OverlapsWith
Supersedes
Deepens
Requires
RecommendedBefore
RecommendedAfter
RelatedTo
```

`SupersededBy` kann in der UI als inverse Sicht von `Supersedes` dargestellt werden.

## 16.3 Symmetrische Relationen

Symmetrisch:

- AlternativeTo
- OverlapsWith
- RelatedTo

Gerichtet:

- Supersedes
- Deepens
- Requires
- RecommendedBefore
- RecommendedAfter

Application Layer verhindert doppelte oder widersprüchliche Beziehungen.

## 16.4 UI

Beispiel:

```text
Typ               Ziel                                  Notiz
Overlaps with     Udemy EX442 Course
Deepens           Brendan Gregg perf documentation
Superseded by     2026 revised edition
```

---

# 17. Provider

## 17.1 Entity

```text
Provider
- Id
- Name
- WebsiteUrl?
- Description?
- ProviderType
- Status
- CreatedAtUtc
- UpdatedAtUtc
- ArchivedAtUtc?
```

## 17.2 ProviderType

```text
LearningPlatform
Publisher
Vendor
University
Community
Personal
Other
```

## 17.3 Optionale Seed-Daten

Beim ersten Start können als editierbare Provider angeboten werden:

- O’Reilly
- LinkedIn Learning
- YouTube
- Udemy
- Microsoft Learn
- Red Hat
- Own / Other

Provider sind Datensätze, keine hardcodierten Anbieterlogiken.

## 17.4 Provider löschen

Provider mit verbundenen Resources wird standardmäßig archiviert, nicht physisch gelöscht.

---

# 18. Quick Capture

## 18.1 Zweck

Eine interessante Resource soll innerhalb weniger Sekunden gesichert werden können.

## 18.2 Aufruf

Global:

```text
+ Ressource erfassen
```

Shortcut:

```text
Ctrl + Shift + N
```

## 18.3 Dialog

```text
URL:   [________________________________________]

Titel: [optional________________________________]

Notiz:
[________________________________________________]
[________________________________________________]

[In Inbox speichern]  [Abbrechen]
```

## 18.4 Mindestdaten

Pflicht ist:

- URL **oder**
- Titel.

Bei URL ohne Titel darf temporär ein neutraler Platzhalter verwendet werden.

## 18.5 Normalized URL

Vor Dublettenprüfung:

- Trim,
- Host normalisieren,
- Fragment in der Regel entfernen,
- keine aggressive Entfernung von Query-Parametern,
- URL syntaktisch prüfen.

Die Original-URL bleibt erhalten.

## 18.6 Dublettenwarnung

Bei identischer normalisierter URL:

```text
Diese URL ist bereits vorhanden:

Linux Performance Optimization

[Bestehende Ressource öffnen]
[Trotzdem neu anlegen]
[Abbrechen]
```

„Trotzdem neu anlegen“ bleibt möglich, weil eine URL in seltenen Fällen mehrere fachliche Inhalte repräsentieren kann.

---

# 19. Inbox

## 19.1 Kriterium

```text
Resource.Status == Inbox
```

## 19.2 InboxView

Spalten:

- Titel
- URL/Domain
- erfasst am
- Kurznotiz
- möglicher Dublettenhinweis.

Aktionen:

- klassifizieren,
- Provider setzen,
- Typ setzen,
- Skills/Topics zuweisen,
- Learning Path zuweisen,
- Planned setzen,
- archivieren,
- URL öffnen.

## 19.3 Grundsatz

Die Inbox ist ein bewusst unvollständiger Zustand. Der Benutzer muss beim Capture keine komplette Taxonomie pflegen.

---

# 20. URL-Metadaten

## 20.1 V1 Core

Quick Capture funktioniert **ohne** Netzwerkmetadaten.

## 20.2 V1.x Service

```csharp
public interface IUrlMetadataService
{
    Task<UrlMetadataResult> TryGetMetadataAsync(
        Uri uri,
        CancellationToken cancellationToken);
}
```

Mögliche Daten:

- HTML title,
- OpenGraph title,
- OpenGraph description,
- canonical URL,
- site name.

## 20.3 Sicherheitsgrenzen

- nur HTTP/HTTPS,
- Timeout,
- Redirect-Limit,
- Response-Größenlimit,
- kein JavaScript,
- keine Browserautomatisierung,
- keine DRM-/Login-Umgehung,
- externe HTML-Inhalte werden nicht als vertrauenswürdiges HTML in die UI übernommen.

## 20.4 Datenschutz

Metadatenabruf ist ein Netzwerkkontakt zur Zielseite und wird nur explizit oder transparent konfiguriert ausgelöst.

Kein automatisches Hintergrund-Crawling aller gespeicherten Links.

---

# 21. Lernaktivität und Fortschritt

## 21.1 Resource Progress

```text
ProgressPercent: nullable integer 0..100
```

Nicht jeder Ressourcentyp benötigt Prozentfortschritt.

## 21.2 Statusübergänge

Zulässige typische Übergänge:

```text
Inbox     → Planned
Planned   → Started
Started   → Paused
Started   → Completed
Started   → Abandoned
Paused    → Started
Paused    → Abandoned
Deferred  → Planned
Completed → Started   // Wiederaufnahme/Auffrischung erlaubt
```

Archivierung bleibt separat kontrolliert.

## 21.3 Zeitstempel

Erster Wechsel auf Started:

```text
StartedAtUtc ??= now
```

Completed:

```text
CompletedAtUtc = now
```

Bei Ressourcentypen mit Prozent darf die UI anbieten:

```text
Fortschritt auf 100 % setzen?
```

Die Entscheidung ist sichtbar und nicht still.

## 21.4 Abbruch

Optionaler Abbruchgrund:

```text
Outdated
LowQuality
TooBasic
TooAdvanced
Duplicate
NoLongerRelevant
Other
```

Historie bleibt erhalten.

## 21.5 Next Action

V1 hält eine kleine lernbezogene Next Action an Goal/Path/Resource:

```text
NextActionText
NextActionDueDate?
```

Der Learning Manager wird dadurch **nicht** zum allgemeinen Task Manager.

---

# 22. Learning Sessions

V1.x kann Lern-Sessions speichern:

```text
LearningSession
- Id
- ResourceId?
- LearningPathId?
- StartedAtUtc
- EndedAtUtc?
- DurationMinutes
- ProgressBefore?
- ProgressAfter?
- Note?
```

Session-Erfassung bleibt optional. Es besteht kein Time-Tracker-Zwang.

---

# 23. Knowledge Artifacts

## 23.1 Grundsatz

Eine Resource und das daraus gewonnene Wissen sind unterschiedliche Objekte.

```text
Resource
   ↓
Note / Summary / Lesson
   ↓
Knowledge Artifact
   ↓
Skill / Path / Goal
```

## 23.2 Entity

```text
KnowledgeArtifact
- Id
- Title
- ArtifactType
- ContentMarkdown
- CreatedAtUtc
- UpdatedAtUtc
- ArchivedAtUtc?
```

## 23.3 Typen

```text
Note
Summary
CheatSheet
CodeSnippet
LessonLearned
Question
CommandReference
Procedure
Other
```

## 23.4 Speicherformat

Inhalt wird als Markdown-kompatibler Plain Text gespeichert.

Keine proprietäre Rich-Text-Struktur als fachliche Source of Truth.

## 23.5 V1 Editor

- mehrzeiliger Texteditor,
- Markdown-Inhalt,
- optional einfache Monospace-/Code-Unterstützung.

Markdown-Vorschau kann V1.x folgen.

## 23.6 Beziehungen

Knowledge Artifact kann mehreren Objekten zugeordnet sein:

- Resource,
- Skill,
- Topic,
- Goal,
- Learning Path.

---

# 24. Evidence

## 24.1 Entity

```text
Evidence
- Id
- Title
- EvidenceType
- Description
- EvidenceDate
- Url?
- LocalPath?
- Rating?
- CreatedAtUtc
- UpdatedAtUtc
- ArchivedAtUtc?
```

## 24.2 Typen

```text
CourseCompletion
Assessment
Quiz
Lab
Project
Certificate
PracticalUse
Documentation
Presentation
SelfAssessment
Other
```

## 24.3 Verknüpfungen

Viele-zu-viele:

- Evidence ↔ Skill,
- Evidence ↔ Resource,
- Evidence ↔ Goal.

## 24.4 Completion als Evidence

Beim Abschluss einer Course-Resource darf die UI anbieten:

```text
Abschluss als Evidence anlegen?
[Ja] [Nein]
```

Kein Zwang.

## 24.5 Zertifikat

V1.x optional:

- Issuer,
- Certificate URL/File,
- ValidFrom,
- ExpiresAt.

---

# 25. Skill Assessment

## 25.1 Dialog

```text
Skill: Docker Networking

Bisher: 2 – mit Unterstützung
Neu:    [3 – selbstständig]

Begründung:
[________________________________________________]

Evidence:
[x] Compose Lab
[x] O'Reilly Course Completion
[ ] anderes Projekt

[Bewertung speichern]
```

## 25.2 Regel

Neue Bewertung erzeugt Historieneintrag.

Alte Einschätzungen werden nicht überschrieben.

## 25.3 Keine automatische Höchststufe

**Verboten:**

```text
Resource Completed → Skill = 5
```

Evidence kann eine Bewertung unterstützen, ersetzt sie aber nicht.

---

# 26. Completion, Mastery und Retention

### DES-DOM-CMR-001

Drei getrennte Dimensionen:

## 26.1 Completion

Resource/Path:

```text
Status
ProgressPercent
StartedAt
CompletedAt
```

## 26.2 Mastery

SkillAssessment:

```text
Level
AssessedAt
Reason
Evidence
```

## 26.3 Retention

Skill:

```text
LastUsedAt?
NextReviewAt?
RetentionStatus?
```

## 26.4 Nichtkopplung

Completion darf Mastery nicht automatisch setzen.

Mastery darf Retention nicht automatisch als „aktuell“ markieren, wenn keine passende Regel existiert.

---

# 27. Retention und Review

V1.x:

```text
Skill.LastUsedAtUtc
Skill.NextReviewAtUtc
```

Möglicher berechneter Status:

```text
Unknown
Current
ReviewSoon
Stale
```

Das Modell bleibt bewusst leichtgewichtig. Kein SM-2-/FSRS-Scheduler in V1.

---

# 28. Suche

## 28.1 Global Search

Suchfelder:

- Goal Titel/Beschreibung,
- Skill Name/Beschreibung,
- Resource Titel/Beschreibung/URL,
- Provider,
- Learning Path,
- Knowledge Artifact Titel/Inhalt,
- Evidence Titel/Beschreibung.

## 28.2 Ergebnisdarstellung

Gruppiert:

```text
Resources (12)
Skills (4)
Learning Paths (2)
Knowledge (7)
Goals (1)
Evidence (3)
```

Klick navigiert zum Objekt.

## 28.3 Suchtechnik

V1:

- parameterisierte SQL-Queries,
- `LIKE`,
- Indizes,
- Pagination.

V1.x:

- SQLite FTS5.

Keine Vektordatenbank im V1-Core.

---

# 29. Filter

Beispiel DTO:

```csharp
public sealed record ResourceFilter(
    string? SearchText,
    Guid? ProviderId,
    ResourceType? Type,
    ResourceStatus? Status,
    ResourcePriority? Priority,
    Guid? SkillId,
    Guid? TopicId,
    Guid? TagId,
    Guid? LearningPathId,
    bool IncludeArchived);
```

Filter werden in SQL angewandt und nicht erst auf tausenden geladenen Datensätzen.

---

# 30. Smart Views

V1.x:

```text
SavedView
- Id
- Name
- ViewType
- FilterJson
- SortJson
- CreatedAtUtc
```

Beispiele:

- „Security – noch nicht begonnen“
- „O’Reilly – geplant“
- „Skills unter Zielniveau“
- „Ressourcen ohne Zuordnung“
- „Review fällig“.

`FilterJson` enthält deklarative Filterdaten, keinen ausführbaren Code.

---

# 31. Dashboard „Als Nächstes“

V1 verwendet keinen AI-Algorithmus.

Sortierlogik:

1. überfälliges Datum,
2. nächstes Fälligkeitsdatum,
3. Priorität,
4. explizite Next Action,
5. Aktualität/letzte Änderung.

UI zeigt nachvollziehbar, warum etwas oben steht.

Beispiele:

```text
Fällig heute
Hohe Priorität
Nächste Aktion gesetzt
Aktiver Learning Path
```

---

# 32. Datenpflege

MaintenanceView zeigt mindestens:

- Inbox,
- Resources ohne Provider,
- Resources ohne Skill/Topic,
- Resources ohne Path,
- mögliche URL-Dubletten,
- archivierte Objekte,
- fehlende lokale Dateien,
- unbenutzte Tags,
- DB-Information,
- Backup/Restore,
- Integrity Check.

Keine automatische „Bereinigung“ ohne Nutzerfreigabe.

---

# 33. Datenbankpfad

Default:

```text
%LOCALAPPDATA%\SASD\LearningManager\data\learning-manager.db
```

Weitere Pfade:

```text
%LOCALAPPDATA%\SASD\LearningManager\logs\
%LOCALAPPDATA%\SASD\LearningManager\backups\
%LOCALAPPDATA%\SASD\LearningManager\settings.json
```

Anwendungsdateien und Benutzerdaten werden strikt getrennt.

---

# 34. SQLite-Konfiguration

Bei jeder Connection:

```sql
PRAGMA foreign_keys = ON;
```

WAL wird getestet und voraussichtlich verwendet:

```sql
PRAGMA journal_mode = WAL;
```

Die endgültige WAL-Entscheidung wird zusammen mit der Backupstrategie per ADR dokumentiert.

Ein angemessener Busy Timeout wird gesetzt und getestet.

---

# 35. Zeitwerte

Application Layer verwendet bevorzugt:

```text
DateTimeOffset
```

Persistenz:

- UTC.

UI:

- lokale Zeit.

SQLite-Darstellung bevorzugt:

```text
ISO-8601 TEXT in UTC
```

Vorteil: einfach lesbar und debugbar.

Eine `IClock`-Abstraktion verbessert Testbarkeit.

---

# 36. IDs

Empfehlung:

- GUID/UUID.

SQLite speichert in V1 GUIDs als standardisierte `TEXT`-Werte.

Gründe:

- gute Debugbarkeit,
- stabile Exporte,
- einfache Merge-/Importlogik,
- keine globale zentrale ID-Sequenz nötig.

---

# 37. Datenbankschema – Kerntabellen

## 37.1 Goals

```text
Goals
- Id TEXT PK
- Title TEXT NOT NULL
- Description TEXT
- GoalType TEXT NOT NULL
- Motivation TEXT
- Priority TEXT NOT NULL
- Status TEXT NOT NULL
- TargetDate TEXT
- NextActionText TEXT
- NextActionDueDate TEXT
- CreatedAtUtc TEXT NOT NULL
- UpdatedAtUtc TEXT NOT NULL
- AchievedAtUtc TEXT
- ArchivedAtUtc TEXT
```

## 37.2 CompetencyAreas

```text
CompetencyAreas
- Id TEXT PK
- Name TEXT NOT NULL
- Description TEXT
- Status TEXT NOT NULL
- CreatedAtUtc TEXT NOT NULL
- UpdatedAtUtc TEXT NOT NULL
- ArchivedAtUtc TEXT
```

## 37.3 Topics

```text
Topics
- Id TEXT PK
- Name TEXT NOT NULL
- Description TEXT
- Status TEXT NOT NULL
- CreatedAtUtc TEXT NOT NULL
- UpdatedAtUtc TEXT NOT NULL
- ArchivedAtUtc TEXT
```

## 37.4 Skills

```text
Skills
- Id TEXT PK
- Name TEXT NOT NULL
- Description TEXT
- CurrentLevel INTEGER
- TargetLevel INTEGER
- LastUsedAtUtc TEXT
- NextReviewAtUtc TEXT
- Status TEXT NOT NULL
- CreatedAtUtc TEXT NOT NULL
- UpdatedAtUtc TEXT NOT NULL
- ArchivedAtUtc TEXT
```

## 37.5 SkillAssessments

```text
SkillAssessments
- Id TEXT PK
- SkillId TEXT NOT NULL FK
- Level INTEGER NOT NULL
- AssessmentType TEXT NOT NULL
- Reason TEXT
- AssessedAtUtc TEXT NOT NULL
- CreatedAtUtc TEXT NOT NULL
```

## 37.6 Providers

```text
Providers
- Id TEXT PK
- Name TEXT NOT NULL
- WebsiteUrl TEXT
- Description TEXT
- ProviderType TEXT NOT NULL
- Status TEXT NOT NULL
- CreatedAtUtc TEXT NOT NULL
- UpdatedAtUtc TEXT NOT NULL
- ArchivedAtUtc TEXT
```

## 37.7 Resources

```text
Resources
- Id TEXT PK
- Title TEXT NOT NULL
- ResourceType TEXT NOT NULL
- ProviderId TEXT FK NULL
- Url TEXT
- NormalizedUrl TEXT
- LocalPath TEXT
- Description TEXT
- WhySaved TEXT
- Creator TEXT
- LanguageCode TEXT
- PublishedDate TEXT
- VersionText TEXT
- EstimatedMinutes INTEGER
- Difficulty TEXT
- Priority TEXT NOT NULL
- Status TEXT NOT NULL
- ProgressPercent INTEGER
- NextActionText TEXT
- NextActionDueDate TEXT
- StartedAtUtc TEXT
- CompletedAtUtc TEXT
- CreatedAtUtc TEXT NOT NULL
- UpdatedAtUtc TEXT NOT NULL
- ArchivedAtUtc TEXT
```

## 37.8 LearningPaths

```text
LearningPaths
- Id TEXT PK
- Title TEXT NOT NULL
- Description TEXT
- Status TEXT NOT NULL
- Priority TEXT NOT NULL
- PlannedStartDate TEXT
- TargetDate TEXT
- NextActionText TEXT
- NextActionDueDate TEXT
- StartedAtUtc TEXT
- CompletedAtUtc TEXT
- CreatedAtUtc TEXT NOT NULL
- UpdatedAtUtc TEXT NOT NULL
- ArchivedAtUtc TEXT
```

## 37.9 LearningPathNodes

```text
LearningPathNodes
- Id TEXT PK
- LearningPathId TEXT NOT NULL FK
- ParentNodeId TEXT FK NULL
- Title TEXT NOT NULL
- Description TEXT
- NodeType TEXT NOT NULL
- SortOrder INTEGER NOT NULL
- IsRequired INTEGER NOT NULL
- Status TEXT NOT NULL
- CreatedAtUtc TEXT NOT NULL
- UpdatedAtUtc TEXT NOT NULL
- ArchivedAtUtc TEXT
```

## 37.10 KnowledgeArtifacts

```text
KnowledgeArtifacts
- Id TEXT PK
- Title TEXT NOT NULL
- ArtifactType TEXT NOT NULL
- ContentMarkdown TEXT
- CreatedAtUtc TEXT NOT NULL
- UpdatedAtUtc TEXT NOT NULL
- ArchivedAtUtc TEXT
```

## 37.11 Evidence

```text
Evidence
- Id TEXT PK
- Title TEXT NOT NULL
- EvidenceType TEXT NOT NULL
- Description TEXT
- EvidenceDate TEXT
- Url TEXT
- LocalPath TEXT
- Rating INTEGER
- CreatedAtUtc TEXT NOT NULL
- UpdatedAtUtc TEXT NOT NULL
- ArchivedAtUtc TEXT
```

## 37.12 Tags

```text
Tags
- Id TEXT PK
- Name TEXT NOT NULL
- Description TEXT
- CreatedAtUtc TEXT NOT NULL
- UpdatedAtUtc TEXT NOT NULL
```

---

# 38. Relationstabellen

Mindestens:

```text
GoalSkill
GoalLearningPath

CompetencyAreaTopic
CompetencyAreaSkill
TopicSkill

ResourceSkill
ResourceTopic
ResourceTag

LearningPathNodeSkill
LearningPathNodeResource

KnowledgeArtifactResource
KnowledgeArtifactSkill
KnowledgeArtifactTopic
KnowledgeArtifactGoal
KnowledgeArtifactLearningPath

EvidenceSkill
EvidenceResource
EvidenceGoal

ResourceRelation
LearningPathNodeRelation
```

Join-Tabellen erhalten:

- Foreign Keys,
- eindeutigen Composite Key oder surrogate ID,
- Indizes auf beiden Richtungen.

---

# 39. Constraints und Indizes

## 39.1 CHECK

```sql
CHECK (
  ProgressPercent IS NULL OR
  (ProgressPercent >= 0 AND ProgressPercent <= 100)
)
```

Skill Level:

```sql
CHECK (
  CurrentLevel IS NULL OR
  (CurrentLevel >= 1 AND CurrentLevel <= 5)
)
```

## 39.2 Indizes

Mindestens:

- `Resources.NormalizedUrl`
- `Resources.Status`
- `Resources.ProviderId`
- `Resources.ResourceType`
- `Resources.Priority`
- `Skills.Name`
- `Skills.Status`
- `LearningPaths.Status`
- `LearningPathNodes.LearningPathId`
- `LearningPathNodes.ParentNodeId`
- `Tags.Name`
- alle häufig genutzten Join-FKs.

## 39.3 Enum-Persistenz

Fachliche Statuswerte werden bevorzugt als **TEXT** persistiert.

Vorteile:

- DB bleibt lesbar,
- Enum-Reihenfolge kann sich nicht unbemerkt auf Persistenz auswirken.

---

# 40. Foreign-Key-Verhalten

Default:

```text
RESTRICT
```

Nicht blind `CASCADE`.

Join-Tabellen dürfen beim bewusst permanenten Löschen entsprechend bereinigt werden. Fachliche Kernobjekte werden standardmäßig archiviert.

---

# 41. Migrationen

## 41.1 Verzeichnis

```text
Infrastructure/Persistence/Migrations/
├── 0001_initial_schema.sql
├── 0002_add_resource_relations.sql
├── 0003_add_skill_assessments.sql
└── ...
```

## 41.2 SchemaMigrations

```text
SchemaMigrations
- Version INTEGER PK
- Name TEXT NOT NULL
- AppliedAtUtc TEXT NOT NULL
- Checksum TEXT
```

## 41.3 Migration Runner

Beim Start:

1. SchemaVersion lesen.
2. ausstehende Migrationen sortieren.
3. Migration in Transaktion ausführen.
4. Version protokollieren.
5. bei Fehler Rollback.
6. Anwendung nicht mit halb migriertem Schema fortsetzen.

## 41.4 Backup vor kritischer Migration

Ab produktivem Pilotbetrieb:

- automatische Sicherheitskopie oder
- nachweislich sichere Migrations-/Restore-Strategie.

---

# 42. Repository-Design

Interfaces:

```text
IGoalRepository
ISkillRepository
IResourceRepository
ILearningPathRepository
IProviderRepository
IKnowledgeArtifactRepository
IEvidenceRepository
ITagRepository
```

Es wird kein universelles `IRepository<T>` erzwungen, wenn fachlich unterschiedliche Queries sinnvoller sind.

Read Queries dürfen spezialisierte Projections nutzen.

---

# 43. Application Services

Beispiele:

## ResourceService

```text
Create
Update
Archive
Restore
PermanentlyDelete
ChangeStatus
ChangeProgress
AssignSkill
AssignTopic
AssignTag
LinkToPathNode
AddRelation
```

## LearningPathService

```text
CreatePath
AddNode
MoveNode
ArchiveNode
AssignSkill
AssignResource
AddNodeRelation
CalculateProgress
```

## SkillService

```text
Create
Update
Assess
SetTargetLevel
AddEvidence
GetGap
ScheduleReview
```

## BackupApplicationService

```text
CreateBackup
ValidateBackup
RestoreBackup
```

---

# 44. Validierung

## 44.1 Domain

- Titel nicht leer,
- Fortschritt 0..100,
- Skill Level 1..5 oder null,
- Relation nicht auf sich selbst,
- Path-Hierarchie ohne Zyklen,
- ungültige Statusübergänge verhindern,
- Archivzustände berücksichtigen.

## 44.2 Application

- referenzierte IDs existieren,
- archivierte Objekte nicht ungewollt aktiv verlinken,
- Dublettenregeln,
- Transaktionsgrenzen.

## 44.3 UI

- `ErrorProvider`,
- verständliche feldnahe Meldung,
- Fokus auf fehlerhaftes Feld,
- keine rohe SQLite-Exception für Benutzer.

---


# 45. Archivieren und permanentes Löschen

## 45.1 Archivieren

Standardoperation:

```text
ArchivedAtUtc = now
Status = Archived
```

Beziehungen und Historie bleiben erhalten.

## 45.2 Wiederherstellen

Archivierte Objekte können wiederhergestellt werden, sofern ihre fachlichen Beziehungen weiterhin valide sind.

## 45.3 Permanentes Löschen

Nur im Bereich **Datenpflege**.

Vor Löschung:

1. Objektbeziehungen ermitteln.
2. Benutzer anzeigen, was betroffen ist.
3. bei historisch relevanten Beziehungen Archivierung empfehlen.
4. explizite Bestätigung.
5. Löschung in Transaktion.

Skills mit Assessments/Evidence, Provider mit Resources und Paths mit Lernhistorie werden standardmäßig nicht physisch gelöscht.

---

# 46. Activity Log / Historie

V1 speichert wichtige fachliche Ereignisse:

```text
ActivityLog
- Id
- EntityType
- EntityId
- ActivityType
- OccurredAtUtc
- Summary
- MetadataJson?
```

Beispiele:

- ResourceCreated
- ResourceStarted
- ResourceCompleted
- ResourceArchived
- SkillAssessed
- EvidenceCreated
- PathCreated
- PathCompleted
- GoalAchieved.

Das Activity Log ist **kein Event-Sourcing-System** und kein manipulationssicheres Audit-Log. Es dient der persönlichen Nachvollziehbarkeit.

---

# 47. Backup

## 47.1 Ziel

Ein Backup muss den vollständigen fachlichen Zustand der Anwendung wiederherstellbar sichern.

## 47.2 Dateiformat

Empfehlung:

```text
SASD-LearningManager-Backup-YYYYMMDD-HHMMSS.zip
```

Inhalt:

```text
manifest.json
database/learning-manager.db
settings/exportable-settings.json
attachments-manifest.json
```

Extern referenzierte PDFs/Bücher/Dateien außerhalb des Anwendungsdatenverzeichnisses werden in V1 **nicht** automatisch kopiert.

## 47.3 Konsistentes SQLite-Backup

Wenn WAL aktiv ist, darf die laufende DB nicht einfach blind kopiert werden.

Verwendet wird:

- SQLite Backup API oder
- eine nachweislich konsistente Checkpoint-/Backupstrategie.

## 47.4 Manifest

Beispiel:

```json
{
  "product": "SASD Learning Manager",
  "backupFormatVersion": 1,
  "applicationVersion": "0.7.0",
  "schemaVersion": 12,
  "createdAtUtc": "2026-08-27T06:00:00Z",
  "files": [
    {
      "path": "database/learning-manager.db",
      "sha256": "..."
    }
  ]
}
```

## 47.5 Hashes

SHA-256 dient zur Integritätsprüfung.

Ein Hash ist **keine digitale Signatur** und beweist nicht die vertrauenswürdige Herkunft eines fremden Backups.

---

# 48. Restore

## 48.1 Ablauf

1. Backup auswählen.
2. ZIP-Struktur validieren.
3. Manifest lesen.
4. Backupformat prüfen.
5. Hashes prüfen.
6. eingebettete DB in temporärem Verzeichnis öffnen.
7. `PRAGMA integrity_check` ausführen.
8. aktuelle Daten optional automatisch sichern.
9. produktive DB schließen.
10. Daten ersetzen.
11. benötigte Schema-Migrationen ausführen.
12. Anwendung neu starten.
13. fachlichen Smoke Check ermöglichen.

## 48.2 Schutzmaßnahmen

- Zip-Slip-Schutz,
- keine Extraktion außerhalb Temp-Ziel,
- unbekannte Backup-Majorversion ablehnen,
- beschädigtes Backup niemals über produktive DB schreiben,
- Restore nie ohne klare Bestätigung.

## 48.3 Restore-Warnung

```text
Das Wiederherstellen ersetzt den aktuellen Datenstand.

Vor dem Restore wird empfohlen, den aktuellen Stand zu sichern.

[Backup jetzt erstellen]
[Restore fortsetzen]
[Abbrechen]
```

---

# 49. Export

## 49.1 Unterschied Backup vs. Export

**Backup**
→ vollständige Wiederherstellung derselben Anwendung.

**Export**
→ Portabilität, Weiterverarbeitung, Analyse.

Die UI erklärt diese Unterscheidung.

## 49.2 Formate

V1.x:

- JSON – vollständige strukturierte Exporte,
- CSV – tabellarische Resources/Skills/Reports,
- Markdown – Knowledge Artifacts.

## 49.3 Versionierter JSON-Export

```json
{
  "format": "sasd-learning-manager-export",
  "version": 1,
  "exportedAtUtc": "...",
  "data": {}
}
```

## 49.4 Beziehungen

Exportierte Entities behalten stabile IDs/Referenzen, sodass Relationen nachvollziehbar bleiben.

---

# 50. Import

## 50.1 Ablauf

1. Datei auswählen.
2. Format erkennen.
3. Parsing in Staging Model.
4. Strukturvalidierung.
5. Dublettenanalyse.
6. Preview.
7. Nutzer entscheidet über Konflikte.
8. Import in Transaktion.
9. Ergebnisbericht.

## 50.2 Sicherheitsregeln

- kein ausführbarer Code,
- keine unsichere polymorphe Typdeserialisierung,
- Dateigrößenlimit,
- Pfade nicht automatisch starten,
- unbekannte Felder kontrolliert behandeln.

## 50.3 Dublettenoptionen

```text
Überspringen
Bestehenden Datensatz verwenden
Daten gezielt zusammenführen
Trotzdem neu anlegen
```

Merge nie ungeprüft vollautomatisch.

---

# 51. Lokale Dateien

Resource und Evidence dürfen lokale Pfade referenzieren.

V1 verwendet:

```text
LocalPath
```

Es gibt noch keinen vollständigen internen File Vault.

UI zeigt:

- Datei vorhanden,
- Datei fehlt,
- Ordner öffnen,
- Datei öffnen.

Bei fehlender Datei bleiben fachliche Resource/Evidence erhalten.

---

# 52. Externe Links

URLs werden über den Standardbrowser geöffnet.

Nur standardmäßig zulässige Schemas:

- `http`
- `https`.

Kein `javascript:`, `data:` oder ungeprüftes `file:`.

Es wird in V1 kein eingebetteter Browser als Kernkomponente verwendet.

---

# 53. Logging

## 53.1 Ziele

Logging dient:

- Fehlerdiagnose,
- Migrationsanalyse,
- Backup/Restore,
- Import/Export,
- Start/Shutdown,
- unerwarteten Exceptions.

## 53.2 Verzeichnis

```text
%LOCALAPPDATA%\SASD\LearningManager\logs\
```

## 53.3 Levels

```text
Trace
Debug
Information
Warning
Error
Critical
```

Release Default:

```text
Information
```

## 53.4 Rotation

Rolling File:

- täglich oder nach Größenlimit,
- begrenzte Aufbewahrung, z. B. 30 Tage,
- ältere Logs automatisch bereinigen.

## 53.5 Nicht loggen

Standardmäßig nicht vollständig protokollieren:

- Knowledge-Inhalte,
- persönliche Notizen,
- komplette Assessment-Begründungen,
- sensible URL Query Parameter,
- Secrets/Tokens.

---

# 54. Fehlerbehandlung

## 54.1 Globale Fehlergrenzen

WinForms:

- `Application.ThreadException`,
- `AppDomain.CurrentDomain.UnhandledException`,
- unbeobachtete Task-Fehler soweit sinnvoll.

## 54.2 Fehlerarten

**Validierungsfehler**

```text
„Der Titel darf nicht leer sein.“
```

**fachlicher Konflikt**

```text
„Dieser Learning-Path-Knoten kann nicht unter eines seiner eigenen
Unterelemente verschoben werden.“
```

**Systemfehler**

```text
„Die Änderung konnte nicht gespeichert werden.
Der Fehler wurde protokolliert.
Fehler-ID: ERR-20260827-7F3A“
```

## 54.3 Correlation/Error ID

Unerwartete Fehler erhalten eine kurze ID, die UI und Log verbindet.

## 54.4 Recovery

Bei kritischem DB-Startfehler:

```text
[Erneut versuchen]
[Backup wiederherstellen]
[Datenordner öffnen]
[Logordner öffnen]
[Beenden]
```

Keine riskante automatische Reparatur.

---

# 55. SQLite Integrity Check

Datenpflege kann explizit ausführen:

```sql
PRAGMA integrity_check;
```

Ergebnis wird verständlich dargestellt.

Bei Problemen:

- Backup empfehlen,
- keine automatische DB-Manipulation ohne gesicherten Plan.

---

# 56. Einstellungen

## 56.1 Speicherformat

Nichtfachliche Benutzereinstellungen:

```text
%LOCALAPPDATA%\SASD\LearningManager\settings.json
```

Fachliche Daten bleiben SQLite.

## 56.2 Inhalte

V1:

- Fensterposition/-größe,
- zuletzt verwendeter Arbeitsbereich,
- Backup-Verzeichnis,
- UI-Sprache vorbereiten,
- ggf. Log-Level in Advanced Settings.

## 56.3 Crash-safe Settings

Settings werden über temporäre Datei geschrieben und möglichst atomar ersetzt.

---

# 57. Security Design

## 57.1 Angriffsflächen V1

- externe URLs,
- URL-Metadatenabruf,
- SQLite-Datei,
- Importdateien,
- Backup-ZIPs,
- lokale Dateipfade,
- Logs.

## 57.2 Keine Provider-Credentials V1

O’Reilly-, LinkedIn-, YouTube- oder Udemy-Passwörter werden nicht gespeichert.

## 57.3 SQL Injection

Alle Queries parameterisiert.

## 57.4 ZIP Slip

Beim Restore:

- Zielpfad jedes Entries normalisieren,
- sicherstellen, dass er innerhalb des temporären Restore-Verzeichnisses liegt.

## 57.5 HTTP

- HTTPS bevorzugt,
- Timeouts,
- Redirect-Grenze,
- Response-Size-Limit,
- keine JavaScript-Ausführung.

## 57.6 Fremde Inhalte

Externe HTML-Inhalte werden in V1 nicht als aktives HTML eingebettet.

---

# 58. Datenschutz und Privacy by Default

## 58.1 Datenarten

Persönlich bzw. potenziell sensibel:

- Karriereziele,
- Kompetenzbewertungen,
- Lernhistorie,
- Notizen,
- Evidence,
- Zeitangaben.

## 58.2 Regeln

- keine verpflichtende Cloud,
- keine verpflichtende Telemetrie,
- keine ungefragte Übertragung persönlicher Inhalte,
- keine automatischen Crash-Uploads,
- Online-Komfortfunktionen transparent und optional.

## 58.3 AI später

Vor Einführung externer AI:

- Data Flow dokumentieren,
- Provider dokumentieren,
- Datenumfang offenlegen,
- Opt-in,
- Aufbewahrung beim Provider berücksichtigen,
- lokale Modelle prüfen.

---

# 59. UI Accessibility

## 59.1 Controls

Sinnvolle:

- `AccessibleName`,
- `AccessibleDescription`,
- logische Tab-Reihenfolge.

## 59.2 Tastatur

Mindestens:

```text
Ctrl+N         Neues Objekt im aktuellen Bereich
Ctrl+Shift+N   Quick Capture
Ctrl+F         globale Suche
Ctrl+S         Speichern
Esc            Abbrechen/Schließen
```

## 59.3 Farbunabhängigkeit

Status nie ausschließlich über Farbe:

```text
✓ Abgeschlossen
▶ In Arbeit
⏸ Pausiert
○ Geplant
```

## 59.4 DPI

Testmatrix:

- 100 %
- 125 %
- 150 %
- 200 %.

Kernfunktionen dürfen nicht abgeschnitten werden.

---

# 60. UI Responsiveness

## 60.1 Längere Operationen

Async + CancellationToken:

- URL-Metadaten,
- Import,
- Export,
- Backup,
- Restore,
- größere Suchläufe.

## 60.2 Kein UI Freeze

Netzwerkzugriffe dürfen niemals synchron im UI-Thread laufen.

## 60.3 Progress

Längere Operationen zeigen:

- Aktivität,
- optional Prozent,
- Abbrechen, sofern sicher möglich.

---

# 61. Performance

## 61.1 Referenzdatenbestand

Zieltests mit mindestens:

```text
5.000 Resources
2.000 Skills/Topics
500 Learning Paths
10.000 Path Nodes
20.000 Relations
10.000 Knowledge Artifacts
```

## 61.2 Engineering-Ziele

| Vorgang | Ziel |
|---|---:|
| Start bis Dashboard | ideal < 3 s |
| Ressourcenliste erste Seite | < 500 ms |
| einfache lokale Suche | < 500 ms |
| einzelnes Speichern | < 250 ms |
| Dashboard Aggregate | < 1 s |

Keine Echtzeitgarantie, sondern messbare Entwicklungsziele.

## 61.3 Pagination

Große Grids laden nicht zwingend alle Records.

Bevorzugt:

- SQL Pagination,
- z. B. 100–250 Rows je Seite.

---

# 62. Caching

Leichtes Caching für:

- Provider Lookup,
- Tags,
- kleine Referenzlisten.

Kein komplexes Cache-System.

---

# 63. Concurrency

V1 Single-user.

Trotzdem:

- Schreiboperationen transaktional,
- separate SQLite Connections für parallele Hintergrundvorgänge,
- Connections nicht zwischen Threads teilen,
- doppeltes gleichzeitiges Speichern aus UI verhindern.

---

# 64. WinForms Presentation Pattern

Kein dogmatisches MVVM.

Empfohlen:

```text
View/UserControl
     ↓
Presenter bzw. dünne Presentation Logic
     ↓
Application Service
```

Event Handler bleiben dünn.

DataGridView bindet bevorzugt DTOs/Projections und nicht komplette Domain Aggregate.

---

# 65. View DTOs

Beispiel:

```csharp
public sealed record ResourceListItemDto(
    Guid Id,
    string Title,
    string? ProviderName,
    ResourceType Type,
    ResourceStatus Status,
    int? ProgressPercent,
    ResourcePriority Priority);
```

Read Queries dürfen direkt optimierte Projections liefern.

---

# 66. Query-/Command-Trennung

Es wird pragmatisch zwischen:

- Read Queries
- Write Use Cases

unterschieden.

Kein vollständiges CQRS-Framework erforderlich.

Keine MediatR-Pflicht.

Keine AutoMapper-Pflicht.

---

# 67. Hierarchien ohne Zyklen

## 67.1 Path Nodes

Ein Node darf nicht:

- eigener Parent,
- Child eines eigenen Descendants

werden.

Verschiebeoperationen validieren dies im Application Layer.

## 67.2 Skill-Hierarchie

Falls Skills später Parent/Child erhalten, gilt dieselbe Regel.

---

# 68. Sortierung der Path Nodes

`SortOrder INTEGER`.

Beim Einfügen/Verschieben ist Neuindexierung der Geschwister zulässig.

Kein komplexes fractional indexing für V1.

---

# 69. Node-Löschung

Node mit Children:

V1 bevorzugt:

```text
„Gesamten Teilbaum archivieren“
```

Komplexe Varianten wie „Node löschen, Children hochstufen“ können später folgen.

---

# 70. Goal ↔ Learning Path

Da ein Path mehreren Zielen dienen kann, wird fachlich bevorzugt:

```text
GoalLearningPath
```

statt nur `LearningPath.GoalId`.

Dies entspricht dem Lastenheft besser und vermeidet spätere Schemaeinschränkung.

---

# 71. Status und Enums in der UI

Domain-Bezeichnungen Englisch.

UI-Labels Deutsch.

Beispiel:

| Domain | UI |
|---|---|
| Inbox | Eingang |
| Planned | Geplant |
| Started | In Arbeit |
| Paused | Pausiert |
| Deferred | Später |
| Completed | Abgeschlossen |
| Abandoned | Abgebrochen |
| Archived | Archiviert |

---

# 72. Empty States

Beispiel Ziele:

```text
Noch keine Lernziele vorhanden.

Ein Lernziel beschreibt, was du erreichen möchtest.

[Erstes Lernziel anlegen]
```

Inbox:

```text
Die Inbox ist leer.
Neue Links kannst du über „Ressource erfassen“ speichern.
```

Leere UI ist erklärend, nicht nur leer.

---

# 73. Bestätigungsdialoge

Bestätigung nur bei:

- permanentem Löschen,
- Restore,
- Mass Archive,
- konfliktbehaftetem Import,
- Datenbankpfadwechsel.

Normale Bearbeitung wird nicht durch übermäßige Dialoge ausgebremst.

---

# 74. Undo

Kein universelles Undo-System in V1.

Archivierte Objekte sind wiederherstellbar.

Komplexe Edit Dialogs können über „Abbrechen“ ungespeicherte Änderungen verwerfen.

---

# 75. Kontextmenüs

Convenience, nie einzige Möglichkeit.

Resource:

- Öffnen,
- URL öffnen,
- Status setzen,
- Skill zuordnen,
- Path zuordnen,
- Archivieren.

---

# 76. Lookup Dialoge

Bei vielen Skills/Resources genügt eine ComboBox nicht.

V1/V1.x verwendet Suchdialoge.

Beispiel:

```text
Skill suchen: [docker net________]

[x] Docker Networking
[ ] Docker Storage
[ ] Docker Security

[Übernehmen]
```

---

# 77. Resource aus Path heraus erfassen

Path Node → `+ Resource`.

Optionen:

```text
[Vorhandene Ressource auswählen]
[Neue Ressource erfassen]
```

Neue unvollständige Resource darf gleichzeitig:

- mit dem Node verbunden,
- weiterhin Status Inbox

haben.

---

# 78. Status/Progress Interaktion

Wenn Progress > 0 und Status Planned:

UI kann `Started` vorschlagen.

Wenn Progress = 100 und Status Started:

UI kann `Completed` vorschlagen.

Keine stille fachliche Änderung ohne sichtbare Aktion.

---

# 79. Search Normalization

V1:

- Trim,
- case-insensitive soweit sinnvoll,
- Unicode beibehalten.

Keine aggressive Transliteration.

Tests mit:

```text
äöüß
é
中文
日本語
🔐
```

---

# 80. Text- und Größenlimits

Vorgeschlagene technische Grenzen:

- Title: 500 Zeichen,
- Tag: 100,
- Provider Name: 200,
- URL: 4096,
- kurze Begründungen: mehrere KB,
- Knowledge Markdown: SQLite TEXT ohne unnötig kleines Limit.

Exakte Werte vor Schema-Freeze festlegen.

---

# 81. Markdown Security

Falls später Preview:

- raw HTML standardmäßig deaktivieren oder sanitizen,
- externe Bilder nicht ungefragt nachladen,
- Links über sichere URL-Öffnung.

---

# 82. Netzwerk-User-Agent

Metadatenservice verwendet einen ehrlichen User-Agent, z. B.:

```text
SASD-LearningManager/1.0
```

Keine aggressive Abfragefrequenz.

---

# 83. App Diagnostics

Datenpflege/About zeigt:

- App Version,
- Schema Version,
- DB Path,
- DB Size,
- Log Path,
- letztes Backup,
- letzter Integrity Check.

Keine persönlichen Inhaltsdaten in der Diagnoseübersicht.

---

# 84. First Run

Beim ersten Start:

```text
Willkommen beim SASD Learning Manager

[Erstes Lernziel anlegen]
[Ressource erfassen]
[Leere Anwendung öffnen]
```

Kurzhinweis:

> Links können zunächst schnell in der Inbox gespeichert und später klassifiziert werden.

Kein langer Wizard-Zwang.

---

# 85. First-use Erklärung Completion/Mastery

Ein kurzer kontextbezogener Hinweis erklärt:

> Ein abgeschlossener Kurs verändert das Skill-Level nicht automatisch. Skills werden separat bewertet und können durch Evidence belegt werden.

---


# 86. Teststrategie

## 86.1 Ziel

Tests sollen insbesondere Datenverlust, falsche Relationen, falsche Statuslogik und Vermischung von Completion/Mastery verhindern.

Testpyramide:

```text
             UI Smoke
               /\
              /  \
         Integration
            /    \
       Application
          /        \
       Domain Unit
```

## 86.2 Domain Tests

Mindestens:

- Goal Statusregeln,
- Resource Statusübergänge,
- Progressgrenzen,
- Skill-Level-Grenzen,
- Skill Gap,
- Path-Zyklusvermeidung,
- Required/Optional Progress,
- ResourceRelation-Regeln,
- Completion ≠ Mastery,
- Archivierungsregeln.

## 86.3 Application Tests

Mindestens:

- Resource erstellen,
- Quick Capture,
- URL-Dublette,
- Resource klassifizieren,
- Resource einem Skill zuordnen,
- Resource mehreren Paths zuordnen,
- Path Node verschieben,
- Evidence anlegen,
- Skill Assessment,
- Goal/Path Status,
- Suche/Filter,
- Backup/Restore-Orchestrierung.

## 86.4 Infrastructure Integration Tests

Mit temporärer SQLite-DB:

- alle Migrationen,
- CRUD,
- Join Tables,
- Foreign Keys,
- Filterqueries,
- Pagination,
- Backup,
- Restore,
- Import,
- Export,
- Integrity Check.

## 86.5 UI Tests

Automatisierte WinForms-End-to-End-Tests sind teuer und fragil. V1 setzt deshalb auf:

- dünne UI-Logik,
- Application Tests,
- Presenter/Presentation Tests soweit sinnvoll,
- manuelle UI-Smoke- und Accessibility-Checklisten.

Business Rules dürfen nie ausschließlich über UI-Tests abgesichert sein.

---

# 87. Requirement Traceability in Tests

Tests referenzieren Lastenheft-IDs.

Beispiel:

```csharp
[Trait("Requirement", "REQ-F-ACT-011")]
[Fact]
public void CompletingResource_DoesNotChangeSkillMastery()
{
    // ...
}
```

oder dokumentierter Testplan mit Mapping.

---

# 88. Kritische Abnahmetests

## AT-001 – Anbieterübergreifender Learning Path

Path enthält:

- O’Reilly Course,
- YouTube Video,
- LinkedIn Learning Resource,
- Herstellerdokumentation,
- eigenes Lab.

Erwartung:

Alle Ressourcen können gemeinsam in einem Path verwaltet werden.

## AT-002 – Canonical Resource

Eine Resource wird zwei Paths und drei Skills zugeordnet.

Erwartung:

- eine Resource ID,
- keine Kopie,
- Änderungen an Titel/URL sind überall sichtbar.

## AT-003 – Completion ≠ Mastery

Resource:

```text
Status = Completed
Progress = 100 %
```

Erwartung:

Skill-Level bleibt unverändert.

## AT-004 – Quick Capture

Nur URL eingeben.

Erwartung:

- Resource wird gespeichert,
- Status Inbox,
- später klassifizierbar.

## AT-005 – Skill Gap

Ist 2, Ziel 4.

Erwartung:

Gap wird als 2 → 4 dargestellt.

## AT-006 – Evidence

Eigenes Lab anlegen und zwei Skills zuordnen.

Erwartung:

Evidence ist bei beiden Skills sichtbar.

## AT-007 – Archivierung

Resource mit Path-/Skillbeziehungen archivieren.

Erwartung:

- aus aktiven Listen entfernt,
- Historie/Relationen erhalten,
- wiederherstellbar.

## AT-008 – Backup/Restore

Komplexen Datenbestand sichern, verändern, wiederherstellen.

Erwartung:

- Entities,
- Join-Relationen,
- Historie

entsprechen dem Backupzustand.

## AT-009 – Offline

Netzwerk deaktivieren.

Erwartung:

- Start,
- Goals,
- Skills,
- Paths,
- Resources,
- Notes,
- Evidence,
- Suche,
- Backup

funktionieren lokal.

## AT-010 – Path-Hierarchie

Versuch, Parent unter eigenen Descendant zu verschieben.

Erwartung:

Operation wird fachlich abgelehnt.

---

# 89. Security-Testfälle

Mindestens:

1. SQL-Metazeichen in allen Textfeldern.
2. sehr lange URL.
3. `javascript:` als URL.
4. `data:` als URL.
5. manipuliertes JSON.
6. ZIP-Entry `../../file`.
7. beschädigte DB im Backup.
8. falscher Backup-Hash.
9. ungültiger Foreign Key.
10. HTTP Timeout.
11. Redirect-Schleife.
12. extrem große Metadata-Response.
13. fehlende lokale Datei.
14. unerwartete Unicode-Daten.
15. Doppelstart der Anwendung.

---

# 90. Migrationstests

Für jede Migration:

```text
vorheriges Schema
      ↓
repräsentative Testdaten
      ↓
Migration
      ↓
Schema validieren
      ↓
Datenbeziehungen validieren
```

Bei Releases zusätzlich:

```text
Backup alte Version
→ Restore/Upgrade neue Version
→ Migration
→ fachlicher Smoke Test
```

---

# 91. Performance-Testdaten

Generator erstellt mindestens:

- 5.000 Resources,
- 50 Providers,
- 1.500 Skills,
- 500 Topics,
- 500 Paths,
- 10.000 Nodes,
- 20.000+ Relations,
- 10.000 Knowledge Artifacts,
- 5.000 Evidence-Einträge.

Performancewerte werden dokumentiert.

---

# 92. Architekturtests

Automatisierte Prüfung, dass:

- Domain keine Infrastructure Assembly referenziert,
- Domain keine WinForms Assembly referenziert,
- Application keine WinForms Assembly referenziert,
- Infrastructure keine WinForms Assembly referenziert.

Dies kann mit Reflection oder einer kleinen geeigneten Architekturtest-Library realisiert werden.

---

# 93. Build

## 93.1 Reproduzierbarer Build

```powershell
dotnet restore .\SASD.LearningManager.sln
dotnet build .\SASD.LearningManager.sln -c Release --no-restore
dotnet test .\SASD.LearningManager.sln -c Release --no-build
```

## 93.2 Qualitätsziel

Release-Build:

```text
0 Errors
0 Warnings
```

Dokumentierte, bewusst akzeptierte Ausnahmen sind möglich, sollen aber selten sein.

## 93.3 Nullable

```xml
<Nullable>enable</Nullable>
```

## 93.4 Warnings as Errors

Nach Bootstrap/Stabilisierung:

```xml
<TreatWarningsAsErrors>true</TreatWarningsAsErrors>
```

---

# 94. Codequalität

## 94.1 XML-Kommentare

Öffentliche fachliche APIs und nichttriviale Services erhalten XML-Dokumentation.

## 94.2 Inline-Kommentare

Kommentare erklären:

- fachliche Gründe,
- Architekturentscheidungen,
- ungewöhnliches Verhalten,
- Sicherheitsgrenzen.

Kommentare sollen nicht den offensichtlichen Code wiederholen.

## 94.3 Sprache

- Code und technische Namen: Englisch.
- UI: primär Deutsch.
- Dokumentation: primär Deutsch.
- Lokalisierung soll später möglich bleiben.

## 94.4 `.editorconfig`

Repository enthält ein gemeinsames `.editorconfig`.

---

# 95. Dependencies

## 95.1 Grundsatz

So wenige externe Bibliotheken wie sinnvoll.

Neue Dependency wird geprüft auf:

- Lizenz,
- Wartungszustand,
- Security,
- Transitive Dependencies,
- Nutzen,
- Ersetzbarkeit.

## 95.2 Keine unnötigen Frameworks

V1 benötigt nicht zwingend:

- MediatR,
- AutoMapper,
- großes ORM,
- UI Theme Framework,
- Message Broker,
- CQRS Framework.

Eine Library wird nur eingeführt, wenn sie ein reales Problem sinnvoll löst.

---

# 96. CI mit GitHub Actions

Pipeline:

```text
Checkout
→ Setup .NET
→ Restore
→ Build
→ Test
→ optional Analyzer/Format Gate
→ Publish Test Results / Build Artifact
```

Pull Requests müssen mindestens:

- erfolgreich bauen,
- Tests bestehen.

Später ergänzbar:

- Dependency Review,
- Secret Scan,
- Security Analyzer,
- Packaging Smoke Test.

---

# 97. Versions- und Releasekonzept

## 97.1 Semantic Versioning

Beispiele:

```text
0.1.0  Bootstrap / Milestone
0.5.0  erste intern produktiv nutzbare Vorschau
0.9.0  Release Candidate Phase
1.0.0  erste stabile Version
```

## 97.2 About Dialog

Zeigt:

- Produktname,
- Version,
- Build/Commit optional,
- Lizenz,
- Repository/Website,
- Datenordner,
- Logordner.

---

# 98. Deployment

## 98.1 Entwicklungs-/Pilotbuild

Beispiel:

```powershell
dotnet publish -c Release -r win-x64 --self-contained false
```

Self-contained kann alternativ geprüft werden.

## 98.2 Installer

Für 1.0 später per ADR:

- MSIX,
- WiX,
- vergleichbares etabliertes Installer-System.

Keine voreilige Installer-Komplexität in Milestone 0.

## 98.3 Datenpfad

Benutzerdaten niemals in `Program Files` oder Installationsordner.

---

# 99. Upgrade

Ablauf:

```text
neue App-Version
      ↓
Start
      ↓
Migration Safety / Backup
      ↓
Schema Migration
      ↓
Application Start
```

Downgrade wird nicht generell garantiert.

Vor migrationskritischen Updates muss Restore möglich sein.

---

# 100. Release Blocker

Folgende Fehler blockieren ein produktives Release:

- Datenverlust,
- fehlerhaftes Backup,
- fehlerhafter Restore,
- halb angewandte Migration,
- Relationenkorruption,
- Canonical Resource wird unbeabsichtigt dupliziert,
- Course Completion verändert Skill Mastery automatisch,
- SQL Injection,
- Zip Slip,
- App startet auf korrupter DB ohne sichere Fehlerbehandlung,
- Kernworkflow nicht offline nutzbar.

Nicht blockierend für V1:

- kein Dark Mode,
- kein PDF Reader,
- kein Browser Plugin,
- keine AI,
- kein Drag & Drop,
- keine semantische Suche.

---

# 101. Repository-Struktur

```text
/
├── .github/
│   └── workflows/
├── docs/
│   ├── requirements/
│   │   ├── Lastenheft.md
│   │   └── Pflichtenheft.md
│   ├── architecture/
│   ├── decisions/
│   ├── testing/
│   ├── security/
│   ├── operations/
│   └── user/
├── src/
│   ├── SASD.LearningManager.Domain/
│   ├── SASD.LearningManager.Application/
│   ├── SASD.LearningManager.Infrastructure/
│   └── SASD.LearningManager.WinForms/
├── tests/
│   ├── SASD.LearningManager.Domain.Tests/
│   ├── SASD.LearningManager.Application.Tests/
│   ├── SASD.LearningManager.Infrastructure.Tests/
│   └── SASD.LearningManager.Architecture.Tests/
├── tools/
├── .editorconfig
├── .gitignore
├── CHANGELOG.md
├── LICENSE
├── README.md
└── SASD.LearningManager.sln
```

---

# 102. Domain-Ordnerstruktur

```text
Domain/
├── Goals/
├── Competencies/
├── Skills/
├── LearningNeeds/
├── LearningPaths/
├── Resources/
├── Providers/
├── Knowledge/
├── Evidence/
├── Tags/
├── Relations/
└── Common/
```

Keine einzige unspezifische `Models`-Mappe für alles.

---

# 103. Application-Ordnerstruktur

```text
Application/
├── Goals/
│   ├── Commands/
│   ├── Queries/
│   └── Dtos/
├── Skills/
├── LearningPaths/
├── Resources/
├── Knowledge/
├── Evidence/
├── Dashboard/
├── Search/
├── Backup/
├── ImportExport/
└── Abstractions/
```

---

# 104. Infrastructure-Ordnerstruktur

```text
Infrastructure/
├── Persistence/
│   ├── SqliteConnectionFactory.cs
│   ├── Repositories/
│   ├── Queries/
│   └── Migrations/
├── Backup/
├── ImportExport/
├── Files/
├── Web/
├── Logging/
└── Configuration/
```

---

# 105. WinForms-Ordnerstruktur

```text
WinForms/
├── Forms/
├── Views/
│   ├── DashboardView.cs
│   ├── GoalsView.cs
│   ├── SkillsView.cs
│   ├── LearningPathsView.cs
│   ├── ResourcesView.cs
│   ├── InboxView.cs
│   ├── KnowledgeView.cs
│   ├── EvidenceView.cs
│   └── MaintenanceView.cs
├── Dialogs/
├── Controls/
├── Navigation/
├── Presentation/
└── Resources/
```

---

# 106. ADRs

Mindestens folgende Entscheidungen sollen separat dokumentiert werden:

1. **ADR-001 – WinForms als V1-Oberfläche**
2. **ADR-002 – SQLite als lokale Persistenz**
3. **ADR-003 – explizites SQL/Repositories statt Full ORM**
4. **ADR-004 – Layered Modular Monolith**
5. **ADR-005 – Canonical Resource**
6. **ADR-006 – Completion/Mastery/Retention getrennt**
7. **ADR-007 – Markdown als Knowledge-Source-Format**
8. **ADR-008 – Archive over Delete**
9. **ADR-009 – kein AI im V1-Core**
10. **ADR-010 – Schema Migration Strategy**
11. **ADR-011 – TEXT vs. INTEGER für persistierte Enums**
12. **ADR-012 – SQLite WAL und Backupstrategie**

---

# 107. Requirement-to-Design-Mapping

| Lastenheft | Technische Umsetzung |
|---|---|
| REQ-F-GOAL-001 | Goal Entity, GoalService, GoalsView |
| REQ-F-GOAL-006 | GoalSkill |
| REQ-F-GOAL-007 | GoalLearningPath |
| REQ-F-SKILL-006 | SkillAssessment |
| REQ-F-SKILL-007 | Skill.TargetLevel |
| REQ-F-SKILL-008 | SkillGap Query |
| REQ-F-SKILL-012 | getrennte Resource/Skill-Modelle |
| REQ-F-NEED-001 | LearningNeed Entity |
| REQ-F-PATH-003 | LearningPathNode Hierarchie |
| REQ-F-PATH-006 | LearningPathNodeResource |
| REQ-F-PATH-007 | Canonical Resource + Join |
| REQ-F-PATH-008 | LearningPathNode.IsRequired |
| REQ-F-PATH-009 | SortOrder |
| REQ-F-PATH-010 | LearningPathNodeRelation.Requires |
| REQ-F-PATH-011 | AlternativeTo |
| REQ-F-PATH-014 | PathProgressService |
| REQ-F-PATH-019 | TreeView Path Editor |
| REQ-F-PATH-023 | providerneutrale Resource-Zuordnung |
| REQ-F-PATH-024 | Node ohne externe Resource möglich |
| REQ-F-RES-002 | kanonische Resource Entity |
| REQ-F-RES-003 | Provider Entity |
| REQ-F-RES-004 | Resource.Url |
| REQ-F-RES-005 | Resource.LocalPath |
| REQ-F-RES-017 | ResourceTag |
| REQ-F-RES-018 | ResourceSkill |
| REQ-F-RES-020 | LearningPathNodeResource |
| REQ-F-RES-021 | ResourceRelation |
| REQ-F-RES-026 | NormalizedUrl Dublettenprüfung |
| REQ-F-RES-030 | Rückverweise in ResourceDetail |
| REQ-F-CAP-001 | QuickCaptureDialog |
| REQ-F-CAP-002 | minimale Capture-Validierung |
| REQ-F-CAP-003 | ResourceStatus.Inbox |
| REQ-F-CAP-004 | Inbox-Klassifikationsworkflow |
| REQ-F-ACT-001 | ResourceStatus |
| REQ-F-ACT-002 | ProgressPercent |
| REQ-F-ACT-004 | StartedAtUtc |
| REQ-F-ACT-005 | CompletedAtUtc |
| REQ-F-ACT-011 | Mastery wird nicht geändert |
| REQ-F-ACT-013 | NextAction |
| REQ-F-KNOW-001 | KnowledgeArtifact ↔ Resource |
| REQ-F-KNOW-002 | eigenständige KnowledgeArtifact Entity |
| REQ-F-KNOW-003 | KnowledgeArtifactSkill |
| REQ-F-KNOW-006 | Quellenrelation |
| REQ-F-KNOW-007 | Markdown Export |
| REQ-F-EVD-001 | Evidence Entity |
| REQ-F-EVD-002 | EvidenceSkill |
| REQ-F-EVD-006 | URL/LocalPath |
| REQ-F-EVD-008 | Evidence Tab in SkillDetail |
| REQ-F-MAST-001 | SkillAssessment getrennt von Resource |
| REQ-F-MAST-002 | AssessSkill Use Case |
| REQ-F-MAST-005 | Assessment History |
| REQ-F-RET-001 | Skill.LastUsedAtUtc |
| REQ-F-RET-002 | Skill.NextReviewAtUtc |
| REQ-F-TAG-001 | ResourceTag |
| REQ-F-REL-001 | ResourceRelation |
| REQ-F-SRCH-001 | SearchService |
| REQ-F-SRCH-009 | kombinierbarer Filter DTO |
| REQ-F-SRCH-011 | InboxView |
| REQ-F-SRCH-016 | SkillGap View |
| REQ-F-DASH-001 | DashboardView |
| REQ-F-DASH-006 | NextAction Query |
| REQ-F-PLAN-005 | „Als Nächstes“ |
| REQ-F-MAINT-003 | Duplicate Maintenance Query |
| REQ-F-MAINT-008 | Soft Archive |
| REQ-F-IO-001 | BackupService |
| REQ-F-IO-002 | RestoreService |
| REQ-F-IO-003 | JSON/CSV Export |
| REQ-F-IO-004 | Markdown Export |
| REQ-F-INT-001 | sicherer Browserstart |
| REQ-F-INT-002 | keine Provider-Credentials |
| REQ-F-AI-001 | AI-unabhängiger Core |
| REQ-Q-PERF-003 | lokale SQLite-Architektur |
| REQ-Q-REL-003 | Backup-/Restore-Tests |
| REQ-Q-PORT-001 | versionierte offene Exporte |
| REQ-Q-MAINT-002 | Domain-/Application-Tests |
| REQ-SEC-PRIV-001 | local-first, kein Cloud-Zwang |
| REQ-SEC-003 | keine Provider-Passwortspeicherung |
| REQ-OPS-004 | lokales Logging |
| REQ-OPS-005 | versionierte DB-Migrationen |

---

# 108. Milestone-Plan

## Milestone 0 – technische Baseline

Lieferumfang:

- Solution-Struktur,
- Git Repository,
- CI,
- DI/Generic Host,
- Logging,
- SQLite Connection,
- Migration Runner,
- MainForm Skeleton,
- Testprojekte,
- Baseline-Dokumentation.

**Exit:**

```text
restore/build/test grün
MainForm startet
leere DB wird erzeugt
Migration 0001 wird protokolliert
```

## Milestone 1 – Provider und Resources

- Provider CRUD,
- Resource CRUD,
- Status,
- Progress,
- URL öffnen,
- Tags Basis,
- ResourcesView,
- ResourceDetail.

## Milestone 2 – Quick Capture und Inbox

- QuickCaptureDialog,
- URL Normalization,
- Dublettenwarnung,
- InboxView,
- Klassifizierung.

## Milestone 3 – Goals und Skills

- Goals,
- Competency Areas,
- Topics,
- Skills,
- Target Level,
- Skill Assessment,
- Skill Gap,
- GoalSkill.

## Milestone 4 – Learning Paths

- LearningPath CRUD,
- Node Tree,
- Sortierung,
- Required/Optional,
- Skill Assignment,
- Resource Assignment,
- Progress.

## Milestone 5 – Knowledge und Evidence

- Knowledge Artifacts,
- Resource Notes,
- Evidence,
- EvidenceSkill,
- Assessment mit Evidence.

## Milestone 6 – Dashboard, Suche, Datenpflege

- Dashboard,
- globale Suche,
- kombinierte Filter,
- nächste Aktionen,
- Maintenance Views.

## Milestone 7 – Backup/Restore und V1-Härtung

- Backup,
- Restore,
- Integrity Check,
- Migrationstests,
- Performance,
- DPI/Accessibility,
- Security Review,
- Release Candidate.

---

# 109. Definition of Done pro Milestone

Ein Milestone gilt nicht als fertig, nur weil UI sichtbar ist.

Erforderlich:

- Requirement IDs zugeordnet,
- Build grün,
- Tests grün,
- fachliche Regeln getestet,
- Migrationen getestet,
- keine Business Logic ausschließlich in UI,
- Fehlerpfade geloggt,
- relevante Dokumente aktualisiert,
- manueller Smoke Test,
- reproduzierbarer Git-Stand.

---

# 110. V1-End-to-End-Abnahme

```text
1. Anwendung frisch starten.
2. Goal „Cloud Engineer“ anlegen.
3. Skill „Docker Networking“ anlegen.
4. Ist-Level 2 und Ziel-Level 4 setzen.
5. Learning Path „Docker Refresher“ anlegen.
6. Node „Networking“ anlegen.
7. O’Reilly-Link über Quick Capture speichern.
8. Resource aus Inbox klassifizieren.
9. Provider O’Reilly zuweisen.
10. Resource dem Skill zuordnen.
11. Resource dem Path Node zuordnen.
12. Resource auf Started setzen.
13. Progress 50 % setzen.
14. Knowledge Note erfassen.
15. Resource auf Completed setzen.
16. Prüfen: Skill-Level bleibt 2.
17. Course Completion Evidence anlegen.
18. eigenes Lab als weitere Evidence anlegen.
19. Skill neu auf Level 3 bewerten.
20. Dashboard prüfen.
21. Suche/Filter prüfen.
22. Resource in zweitem Path verwenden.
23. Prüfen: kein Resource-Duplikat.
24. Backup erstellen.
25. Daten verändern.
26. Restore durchführen.
27. Beziehungen und Historie prüfen.
28. Netzwerk deaktivieren.
29. lokale Kernfunktionen erneut prüfen.
```

Wenn diese Kette stabil funktioniert, ist der technische V1-Kern erreicht.

---

# 111. Qualität der V1

V1 soll folgende Eigenschaften besitzen:

1. **verständlich** – Begriffe und Navigation sind konsistent.
2. **schnell** – Quick Capture stört den Arbeitsfluss kaum.
3. **fokussiert** – Dashboard zeigt aktuelle Arbeit statt Statistikflut.
4. **nachvollziehbar** – Resource zeigt ihren fachlichen Kontext.
5. **konsistent** – Completion/Mastery/Retention bleiben getrennt.
6. **robust** – Datenintegrität und Backup sind priorisiert.
7. **portabel** – Export ist vorgesehen.
8. **privat** – keine Cloudpflicht.
9. **testbar** – Fachkern ohne UI prüfbar.
10. **erweiterbar** – AI und Integrationen liegen außerhalb des Kerns.

---

# 112. Scope Guard für neue V1-Funktionen

Neue Funktion kommt nur in V1, wenn sie wesentlich hilft, mindestens eine dieser Fragen zu beantworten:

1. Was möchte ich lernen?
2. Welche Skills fehlen?
3. Welche Ressourcen habe ich?
4. Wie gehören Resources, Skills und Paths zusammen?
5. Woran arbeite ich gerade?
6. Was habe ich abgeschlossen?
7. Welche Evidence habe ich?
8. Was kann ich besser als vorher?
9. Wie sichere ich meine Lernhistorie?

Wenn nicht, wird sie V1.x/V2 zugeordnet.

---

# 113. Bewusst nicht in V1

- eigener PDF Reader,
- eigener Video Player,
- Video Hosting,
- Browser Extension,
- Webarchivierungsengine,
- semantische Vektorsuche,
- AI Chat,
- AI Skill Assessment,
- automatisches AI Tagging,
- Provider APIs,
- Cloud Sync,
- Anki-/FSRS-System,
- öffentliche Community,
- Team-/HR-Funktionen,
- SCORM Authoring,
- Kursmarktplatz,
- Gamification-Rankings,
- komplexe visuelle Graphengine.

---

# 114. Spätere AI-Architektur

AI bleibt optionaler Adapter.

Mögliche Abstraktion:

```csharp
public interface ILearningAssistant
{
    Task<IReadOnlyList<TagSuggestion>> SuggestTagsAsync(...);
    Task<IReadOnlyList<SkillSuggestion>> SuggestSkillsAsync(...);
    Task<ResourceSummarySuggestion> SummarizeAsync(...);
}
```

Suggestion besitzt:

```text
Suggested
Accepted
Rejected
```

Kein AI-Service darf ungefragt fachliche Daten oder Skill-Level überschreiben.

---

# 115. Spätere Provider-Integrationen

Adaptermodell:

```text
IProviderIntegration
```

Mögliche Capabilities:

```text
GetMetadata
GetProgress
GetCompletion
```

Kern-Domain kennt keine konkrete O’Reilly- oder LinkedIn-Klasse.

Jede Integration:

- separat optional,
- rechtlich/Nutzungsbedingungen prüfen,
- Credentials sicher behandeln,
- Datenfluss dokumentieren.

---

# 116. Spätere Browser Extension

V2-Optionen:

- Custom URI Scheme,
- lokal abgesicherter IPC/localhost-Endpunkt,
- Zwischenablage/Importfile.

V1 startet hierfür keinen unnötigen Webserver.

---

# 117. Spätere Webarchivierung

Nur als separates Modul nach Bewertung von:

- Urheberrecht/Nutzungsbedingungen,
- Storage,
- Malware-/Active-Content-Risiken,
- Hashing,
- Größenlimits,
- Backupauswirkung.

---

# 118. Technische Risiken

## TR-01 – Business Logic in Forms

**Risiko:** WinForms wird unwartbar.

**Gegenmaßnahme:** Application Layer, Services, Presenter, Architekturtests.

## TR-02 – Relationenschema wird komplex

**Gegenmaßnahme:** klare Join Tables, spezialisierte Queries, Integrationstests.

## TR-03 – Path Editor verschlingt Zeit

**Gegenmaßnahme:** TreeView + Buttons zuerst; Drag & Drop später.

## TR-04 – Search wird langsam

**Gegenmaßnahme:** Indizes, SQL-Filter, Pagination, FTS5 erst bei Bedarf.

## TR-05 – WAL-Backup inkonsistent

**Gegenmaßnahme:** SQLite Backup API, Restore-Tests.

## TR-06 – Skill-Level wirkt objektiver als es ist

**Gegenmaßnahme:** qualitative Textstufen und Evidence anzeigen.

## TR-07 – lokale Pfade brechen

**Gegenmaßnahme:** Missing-File-Status; Resource nicht löschen.

## TR-08 – Scope Creep

**Gegenmaßnahme:** V1 Scope Guard und ADRs.

---

# 119. Offene technische Entscheidungen

| ID | Entscheidung | Empfehlung | Zieltermin |
|---|---|---|---|
| Q-T-001 | Skill-Level `null+1..5` oder `0..5` | `null+1..5` | vor Schema Freeze |
| Q-T-002 | Enum-Persistenz TEXT/INTEGER | TEXT | ADR vor Migration 0001 |
| Q-T-003 | WAL Mode | testen, wahrscheinlich ja | vor Backup |
| Q-T-004 | Markdown Preview Library | keine für V1 | V1.x |
| Q-T-005 | Installer | später MSIX/WiX evaluieren | vor 1.0 |
| Q-T-006 | Managed Attachments | V1 nur LocalPath | V2 |
| Q-T-007 | FTS5 | erst nach Messung | V1.x |
| Q-T-008 | Architecture Test Library | klein/pragmatisch | Milestone 0 |
| Q-T-009 | DateTime Storage | ISO-8601 UTC TEXT | vor Migration 0001 |
| Q-T-010 | Goal↔Path | Many-to-many | vor Schema Freeze |

---

# 120. Empfohlene nachgelagerte Dokumente

Nach Review des Pflichtenhefts:

1. `PROJECT-BRIEF.md`
2. `ARCHITECTURE.md`
3. `DATA-MODEL.md`
4. ADR-001 bis ADR-012
5. `TEST-STRATEGY.md`
6. `SECURITY.md`
7. `BACKUP-RESTORE.md`
8. `MILESTONES.md`
9. `USER-GUIDE.md`
10. Coding-/Debugging-Chat-Übergabeprompt.

Nachgelagerte Dokumente sollen referenzieren statt Inhalte unnötig zu duplizieren.

---

# 121. Freigabekriterien für dieses Pflichtenheft

Vor Implementierungsfreigabe prüfen:

- [ ] V1 Scope akzeptiert
- [ ] Domain Model akzeptiert
- [ ] WinForms akzeptiert
- [ ] Layered Architecture akzeptiert
- [ ] SQLite akzeptiert
- [ ] explizites SQL/Repository akzeptiert
- [ ] Skill-Level-Modell akzeptiert
- [ ] Topic vs Skill akzeptiert
- [ ] Canonical Resource akzeptiert
- [ ] Completion/Mastery/Retention akzeptiert
- [ ] Backup/Restore-Konzept akzeptiert
- [ ] Export/Portabilität akzeptiert
- [ ] Teststrategie akzeptiert
- [ ] Security-/Privacy-Baseline akzeptiert
- [ ] ADR-Liste vollständig genug

---

# 122. Änderungsverlauf

| Datum | Version | Bereich | Änderung | Grund |
|---|---|---|---|---|
| 2026-08-27 | 0.1 | Gesamt | Erstfassung | aus Lastenheft und Research abgeleitet |

---

# 123. Freigabe

**Dokumentstatus:** Proposed  
**Technische Freigabe:** offen  
**Produktfreigabe:** offen  
**Bekannte Abweichungen:** keine freigegebenen Abweichungen  
**Nächster Gate:** Strategie-/Architekturreview

---

# Anhang A – Gesamtarchitektur

```text
┌──────────────────────────────────────────────────────────────┐
│                    SASD Learning Manager                     │
│                                                              │
│  ┌──────────────────── WinForms ──────────────────────────┐  │
│  │ MainForm · Views · Dialogs · Navigation · Presentation│  │
│  └──────────────────────────┬──────────────────────────────┘  │
│                             │                                 │
│  ┌──────────────────── Application ────────────────────────┐  │
│  │ Use Cases · Queries · Services · DTOs · Interfaces     │  │
│  └──────────────────────────┬──────────────────────────────┘  │
│                             │                                 │
│  ┌────────────────────── Domain ───────────────────────────┐  │
│  │ Goals · Skills · Paths · Resources · Evidence · Rules  │  │
│  └─────────────────────────────────────────────────────────┘  │
│                             ▲                                 │
│                             │                                 │
│  ┌────────────────── Infrastructure ───────────────────────┐  │
│  │ SQLite · Repositories · Backup · Export · HTTP · Files │  │
│  └─────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────┘

                    externe Systeme
                         │
              ┌──────────┴──────────┐
              ▼                     ▼
        Standardbrowser         Dateisystem
       O’Reilly etc.           PDFs/Bücher/etc.
```

---

# Anhang B – Kern-Domain

```text
Goal ───────────────┬──────────── Skill
 │                  │               │
 │                  │               ├── SkillAssessment
 │                  │               ├── Evidence
 │                  │               └── KnowledgeArtifact
 │                  │
 │                  ▼
 │             LearningPath
 │                  │
 │                  ▼
 │            LearningPathNode
 │                  │
 │             ┌────┴───────┐
 │             ▼            ▼
 │           Skill       Resource
 │                          │
 │                   ┌──────┼────────┐
 │                   ▼      ▼        ▼
 │                Provider Tags  Relations
 │
 └──────────────────────── Knowledge/Evidence
```

---

# Anhang C – Completion / Mastery / Retention

```text
RESOURCE
O'Reilly Docker Course
Status: Completed
Progress: 100 %
Completed: 27.08.2026
        │
        │ kann Evidence liefern
        ▼
EVIDENCE
Course Completion
        │
        │ unterstützt Bewertung
        ▼
SKILL
Docker Networking
Current Level: 3
Target Level: 4
        │
        │ zeitliche Dimension
        ▼
RETENTION
Last used: 12.05.2026
Next review: 01.10.2026
```

Keine Ebene ersetzt die andere.

---

# Anhang D – V1 Navigation

```text
SASD Learning Manager
├── Heute
├── Ziele
├── Lernpfade
├── Skills
├── Ressourcen
│   ├── Alle
│   ├── Geplant
│   ├── In Arbeit
│   ├── Abgeschlossen
│   └── Archiv
├── Inbox
├── Wissen
├── Evidence
├── Suche
├── Datenpflege
└── Einstellungen
```

---

# Anhang E – Resource Detail Beispiel

```text
Linux Performance Optimization
──────────────────────────────────────────────────────────────

Typ:          Course
Provider:     O'Reilly
Status:       Completed
Fortschritt:  100 %
Priorität:    High
Dauer:        8 h
Sprache:      English

URL:
https://...

Warum gespeichert:
Vorbereitung auf Linux Performance / EX442.

Skills:
• Linux Performance
• CPU Diagnostics
• Memory Diagnostics

Learning Paths:
• EX442 Preparation
• Linux Troubleshooting

Relations:
• Overlaps with: Udemy EX442 Course

[Übersicht] [Lernstatus] [Zuordnungen]
[Beziehungen] [Notizen] [Evidence] [Historie]
```

---

# Anhang F – Path Editor Beispiel

```text
Linux Performance
──────────────────────────────────────────────────────────────

Core Progress: 6 / 10 required nodes complete

▼ CPU
  ✓ Load Average
  ✓ Scheduler
  ▶ perf
  ○ Flame Graphs
▼ Memory
  ✓ Virtual Memory
  ○ NUMA
  ○ OOM
▶ Storage
  ○ iostat
  ○ latency

Selected Node: perf
──────────────────────────────────────────────────────────────
Required: Yes
Skill: Linux CPU Diagnostics

Resources:
• O'Reilly EX442 – perf chapter
• Brendan Gregg perf documentation

Next action:
Run perf stat / record / report lab.

[+ Resource] [+ Skill] [Relation] [Edit]
```

---

# Anhang G – Datenverzeichnis

```text
%LOCALAPPDATA%\SASD\LearningManager\
├── data\
│   └── learning-manager.db
├── logs\
│   └── learning-manager-2026-08-27.log
├── backups\
│   └── SASD-LearningManager-Backup-20260827-074500.zip
└── settings.json
```

---

# Anhang H – ADR-Dateien

```text
ADR-001-winforms-ui.md
ADR-002-sqlite-persistence.md
ADR-003-explicit-sql-repositories.md
ADR-004-layered-architecture.md
ADR-005-canonical-resource.md
ADR-006-completion-mastery-retention.md
ADR-007-markdown-knowledge-format.md
ADR-008-archive-over-delete.md
ADR-009-no-ai-v1-core.md
ADR-010-schema-migrations.md
ADR-011-persisted-enum-format.md
ADR-012-sqlite-wal-backup.md
```

---

# Anhang I – Technische Definition „produktiv nutzbar“

Eine Version gilt für den persönlichen Pilotbetrieb als produktiv nutzbar, wenn:

- der Kernworkflow vollständig funktioniert,
- reale Daten sicher gespeichert werden,
- Backup und Restore getestet sind,
- Migrationen reproduzierbar laufen,
- keine bekannten datenverlustkritischen Fehler existieren,
- Completion/Mastery nicht vermischt werden,
- Search/Filter ausreichend schnell sind,
- Logs für Fehlersuche vorhanden sind,
- Betrieb offline möglich ist,
- Build und Tests reproduzierbar sind,
- relevante Dokumentation aktualisiert ist.

---

# Anhang J – Bezug zum SASD Development Standard

Das Projekt soll die anwendbaren Regeln des SASD Development Standard proportional erfüllen. Das bedeutet insbesondere:

- Anforderungen bleiben mit IDs rückverfolgbar.
- Architekturentscheidungen werden dokumentiert.
- Build und Tests sind reproduzierbar.
- Security und Privacy werden nicht erst am Ende betrachtet.
- Releases erhalten nachvollziehbare Evidenz.
- Dokumentation dient Wartbarkeit und Wissenserhalt.
- AI-Unterstützung wird – falls später eingesetzt – kontrolliert und nachvollziehbar eingebunden.

Der Standard ist kein Runtime-Dependency des Produkts; er definiert Vorgehen, erwartete Ergebnisse und Evidenz.

---

**Ende des Pflichtenhefts**
