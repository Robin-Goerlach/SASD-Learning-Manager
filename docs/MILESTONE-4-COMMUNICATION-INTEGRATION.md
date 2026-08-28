# Milestone 4 – Kommunikationsintegration / v0.3.0

## Ziel

v0.3.0 verbindet den SASD Bewerbungsmanager mit elektronischer Kommunikation, ohne den
Bewerbungsmanager selbst in einen zweiten Mailclient zu verwandeln.

Die Architektur folgt deshalb einer klaren Verantwortungsgrenze:

```text
SASD Mail Workbench / Clipboard / lokaler Adapter
                    │
                    │ normalisierte Kommunikation
                    ▼
          SASD Bewerbungsmanager
                    │
                    ├─ deduplizieren
                    ├─ Kontakt konservativ erkennen
                    ├─ Bewerbung/Stelle konservativ zuordnen
                    ├─ Recruiter-Mail → Activity
                    ├─ Job-Alert lokal analysieren
                    ├─ ACTION erzeugen
                    └─ Nachrichtentext → Stellen-Snapshot
```

IMAP, POP3, SMTP, MIME, Anhänge, Mailbox-Synchronisation und Zugangsdaten bleiben außerhalb des
Bewerbungsmanagers. Dafür ist langfristig die SASD Mail Workbench zuständig.

## Kommunikations-Inbox

Die neue Seite **„Kommunikation“** zeigt normalisierte Nachrichten mit:

- Quellsystem,
- Eingang/Ausgang,
- Klassifikation,
- Absender,
- Betreff,
- Nachrichtenzeitpunkt,
- Verarbeitungsstatus,
- Zuordnungsstatus,
- erzeugter Timeline-Aktivität,
- Nachrichtentext und lokal erkannten HTTP/HTTPS-Links.

Die Daten bleiben lokal in SQLite.

## Mail-Workbench-Handoff

v0.3.0 definiert ein kleines versioniertes JSON-Handoff-Format (`schemaVersion = 1`). Es transportiert
bewusst nur normalisierte Daten und keine Roh-MIME-Datei:

- externe Nachrichten-ID, sofern vorhanden,
- Richtung,
- optionale Klassifikation,
- Absendername/-adresse,
- Empfängertext,
- Betreff,
- Plain-Text-Inhalt,
- Nachrichtenzeitpunkt,
- optionale Quellreferenz,
- optional bereits bekannte SASD-IDs für Stelle/Bewerbung/Kontakt/Organisation.

Ein Beispiel liegt unter `docs/examples/mail-workbench-handoff-v1.json`.

### Warum kein direkter IMAP-Import?

Der Bewerbungsmanager soll ein Job-Search-CRM bleiben. Mailprotokolle, MIME, Offline-Synchronisation,
Authentifizierung und Mail-Sicherheitsanalyse sind eine eigene Problemklasse und gehören in die SASD
Mail Workbench. Die Handoff-Schnittstelle hält beide Produkte klein und testbar.

## Deduplizierung

Jede Nachricht erhält einen SHA-256-Fingerprint aus normalisierten Kerndaten. Liefert das Quellsystem
eine stabile externe Nachrichten-ID, wird zusätzlich `(SourceSystem, ExternalMessageId)` geprüft.

Erneuter Import derselben Nachricht erzeugt daher weder einen zweiten Communication-Datensatz noch
eine zweite Timeline-Aktivität.

## Konservative automatische Zuordnung

Automatische Zuordnung darf keine falsche Bewerbung erfinden. Deshalb gelten bewusst strenge Regeln:

1. Explizite IDs aus einem Handoff werden validiert und verwendet.
2. Eine bekannte Absenderadresse wird nur dann automatisch einem Kontakt zugeordnet, wenn genau ein
   nicht archivierter Kontakt diese Adresse besitzt.
3. Eine Bewerbung/Stelle wird über frühere Aktivitäten dieses Kontakts nur dann übernommen, wenn die
   Zuordnung eindeutig ist.
4. Alternativ wird bei bekannter Organisation nur dann eine aktive Stelle übernommen, wenn genau eine
   mögliche Stelle existiert.
5. Mehrdeutige Fälle bleiben unzugeordnet und können in der UI manuell bestätigt werden.

Damit bleibt der Grundsatz **Goal ≠ Authorization** auch fachlich sinnvoll: Eine vermeintlich passende
Mail darf nicht stillschweigend den falschen Bewerbungsprozess verändern.

