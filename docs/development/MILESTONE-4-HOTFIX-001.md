# Milestone 4 Hotfix 001 – TestDoubles Source Repair

**Datum:** 2026-08-28  
**Status:** bereit zur Windows-Verifikation

## Symptom

Der erste Milestone-4-Stand scheiterte im Projekt
`SASD.LearningManager.Application.Tests`. Der Compiler meldete sehr viele Fehler wie
`CS1056`, `CS0116`, `CS1519` und `CS1002`. Sämtliche Fehlerpositionen lagen in
`TestDoubles.cs` auf der physischen Zeile 275.

## Root Cause

Beim Erzeugen der M4-Datei wurde der neue `FakeLearningPathRepository`-Block nicht
mit echten Zeilenumbrüchen angehängt. Stattdessen enthielt die Datei wörtliche
Zeichenfolgen `\n`. Dadurch bestand der gesamte hinzugefügte Block aus einer einzigen
6.587 Zeichen langen C#-Zeile mit Backslashes im Quelltext.

Es handelte sich **nicht** um einen Fehler des Benutzers, von Git oder von .NET.

## Korrektur

- 112 literale `\n`-Sequenzen des beschädigten Blocks wurden in echte Zeilenumbrüche umgewandelt.
- `FakeLearningPathRepository` liegt wieder als normaler, lesbarer C#-Typ vor.
- alle M4-Quelltexte wurden auf vergleichbare Escape-Artefakte geprüft.
- die maximale C#-Zeilenlänge wurde auf pathologische Generatorzeilen geprüft.
- xUnit1051-/xUnit2017-Risikomuster wurden erneut geprüft.
- Migrationen 0001–0003 bleiben bytegleich zum bestätigten M3-Stand.
- `*.sql text eol=lf` wurde in `.gitattributes` ergänzt, um Migration-Checksums auf Windows stabil zu halten.
- `SASD-Learning-Manager-Milestone-*.zip` wurde `.gitignore` hinzugefügt.

## Aus dem fehlgeschlagenen Build ableitbarer positiver Befund

Vor dem Abbruch wurden bereits erfolgreich kompiliert:

- `SASD.LearningManager.Domain`
- `SASD.LearningManager.Application`
- `SASD.LearningManager.Infrastructure`
- `SASD.LearningManager.WinForms`
- `SASD.LearningManager.Domain.Tests`
- `SASD.LearningManager.Infrastructure.Tests`
- `SASD.LearningManager.Architecture.Tests`

Damit war der gemeldete Fehler auf das Application-Testprojekt und dort auf die
beschädigte Testdouble-Datei eingegrenzt.

## Windows-Verifikation

```cmd
dotnet clean .\SASD.LearningManager.sln
dotnet restore .\SASD.LearningManager.sln
dotnet build .\SASD.LearningManager.sln -c Release --no-restore
dotnet test .\SASD.LearningManager.sln -c Release --no-build
```

Erwartung: 0 Warnungen, 0 Fehler, 64 Tests grün.
