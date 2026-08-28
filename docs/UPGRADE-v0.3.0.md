# Upgrade auf v0.3.0 – Kommunikationsintegration

## Vor dem ersten Start

Bestehende v0.1.x-/v0.2.0-Datenbanken werden in-place migriert. Ein Löschen der produktiven
`application-tracker.db` ist nicht erforderlich und nicht erwünscht.

Für reale Daten empfiehlt sich vor dem ersten Start eine normale Sicherung der lokalen Datenbank.

## Migration

Beim Programmstart führt `DatabaseInitializer` automatisch aus:

```text
202608270003_CommunicationIntegration
```

Dabei wird ausschließlich die neue Tabelle `communication_messages` mit Indizes und optionalen
Fremdschlüsseln angelegt. Bestehende Tabellen werden nicht umgebaut.

## Verifikation

```powershell
dotnet clean .\SASD.Bewerbungsmanager.sln
dotnet restore .\SASD.Bewerbungsmanager.sln
dotnet build .\SASD.Bewerbungsmanager.sln -c Release --no-restore
dotnet test .\SASD.Bewerbungsmanager.sln -c Release --no-build
dotnet run --project .\src\SASD.Bewerbungsmanager.WinForms\SASD.Bewerbungsmanager.WinForms.csproj
```

Wichtig: Wenn Windows Defender ein Test-Assembly blockiert und VSTest `No test is available` meldet,
ist der Testlauf nicht vollständig. Siehe `docs/TROUBLESHOOTING.md`.
