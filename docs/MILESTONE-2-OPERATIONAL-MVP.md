# Milestone 2 – Operational MVP / v0.1.0

## Ziel

Milestone 2 macht aus der technischen Kernakte des ersten Milestones eine Anwendung, die den
Arbeitsalltag einer persönlichen Jobsuche aktiv unterstützt. Das Dashboard beantwortet nicht mehr
nur „welchen Status hat etwas?“, sondern vor allem:

- Was muss ich selbst als Nächstes tun?
- Was ist überfällig?
- Auf wessen Rückmeldung warte ich?
- Welche Termine stehen an?
- Welche Jobsuchen muss ich prüfen?

Der fachliche Leitgedanke bleibt:

> **Der nächste Schritt ist wichtiger als der aktuelle Status.**

## Neue Domain-Objekte

### `Activity`

`Activity` ist der universelle Timeline-Eintrag für bereits stattgefundene und geplante Ereignisse.
Unterstützt werden unter anderem E-Mail, Telefonat, LinkedIn, Bewerbung versendet, Interview,
Meeting, Notiz und Behördentermin.

Historische Einträge haben den Status `Recorded`. Geplante Einträge beginnen als `Planned` und
können anschließend als `Completed` oder `Cancelled` abgeschlossen werden. Eine Aktivität kann
optional mit Opportunity, Bewerbung, Kontakt und Organisation verknüpft werden.

### `TrackerTask`

Der technische Klassenname lautet bewusst `TrackerTask`, um nicht mit `System.Threading.Tasks.Task`
zu kollidieren. Fachlich gibt es genau die beiden für den frühen Workflow wichtigen Arten:

- `ACTION`: Der Benutzer muss selbst etwas tun.
- `WAITING_FOR`: Der nächste Ball liegt bei einer anderen Person oder Organisation.

Aufgaben können eine Fälligkeit haben, müssen aber keine besitzen. Offene Aufgaben ohne Fälligkeit
bleiben im Heute-Cockpit sichtbar, damit sie nicht unbemerkt verschwinden.

### `SearchProfile`

Ein SearchProfile speichert eine manuell zu prüfende Jobsuche oder Karriereseite mit URL,
Prüfintervall, letzter und nächster Prüfung. „Heute geprüft“ setzt den letzten Prüfzeitpunkt und
berechnet die nächste Prüfung anhand des konfigurierten Intervalls.

Es gibt in v0.1.0 ausdrücklich kein Scraping und keine Portalautomatisierung.

### `Document`

Ein Dokumentkatalog-Eintrag verweist zunächst auf eine existierende Datei und speichert:

- Typ,
- Bezeichnung,
- Version,
- Sprache,
- optionale Tags,
- Originalpfad,
- Dateigröße,
- SHA-256-Fingerprint.

Die Datei wird beim bloßen Registrieren nicht dupliziert.

### `ApplicationDocumentSnapshot`

Wird eine konkrete Dokumentversion einer Bewerbung zugeordnet, prüft die Anwendung den
SHA-256-Fingerprint erneut. Nur wenn die Quelldatei noch exakt der registrierten Version entspricht,
wird sie in den privaten lokalen Anwendungsbereich kopiert und ein unveränderlicher
Zuordnungs-Snapshot gespeichert.

Private Kopien liegen unter:

```text
%LOCALAPPDATA%\SASD GmbH\SASD Bewerbungsmanager\Documents\<ApplicationId>\
```

Der Dateiname basiert auf dem SHA-256-Hash. Dadurch können Monate später die tatsächlich
verwendeten Unterlagen nachvollzogen werden, selbst wenn das Original inzwischen verändert oder
verschoben wurde.

## Heute / Operational Cockpit

Das Dashboard zeigt zusätzlich zu den kleinen Bestandskennzahlen fünf operative Bereiche:

1. **Überfällige ACTIONs**
2. **Heute / ohne Termin**
3. **WAITING_FOR**
4. **Nächste Termine**
5. **Suchquellen prüfen**

Die Filterung auf `DateTimeOffset` erfolgt nach dem Materialisieren im Speicher. Das ist eine
bewusste SQLite-Entscheidung: Der EF-Core-SQLite-Provider kann `DateTimeOffset` persistieren, aber
nicht alle Vergleiche und Sortierungen serverseitig übersetzen. Für die persönliche Datenmenge ist
die verständliche In-Memory-Auswertung in dieser Version angemessen.

