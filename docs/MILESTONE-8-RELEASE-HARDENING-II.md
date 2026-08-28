# Milestone 8 – v0.7.0 Releasehärtung II / RC-Vorbereitung

## Ziel

v0.7.0 schließt zwei zentrale Release-Risiken aus der ersten Härtungsstufe: Backups können jetzt
optional – in der Oberfläche standardmäßig – passwortgeschützt gespeichert werden, und sowohl die
laufende Installation als auch das Repository erhalten explizite Release-Gates.

Der Milestone verändert keine Bewerbungsfachlichkeit und keine Datenbanktabellen.

## Passwortgeschützte Backups

Die bestehende innere ZIP-Struktur aus v0.6.0 bleibt unverändert. Für neue geschützte Backups wird sie
in einen eigenen SASD-Container (`*.sasdbak`) eingebettet.

Technischer Aufbau:

- PBKDF2-HMAC-SHA-256 mit zufälligem 128-Bit-Salt und 600.000 Iterationen;
- getrennte 256-Bit-Schlüssel für AES und HMAC;
- AES-256-CBC mit zufälligem 128-Bit-IV für streamingfähige Verschlüsselung;
- HMAC-SHA-256 über Header und vollständigen Ciphertext;
- Authentizitätsprüfung **vor** dem Entschlüsseln in eine nutzbare temporäre ZIP-Datei;
- konstante Tag-Prüfung über `CryptographicOperations.FixedTimeEquals`;
- Schlüsselmaterial wird nach der Operation bestmöglich aus den Byte-Arrays gelöscht.

Falsches Passwort und manipulierte Datei erzeugen absichtlich dieselbe Fehlermeldung. Das Passwort
wird vom Bewerbungsmanager nicht gespeichert.

Unverschlüsselte v0.6-ZIP-Backups bleiben les- und wiederherstellbar. Beim Erstellen muss ihre
unverschlüsselte Speicherung jedoch ausdrücklich bestätigt werden.

## Lokaler RC-Check

Unter **Sicherung / Diagnose** prüft `RC-Check` ausschließlich technisch beweisbare lokale Gates:

- SQLite `quick_check`;
- Foreign-Key-Integrität;
- keine ausstehenden EF-Migrationen;
- keine aktuell vorgemerkte Wiederherstellung;
- vorhandene, nicht leere SQLite-Datei;
- Schreibbarkeit des lokalen Datenverzeichnisses;
- Vorhandensein einer pre-restore Recovery-Kopie als Nachweisindikator.

CI, Code Signing, Lizenzentscheidung und Installer-Veröffentlichung bleiben bewusst externe Gates.

## Test-Discovery-Gate

`scripts/Verify-Tests.ps1` führt jedes Testprojekt separat aus, schreibt TRX und lehnt einen Lauf ab,
wenn ein Projekt zwar mit Exitcode 0 endet, aber **keinen einzigen Test entdeckt**. Damit wird das
bereits beobachtete `Infrastructure.Tests`/Defender-Problem nicht mehr durch einen scheinbar grünen
Solution-Testlauf verdeckt.

GitHub Actions verwendet ab v0.7.0 dieses Gate.

## Release-Paket

`scripts/Publish-Release.ps1` erzeugt einen reproduzierbaren Windows-x64-Publish-Ordner, ein ZIP-Paket
und eine SHA-256-Datei. Standardmäßig wird self-contained veröffentlicht. Das Skript signiert noch
nicht und erzeugt noch keinen Installer; diese beiden Punkte bleiben explizite Release-Gates.

`scripts/Invoke-ReleaseGate.ps1` bündelt Clean, Restore, Build und den Test-Discovery-Check für lokale
Abnahmen.

## Keine Schemaänderung

v0.7.0 benötigt keine neue EF-Core-Migration. Die produktive Migrationskette endet weiterhin bei
`202608270005_AssistantWorkspace`.
