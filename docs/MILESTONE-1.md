# Milestone 1 – Kernakte

## Ziel

Milestone 1 schafft einen stabilen fachlichen und technischen Kern, auf dem die täglich produktive v0.1.0 aufgebaut werden kann. Der Scope bleibt absichtlich kleiner als die langfristige V1-Spezifikation.

## Implementiert

### Domain

- `Organization`
- `Contact`
- `Opportunity`
- `SourceLink`
- `Application`
- `ApplicationStatusHistory`
- fachliche Status- und Kanal-Enums

`Opportunity` und `Application` bleiben ausdrücklich getrennt. Eine interessante Stelle kann daher erfasst werden, bevor eine Bewerbung existiert.

### Rollenbeschreibung als Snapshot

`Opportunity.DescriptionSnapshot` speichert die zum Erfassungszeitpunkt relevante Rollenbeschreibung. Ein externer Link ist nur eine Quelle und nicht die einzige historische Wahrheit.

### Statushistorie

Beim Erzeugen einer `Application` entsteht ein initialer Statushistorieneintrag. Ein späterer Statuswechsel aktualisiert aktuellen Status und History innerhalb desselben EF-Core-`SaveChanges`.

### Persistenz

- SQLite
- EF Core 10
- Migration `202608260001_InitialMilestone1`
- Foreign Keys und Indizes
- produktive Datenbank unter `%LOCALAPPDATA%`
- `IDbContextFactory<ApplicationTrackerDbContext>` statt langlebigem MainForm-DbContext

### WinForms

Grundmuster:

```text
Navigation | Arbeitsbereich
```

Vorhandene Bereiche:

- Heute / Übersicht
- Organisationen
- Kontakte
- Stellen
- Bewerbungen

Milestone 1 verwendet bewusst klassische WinForms-Steuerelemente und programmgesteuerte Layouts statt einer Web-UI-Nachbildung.

### Tests

- Domain: Statushistorie
- Application: Opportunity-Snapshot und Validierung
- Infrastructure: Migration + SQLite-Roundtrip
- Presentation: UI-Labels
- System: Organization → Opportunity → Application → Statuswechsel

Alle automatisierten Daten sind synthetisch.

## Bewusst nicht in Milestone 1

- Activity / Timeline
- Task
- ACTION
- WAITING_FOR
- Terminverwaltung
- SearchProfiles
- Dokumentversionen und SHA-256
- „Kontext für ChatGPT kopieren“
- Mailintegration
- Scraping
- Cloud-Synchronisation
- generative KI

Diese Abgrenzung ist kein fehlender Rest, sondern soll verhindern, dass die erste technische Basis wieder zu einem Großprojekt wird.

## Lokale Prüfung dieser Lieferung

Die Erstellungsumgebung dieser ZIP enthält kein .NET SDK. Deshalb konnte hier **kein echter `dotnet build` oder `dotnet test` ausgeführt werden**. Vor Auslieferung wurden stattdessen Repository-Struktur, Projektverweise, XML/JSON-Dateien und Solution-Inhalte automatisiert statisch geprüft.

Auf einer Windows-.NET-10-Entwicklungsmaschine bitte ausführen:

```powershell
dotnet restore .\SASD.Bewerbungsmanager.sln
dotnet build .\SASD.Bewerbungsmanager.sln -c Release --no-restore
dotnet test .\SASD.Bewerbungsmanager.sln -c Release --no-build
```
