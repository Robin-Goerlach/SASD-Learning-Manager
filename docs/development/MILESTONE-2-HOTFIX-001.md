# Milestone 2 Hotfix 001 – xUnit2017

**Datum:** 2026-08-27  
**Status:** Applied – erneuter Windows-Build erforderlich

## Fehler

Der reale Release-Build unter Windows brach in `tests/SASD.LearningManager.Application.Tests/ResourceServiceTests.cs` mit `xUnit2017` ab.

Beanstandeter Testcode:

```csharp
Assert.True(resources.Tags[id].Contains("docker", StringComparer.OrdinalIgnoreCase));
```

Der xUnit-Analyzer verlangt für die Prüfung, ob eine Collection einen Wert enthält, die spezialisierte Collection-Assertion.

## Korrektur

```csharp
Assert.Contains("docker", resources.Tags[id], StringComparer.OrdinalIgnoreCase);
```

Die Assertion behält damit dieselbe fachliche Semantik: Der normalisierte Tag `docker` muss unabhängig von Groß-/Kleinschreibung vorhanden sein.

## Zusatzprüfung

Der gesamte Testbestand wurde nach weiteren `Assert.True(...)`- und `Assert.False(...)`-Collection-Antipatterns durchsucht; nach der Korrektur existieren keine weiteren Treffer dieses Musters. Die bereits vorher korrigierten xUnit1051-Stellen verwenden weiterhin `TestContext.Current.CancellationToken`.

## Verifikation

Auf dem Windows-.NET-8-System erneut ausführen:

```powershell
dotnet clean .\SASD.LearningManager.sln
dotnet restore .\SASD.LearningManager.sln
dotnet build .\SASD.LearningManager.sln -c Release --no-restore
dotnet test .\SASD.LearningManager.sln -c Release --no-build
```