## Neue WinForms-Bereiche

Die Navigation enthält nun:

```text
Heute
Aufgaben
Termine
Verlauf
Suchquellen
Bewerbungen
Stellen
Kontakte
Organisationen
Dokumente
```

### Aufgaben

- ACTION oder WAITING_FOR anlegen
- optionale Fälligkeit
- optionale Zuordnung zu Stelle/Bewerbung/Kontakt/Organisation
- erledigen
- abbrechen

### Termine

- geplante Aktivitäten erfassen
- Interviews, Meetings und Behördentermine über `ActivityKind`
- erledigen
- absagen

### Verlauf

- historische Aktivitäten erfassen
- Termine erfassen
- Timeline mit Art, Status, Zeitpunkt und Notiz

### Suchquellen

- Suchprofil anlegen/bearbeiten
- URL im Standardbrowser öffnen
- „Heute geprüft“
- nächste Prüfung automatisch fortschreiben

### Dokumente

- vorhandene Datei registrieren
- SHA-256 asynchron berechnen
- Version, Sprache und Tags erfassen

### Bewerbungen

Zusätzlich zu Status und Historie:

- registrierte Dokumentversion einer Bewerbung zuordnen
- verwendete immutable Snapshots anzeigen
- „Kontext für ChatGPT kopieren“

## „Kontext für ChatGPT kopieren“

Die Funktion erzeugt lokal und deterministisch einen Text aus gespeicherten Daten. Es findet kein
KI-Aufruf statt. Enthalten sind, soweit vorhanden:

```text
Position
Unternehmen
Vermittler
Kontakte
Status
Rollenbeschreibung
Bisheriger Verlauf
Offene Aufgaben
Warten auf
Verwendete Dokumente
Nächster Termin
```

Der Text wird in die Windows-Zwischenablage kopiert und kann anschließend bewusst in einen
separaten Chat übernommen werden.

## Persistenz und Migration

Neue Migration:

```text
202608260002_OperationalMvp
```

Neue Tabellen:

- `activities`
- `work_items`
- `search_profiles`
- `documents`
- `application_document_snapshots`

Die bestehende Milestone-1-Datenbank wird beim normalen Anwendungsstart durch
`DatabaseInitializer` migriert. Ein Löschen der vorhandenen Datenbank ist nicht vorgesehen.

DbContexts bleiben weiterhin kurzlebig und werden über `IDbContextFactory` erzeugt.

## Testabdeckung dieses Milestones

Neu bzw. erweitert wurden Tests für:

- Domain-Lifecycle von Tasks und Activities
- Fortschreiben eines SearchProfiles
- Today-Cockpit-Selektion
- Erzeugung des ChatGPT-Kontextes
- Dokumentregistrierung und Application-Snapshot-Metadaten
- SQLite-Roundtrip aller neuen Entitäten
- kompletter Kernworkflow inklusive ACTION, WAITING_FOR, Termin und Suchprofil
- Presentation-Labels der neuen Typen
- DI-/Composition-Root-Validierung aller operationalen WinForms-Views

Alle automatisierten Testdaten sind synthetisch.

## Bewusste Grenzen von v0.1.0

Nicht Bestandteil dieses Milestones sind:

- automatische Mailintegration,
- automatischer E-Mail-Import,
- Scraping,
- Browserextension,
- automatisches Bewerben,
- Cloud-Synchronisation,
- Multiuser,
- generative KI im Anwendungskern,
- komplexes Reporting oder Marktstatistik.

Diese Punkte gehören – soweit überhaupt sinnvoll – in spätere Milestones und werden nicht durch
v0.1.0 vorweggenommen.

## Verifikation

Die Code-Erstellungsumgebung dieses Chats besitzt kein .NET-10-SDK. Deshalb kann diese konkrete
Lieferung hier nur statisch geprüft werden. Auf der Windows-Entwicklungsmaschine muss vor dem
Commit ausgeführt werden:

```powershell
dotnet clean .\SASD.Bewerbungsmanager.sln
dotnet restore .\SASD.Bewerbungsmanager.sln
dotnet build .\SASD.Bewerbungsmanager.sln -c Release --no-restore
dotnet test .\SASD.Bewerbungsmanager.sln -c Release --no-build
dotnet run --project .\src\SASD.Bewerbungsmanager.WinForms\SASD.Bewerbungsmanager.WinForms.csproj
```
