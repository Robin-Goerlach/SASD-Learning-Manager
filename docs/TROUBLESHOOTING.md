# Troubleshooting

## Windows Defender blockiert eine lokal gebaute Test-DLL

Bei einem v0.2.0-Build wurde einmal gemeldet, dass
`SASD.Bewerbungsmanager.Infrastructure.Tests.dll` ein Virus oder potenziell unerwünschte Software
enthalte. Der anschließende Testlauf konnte deshalb für dieses Assembly keine Tests entdecken.

Das ist **kein vollständig grüner Testlauf**, auch wenn `dotnet test` für die übrigen Projekte keine
fehlgeschlagenen Tests meldet.

Wenn die Meldung erneut erscheint:

1. Windows-Sicherheit → Viren- & Bedrohungsschutz → Schutzverlauf öffnen.
2. Den **genauen Erkennungsnamen** und die betroffene Datei notieren.
3. `bin`/`obj` des betroffenen Testprojekts löschen und dieses Projekt separat neu bauen/testen.
4. Keine pauschale Defender-Ausnahme für das komplette Repository anlegen, solange die konkrete
   Erkennung nicht geprüft wurde.
5. Erkennungsname und Buildausgabe im Entwicklungs-Chat bereitstellen.

Beispiel:

```powershell
Remove-Item -Recurse -Force .\tests\SASD.Bewerbungsmanager.Infrastructure.Tests\bin -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force .\tests\SASD.Bewerbungsmanager.Infrastructure.Tests\obj -ErrorAction SilentlyContinue

dotnet build .\tests\SASD.Bewerbungsmanager.Infrastructure.Tests\SASD.Bewerbungsmanager.Infrastructure.Tests.csproj -c Release
dotnet test  .\tests\SASD.Bewerbungsmanager.Infrastructure.Tests\SASD.Bewerbungsmanager.Infrastructure.Tests.csproj -c Release --no-build
```

Der Test muss tatsächlich von xUnit entdeckt werden; eine Meldung `No test is available` ist nicht als
erfolgreiche Testabdeckung zu werten.
