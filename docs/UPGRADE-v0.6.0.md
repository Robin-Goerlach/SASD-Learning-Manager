# Upgrade auf v0.6.0

1. Bestehenden Checkout sichern bzw. `git status` prüfen.
2. Update-ZIP im Repository-Root überlagern.
3. Einmalig `powershell -ExecutionPolicy Bypass -File .\APPLY-v0.6.0.ps1` ausführen. Das entfernt nur
   bekannte M0-Overlay-Reste aus älteren inkrementellen Lieferungen.
4. `dotnet clean`, `restore`, `build`, `test` ausführen.
5. Anwendung starten und unter **Sicherung / Diagnose** ein Backup erstellen und validieren.
6. Vor RC1 mindestens einen Restore mit Testdaten praktisch durchführen.

v0.6.0 enthält keine neue Datenbankmigration. Bestehende Daten werden nicht absichtlich verändert.
