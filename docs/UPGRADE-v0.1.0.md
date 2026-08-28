# Upgrade-Hinweise auf v0.1.0

## Bestehende Milestone-1-Daten

Die bestehende SQLite-Datenbank kann weiterverwendet werden. Beim nächsten Start führt
`DatabaseInitializer` automatisch die Migration `202608260002_OperationalMvp` aus.

Die produktive Datenbank **nicht löschen**, sofern die vorhandenen Daten erhalten bleiben sollen.

## Einmaliges Legacy-Cleanup bei älteren Overlay-Repositories

Während der Milestone-1-Stabilisierung gab es in manchen überkopierten Repository-Ständen noch
ein altes M0-WinForms-Gerüst parallel zur eigentlichen Oberfläche. Diese Dateien gehören nicht
mehr zum aktuellen Repository:

```text
src\SASD.Bewerbungsmanager.WinForms\MainForm.cs
src\SASD.Bewerbungsmanager.WinForms\Presentation\MainShellPresenter.cs
```

Wenn eine dieser Dateien lokal noch existiert, kann sie vor dem Build gelöscht werden.
**Nicht** löschen:

```text
src\SASD.Bewerbungsmanager.WinForms\Forms\MainForm.cs
```

`Program.cs` verwendet zusätzlich weiterhin einen expliziten Alias für die aktuelle Form, sodass
ein versehentlich liegen gebliebener M0-Typ nicht erneut den Startup-Pfad übernehmen kann.

## Empfohlener Upgrade-Ablauf

```powershell
if (Test-Path .\src\SASD.Bewerbungsmanager.WinForms\MainForm.cs) {
    Remove-Item .\src\SASD.Bewerbungsmanager.WinForms\MainForm.cs
}

if (Test-Path .\src\SASD.Bewerbungsmanager.WinForms\Presentation\MainShellPresenter.cs) {
    Remove-Item .\src\SASD.Bewerbungsmanager.WinForms\Presentation\MainShellPresenter.cs
}

dotnet clean .\SASD.Bewerbungsmanager.sln
dotnet restore .\SASD.Bewerbungsmanager.sln
dotnet build .\SASD.Bewerbungsmanager.sln -c Release --no-restore
dotnet test .\SASD.Bewerbungsmanager.sln -c Release --no-build
dotnet run --project .\src\SASD.Bewerbungsmanager.WinForms\SASD.Bewerbungsmanager.WinForms.csproj
```
