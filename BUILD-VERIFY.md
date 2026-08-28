# Build Verification – SASD Learning Manager M4 Hotfix 001

## Verifizierte Ausgangsbasis

Milestone 3 wurde am 28.08.2026 auf Windows erfolgreich nachgewiesen:

```text
Build succeeded
0 Warning(s)
0 Error(s)
48 / 48 Tests grün
```


## M4 Hotfix 001 – Ursache des ersten fehlgeschlagenen Builds

Der erste M4-Auslieferungsstand enthielt in
`tests/SASD.LearningManager.Application.Tests/TestDoubles.cs` ab Zeile 275
einen Generator-/Serialisierungsfehler: ein kompletter C#-Block war mit wörtlichen
`\n`-Sequenzen in **eine physische Quelltextzeile** geschrieben worden.

Der Windows-Compiler meldete deshalb zahlreiche Syntaxfehler, die sämtlich von
`TestDoubles.cs(275, ...)` ausgingen. Der Produktcode selbst (Domain, Application,
Infrastructure und WinForms) sowie drei der vier Testprojekte wurden davor bereits
erfolgreich kompiliert.

Hotfix 001 ersetzt den beschädigten Block durch normalen C#-Quelltext und enthält
einen zusätzlichen Quelltextscan gegen vergleichbare Generator-Artefakte.

## Milestone 4 – auszuführender Nachweis

Im Repository-Root:

```cmd
dotnet clean .\SASD.LearningManager.sln
dotnet restore .\SASD.LearningManager.sln
dotnet build .\SASD.LearningManager.sln -c Release --no-restore
dotnet test .\SASD.LearningManager.sln -c Release --no-build
```

Erwartet:

```text
Build succeeded.
0 Warning(s)
0 Error(s)

Domain.Tests          23 passed
Application.Tests     26 passed
Infrastructure.Tests  11 passed
Architecture.Tests     4 passed
Total                 64 passed
Failed                 0
```

## Zusätzlich geprüfte M4-Baseline

Vor Auslieferung wurden in der Erstellungsumgebung geprüft:

- Migration 0001 bytegleich zu M3
- Migration 0002 bytegleich zu M3
- Migration 0003 bytegleich zu M3
- Migration 0004 mit SQLite ausgeführt
- Foreign-Key-Check ohne Fehler
- SQLite Integrity Check `ok`
- alle ProjectReferences vorhanden
- Projekt-/Props-XML valide
- bekannte xUnit1051/xUnit2017-Antipatterns nicht gefunden
- keine literalen `\n`-Generator-Artefakte außerhalb gültiger C#-Strings/Kommentare
- keine C#-Quelltextzeile > 300 Zeichen nach Hotfix
- alle 64 `[Fact]`-Tests im Quellbestand gezählt

## Anwendung starten

```cmd
dotnet run --project .\src\SASD.LearningManager.WinForms\SASD.LearningManager.WinForms.csproj
```

Im UI sollte jetzt **Lernpfade** aktiv sein.
