# Build Verification – SASD Learning Manager

**Stand:** 2026-09-02

## Historische Baselines

### Milestone 3

Vom Nutzer auf Windows bestätigt:

```text
Build succeeded
0 Warning(s)
0 Error(s)
48 / 48 Tests grün
```

### Milestone 4 Hotfix 001

Vom Nutzer auf Windows bestätigt:

```text
Build succeeded
0 Warning(s)
0 Error(s)

Domain.Tests          23 passed
Application.Tests     26 passed
Infrastructure.Tests  11 passed
Architecture.Tests     4 passed
Total                 64 passed
```

Der Hotfix beseitigte den früheren Generator-/Serialisierungsfehler mit literal geschriebenen `\n`-Sequenzen in `TestDoubles.cs`.

## Milestone 5 / Import-Export Review

Branch:

```text
feature/import-export-review
```

GitHub Actions führt auf `windows-latest` mit .NET 8 aus:

```powershell
dotnet restore .\SASD.LearningManager.sln
dotnet build .\SASD.LearningManager.sln -c Release --no-restore
dotnet test .\SASD.LearningManager.sln -c Release --no-build
```

Ein Zwischenstand des Branches wurde am 02.09.2026 bereits erfolgreich mit folgendem Ergebnis geprüft:

```text
Build succeeded.
0 Warning(s)
0 Error(s)

Domain.Tests          27 passed
Application.Tests     30 passed
Infrastructure.Tests  12 passed
Architecture.Tests     4 passed
Total                 73 passed
Failed                 0
```

Danach wurden drei zusätzliche Application-Tests für das konkrete Resource-CSV-Importverhalten ergänzt. Für den finalen Branch werden daher **76 grüne Tests** erwartet; die letzte GitHub-Actions-Ausführung ist maßgeblich.

## Was der aktuelle Gate abdeckt

- M0–M4 bestehende Regressionstests
- M5 Knowledge/Evidence Domainregeln
- Migration `0005_knowledge_evidence.sql`
- Knowledge/Evidence Persistence Smoke Tests
- CSV-Codec: Quotes, Kommas, BOM, eingebettete Zeilenumbrüche und fehlerhafte Spaltenzahl
- Resource-CSV-Import: Provideranlage, Tags, Canonical-URL-Dublette und zeilenbezogene Fehlerfortsetzung
- Architecture Tests
- WinForms-Kompilierung mit neuem `Daten`-Menü

## Manuelle Prüfung nach Checkout

```powershell
dotnet clean .\SASD.LearningManager.sln
dotnet restore .\SASD.LearningManager.sln
dotnet build .\SASD.LearningManager.sln -c Release --no-restore
dotnet test .\SASD.LearningManager.sln -c Release --no-build
```

Anwendung:

```powershell
dotnet run --project .\src\SASD.LearningManager.WinForms\SASD.LearningManager.WinForms.csproj
```

Danach im Menü:

```text
Daten → Ressourcen aus CSV importieren …
```

und die Datei

```text
testdata\import\resources-chat-recommendations.csv
```

wählen.

## Noch nicht durch automatisierte UI-Tests abgedeckt

- Interaktion mit Windows `OpenFileDialog` / `SaveFileDialog`
- visuelle Darstellung der importierten Daten im Grid
- die noch nicht implementierten Knowledge-/Evidence-Workspaces
- direkte SkillAssessment↔Evidence-Zuordnung

Diese Punkte benötigen entweder manuelle UI-Prüfung oder spätere presentation-orientierte Tests.
