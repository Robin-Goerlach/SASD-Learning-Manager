# Build & Verify – Milestone 3

## Bestätigte Baseline

Milestone 2 Hotfix 001 wurde am 27.08.2026 auf Windows erfolgreich verifiziert:

```text
Build succeeded
0 Warning(s)
0 Error(s)
29 / 29 Tests grün
```

Milestone 3 baut direkt auf diesem bestätigten Stand auf.

## Vollständiger M3-Nachweis

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
48 Tests grün
```

## Anwendung

```powershell
dotnet run --project .\src\SASD.LearningManager.WinForms\SASD.LearningManager.WinForms.csproj
```

## M3 Smoke Test

1. Skills → Kompetenzkatalog → Bereich und Topic anlegen.
2. Skill mit Target Level 4 anlegen.
3. Skill auf Current Level 2 bewerten.
4. Gap `+2` prüfen.
5. Assessment-Historie erneut öffnen.
6. Goal anlegen und Skill zuordnen.
7. Skill danach erneut prüfen: Current Level unverändert.
8. Archivieren/Wiederherstellen von Goal/Skill prüfen.
9. App neu starten und Persistenz prüfen.

## Datenbankmigration

Beim ersten Start des M3-Stands wird ausschließlich `0003_goals_skills.sql` zusätzlich angewandt.

Die bereits bestätigten Migrationen `0001` und `0002` sind bytegleich zum M2-Hotfix-Stand.

## Verifikation in der Erstellungsumgebung

- alle drei SQL-Migrationen mit SQLite ausgeführt: PASS
- `PRAGMA foreign_key_check`: PASS
- `PRAGMA integrity_check`: PASS
- M3-Taxonomie-/Skill-/Goal-Beziehungen als SQL-Smoke-Test: PASS
- M1/M2 Migration-Checksums unverändert: PASS
- alle `.csproj`/`.props`: XML geprüft
- alle ProjectReferences: geprüft
- C#-Delimiter-/Lexik-Check: PASS
- bekannte xUnit2017-/Application.Run-Regressionsmuster: geprüft

Ein echter .NET-Compiler ist in der Erstellungsumgebung weiterhin nicht installiert; deshalb ist der obige Windows-Build der finale Nachweis.
