# Milestone 7 – v0.6.0 Datenhoheit & Releasehärtung I

## Ziel

Nach Abschluss des funktionalen 0.x-Pfads wird die lokale Datenhaltung erstmals wie ein Releasegut
behandelt. v0.6.0 ergänzt deshalb keine neue Bewerbungsfachlichkeit, sondern schützt die bereits
vorhandenen Daten und schafft reproduzierbare Diagnose- und Restore-Grenzen.

## Komplettbackup

Ein Backup ist eine ZIP-Datei mit:

- `database/application-tracker.db` – über die SQLite Online-Backup-API erzeugte konsistente Kopie;
- `documents/...` – private immutable Dokument-Snapshots;
- `manifest.json` – Schemaversion, Erstellungszeit, angewandte EF-Migrationen sowie Größe und SHA-256
  jeder autoritativen Datei.

Die Backup-Datei ist in v0.6.0 **nicht verschlüsselt**. Sie muss deshalb wie die produktive lokale
Bewerbungsdatenbank geschützt werden.

## Validierung

Vor einem Restore werden ZIP- und Manifestpfade auf Traversal geprüft, doppelte Pfade abgelehnt,
Dateigrößen und SHA-256 kontrolliert und unbekannte neuere EF-Migrationen zurückgewiesen. Ein Backup
wird niemals allein aufgrund der Dateiendung als vertrauenswürdig betrachtet.

## Restore-Grenze

Ein Restore ersetzt keine laufende Datenbank. Die UI validiert und extrahiert das Backup in ein
privates Staging-Verzeichnis und schreibt nur `pending-restore.json`. Beim nächsten Prozessstart –
noch vor `DatabaseInitializer` – wird eine Recovery-Kopie des aktuellen Zustands erzeugt und erst
danach Datenbank und Dokumente ersetzt. Scheitert der Austausch, versucht der Coordinator den alten
Zustand aus der Recovery-Kopie zurückzusetzen.

Nach dem Start werden `StoredPath`-Werte von Dokument-Snapshots anhand von Application-ID und SHA-256
auf das aktuelle lokale Datenverzeichnis neu gebunden. Damit funktioniert ein Restore auch in einem
anderen Windows-Profil.

## Diagnose

Der Diagnosebericht enthält nur technische Zustände:

- SQLite `quick_check`;
- Anzahl Foreign-Key-Verletzungen;
- angewandte und offene Migrationen;
- Datensatzanzahlen je fachlicher Tabelle.

Nicht enthalten sind fachliche Freitexte, Mailtexte, Stellenbeschreibungen, Dokumentinhalte, Secrets
oder absolute Benutzerprofilpfade.

## Single Instance

Ein Named Mutex verhindert eine zweite interaktive Instanz innerhalb derselben Windows-Sitzung. Das
reduziert neben Bedienfehlern insbesondere das Risiko, dass ein zweiter Prozess während eines
restart-bound Restores noch SQLite-Dateien geöffnet hält.

## Keine Schemaänderung

v0.6.0 fügt keine EF-Migration hinzu. Die aktuelle produktive Kette endet bei
`202608270005_AssistantWorkspace`.
