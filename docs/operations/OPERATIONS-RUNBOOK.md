# Operations Runbook

## Datenpfad
`%LOCALAPPDATA%\SASD\LearningManager\`

## App startet nicht

Error ID notieren, Logs prüfen, freien Speicher/DB-Pfad prüfen, keine manuelle DB-Reparatur, Backup prüfen.

## Integrity

`PRAGMA integrity_check;` über Wartungsfunktion. Bei Fehler sichern und Restore evaluieren.

## Missing File

LocalPath korrigieren; Resource/Evidence bleibt erhalten.

## Support

Teilen: Error ID, Version, Schema, relevanter technischer Logauszug. Nicht ungefragt: DB, Backup, Knowledge.
