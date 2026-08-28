# Upgrade auf v0.7.0

1. Bestehenden Checkout sichern bzw. `git status` prüfen.
2. Update-ZIP im Repository-Root überlagern.
3. Einmalig `powershell -ExecutionPolicy Bypass -File .\APPLY-v0.7.0.ps1` ausführen. Das Script
   entfernt ausschließlich bekannte M0-/FinanceControl-Overlay-Reste aus langen inkrementellen Checkouts.
4. `powershell -ExecutionPolicy Bypass -File .\scripts\Invoke-ReleaseGate.ps1` ausführen.
5. Anwendung starten und unter **Sicherung / Diagnose** ein passwortgeschütztes `.sasdbak` erstellen.
6. Das Backup mit demselben Passwort validieren.
7. Vor RC1 einen Restore mit Testdaten praktisch durchführen und danach erneut `RC-Check` ausführen.

v0.7.0 ändert das Datenbankschema nicht. Alte unverschlüsselte ZIP-Backups bleiben kompatibel.
