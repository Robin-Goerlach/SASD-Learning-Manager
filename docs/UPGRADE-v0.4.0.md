# Upgrade auf v0.4.0

## Ausgangspunkt

v0.4.0 baut auf dem verifizierten v0.3.0-Stand auf.

## Datenbank

Die vorhandene lokale Datenbank **nicht löschen**. Beim Start wird automatisch folgende Migration
angewendet:

```text
202608270004_JobSearchAdapters
```

Sie ergänzt ausschließlich die Tabelle `job_leads` und die dazugehörigen Indizes/Fremdschlüssel.

## Prüfung

```powershell
dotnet clean .\SASD.Bewerbungsmanager.sln
dotnet restore .\SASD.Bewerbungsmanager.sln
dotnet build .\SASD.Bewerbungsmanager.sln -c Release --no-restore
dotnet test .\SASD.Bewerbungsmanager.sln -c Release --no-build
dotnet run --project .\src\SASD.Bewerbungsmanager.WinForms\SASD.Bewerbungsmanager.WinForms.csproj
```

Im Hauptfenster erscheint anschließend die neue Navigation **Jobsuche**.
