# Build & Verify – Milestone 2

## Voraussetzungen

- Windows 11 oder kompatible Windows-Entwicklungsumgebung
- .NET 8 SDK
- optional Visual Studio 2022 mit .NET Desktop Development Workload

## Vollständiger Nachweis

```powershell
dotnet clean .\SASD.LearningManager.sln
dotnet restore .\SASD.LearningManager.sln
dotnet build .\SASD.LearningManager.sln -c Release --no-restore
dotnet test .\SASD.LearningManager.sln -c Release --no-build
```

Erwartet:

```text
Build succeeded
0 Warning(s)
0 Error(s)
alle Tests grün
```

## Anwendung

```powershell
dotnet run --project .\src\SASD.LearningManager.WinForms\SASD.LearningManager.WinForms.csproj
```

M2-Smoke-Test:

1. `Ctrl+Shift+N` drücken.
2. `https://example.com/learning` speichern.
3. Inbox öffnet sich.
4. Eintrag doppelklicken bzw. `Klassifizieren` wählen.
5. Provider/Typ/Status ergänzen.
6. Speichern.
7. Eintrag verschwindet aus Inbox und bleibt in der Resource Library erhalten.
8. dieselbe URL erneut capturen und Dublettendialog prüfen.

## Behobene Buildfehler aus den Windows-Builds

Dieser Stand enthält die Korrekturen der bisher real beobachteten Compiler-/Analyzerfehler:

- `Program.cs`: voll qualifiziertes `System.Windows.Forms.Application.Run(...)`
- xUnit v3 / xUnit1051: `TestContext.Current.CancellationToken` an cancellable Methoden
- xUnit2017: `Assert.True(resources.Tags[id].Contains(...))` ersetzt durch `Assert.Contains("docker", resources.Tags[id], StringComparer.OrdinalIgnoreCase)`

Nach dem letzten gemeldeten Windows-Build war dies der einzige verbleibende Fehler. Trotzdem gilt erst der erneute vollständige `build`- und `test`-Lauf als endgültiger Nachweis.

## Hinweis zur Erstellungsumgebung

Hier steht weiterhin kein .NET SDK zur Verfügung. Die SQL-Abfragen und Migrationen wurden mit einer echten SQLite-Engine getestet und der Source statisch geprüft. Der definitive Compile-/Testnachweis erfolgt mit den oben genannten Befehlen auf dem Windows-.NET-System oder über GitHub Actions.