## Recruiter- und Bewerbungs-Mails

Direkte Recruiter-/HR-Kommunikation und Antworten im Bewerbungsprozess werden beim Import automatisch
als `ActivityKind.Email` in die Timeline übernommen, sofern die Nachricht nicht bereits verarbeitet wurde.

Die Activity enthält:

- Zeitpunkt der Nachricht,
- Betreff,
- Quellsystem,
- Absender,
- einen auf die Activity-Grenze gekürzten Nachrichtentext,
- erkannte bzw. bestätigte Relationen.

## Job-Alert-Analyse

Die Analyse ist absichtlich deterministisch und lokal. Es wird keine KI und kein externer Dienst
aufgerufen.

v0.3.0 erkennt:

- typische Job-Alert-Begriffe in Deutsch/Englisch,
- HTTP-/HTTPS-Links,
- einen Titelvorschlag aus Betreff bzw. erster Textzeile.

Aus einer importierten Nachricht kann anschließend eine neue Stelle angelegt werden. Der komplette
normalisierte Nachrichtentext wird als Rollenbeschreibung-Snapshot gespeichert. Arbeitgeber,
Standort und weitere strukturierte Angaben können danach bewusst in der Stellenansicht ergänzt werden.

## Clipboard-Workflow

Neben Mail-Workbench-JSON kann Text direkt aus der Windows-Zwischenablage importiert werden. Das
unterstützt beispielsweise:

- Recruiter-Nachrichten aus Web-Portalen,
- LinkedIn-/Portaltexte, die als Kommunikationskontext erhalten bleiben sollen,
- Stellenbeschreibungen aus Mails oder Weboberflächen,
- kontrollierte manuelle Übergaben, solange die Mail Workbench noch nicht direkt angebunden ist.

## ACTION aus Kommunikation

Eine selektierte Kommunikation kann direkt eine `ACTION` erzeugen. Die Aufgabe übernimmt die bereits
zugeordnete Stelle/Bewerbung/Kontakt/Organisation und verweist in den Notizen auf den Nachrichtenbetreff.

## Datenmodell

Neue Entität:

- `CommunicationMessage`

Neue Enums:

- `CommunicationDirection`
- `CommunicationKind`
- `CommunicationStatus`

Neue Migration:

```text
202608270003_CommunicationIntegration
```

Neue Tabelle:

```text
communication_messages
```

Die Tabelle referenziert optional bestehende Opportunities, Applications, Contacts, Organizations und
Activities. Löschregeln sind `SetNull`, damit ein importierter Kommunikationsnachweis nicht durch das
Entfernen einer Relation verschwindet.

## Architektur

Neue Application-Komponenten:

- `CommunicationImportService`
- `ICommunicationHandoffReader`
- versionierte Handoff-/Import-ReadModels

Neue Infrastructure-Komponente:

- `JsonCommunicationHandoffReader`
- SQLite-Persistenz für `CommunicationMessage`

Neue WinForms-Komponenten:

- `CommunicationsControl`
- Clipboard-Importdialog
- Zuordnungsdialog
- ACTION-Dialog
- Stelle-aus-Kommunikation-Dialog

Es wurde keine neue externe Bibliothek eingeführt.

## Tests

Der Milestone ergänzt Tests für:

- Lifecycle des Communication-Domainobjekts,
- Kontakt- und eindeutiges Kontext-Matching,
- automatische Activity-Erzeugung,
- idempotenten Re-Import,
- lokale Job-Alert-/URL-Analyse,
- Stellenanlage aus Kommunikation,
- JSON-Handoff mit String-Enums,
- SQLite-Roundtrip über die aktuelle Migration,
- Systemworkflow mit echter SQLite-Datenbank,
- WinForms-DI-/Composition-Root.

## Bewusste Grenzen

Nicht Bestandteil von v0.3.0 sind:

- direkter IMAP-/POP3-Zugriff,
- SMTP-Versand,
- OAuth-/Passwortverwaltung,
- Roh-MIME-Speicherung,
- Anhänge,
- HTML-Mail-Rendering,
- automatische KI-Zusammenfassungen,
- unsichere fuzzy Zuordnung zu Bewerbungen,
- Hintergrund-Dateiwatcher oder Cloud-Synchronisation.

Diese Punkte gehören entweder in die SASD Mail Workbench oder in spätere strategische Ausbaustufen.
