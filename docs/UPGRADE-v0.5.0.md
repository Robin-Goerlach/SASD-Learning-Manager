# Upgrade auf v0.5.0 – Optionale Assistenz

## Voraussetzung

Ausgangspunkt ist der v0.4.0-Stand „Jobsuche und Quellenadapter“.

## Datenbank

Die bestehende SQLite-Datenbank **nicht löschen**. Beim Start führt `DatabaseInitializer` die neue
Migration automatisch aus:

```text
202608270005_AssistantWorkspace
```

Sie ergänzt ausschließlich `assistant_sessions`.

## Datenschutz

v0.5.0 aktiviert keine Netzwerkverbindung zu einem KI-Anbieter. Die neue Assistenz-Seite arbeitet
lokal. Erst wenn der Benutzer „Prompt kopieren“ nutzt und den Text selbst in einen externen Dienst
einfügt, verlässt der Inhalt den Bewerbungsmanager.

## Prüfung nach dem Overlay

```powershell
dotnet clean .\SASD.Bewerbungsmanager.sln
dotnet restore .\SASD.Bewerbungsmanager.sln
dotnet build .\SASD.Bewerbungsmanager.sln -c Release --no-restore
dotnet test .\SASD.Bewerbungsmanager.sln -c Release --no-build
dotnet run --project .\src\SASD.Bewerbungsmanager.WinForms\SASD.Bewerbungsmanager.WinForms.csproj
```

Falls Windows Defender erneut `Infrastructure.Tests.dll` blockiert und VSTest `No test is available`
meldet, ist der Testlauf nicht vollständig. Siehe `docs/TROUBLESHOOTING.md`.
