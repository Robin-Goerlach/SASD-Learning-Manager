# Milestone 5 – Jobsuche und Quellenadapter / v0.4.0

## Ziel

v0.4.0 ergänzt den SASD Bewerbungsmanager um eine eigene **Job-Fund-Inbox** zwischen Suchquelle und
`Opportunity`. Ein Treffer auf LinkedIn, BA Jobsuche, StepStone, Indeed, meinestadt oder einer
Unternehmensseite ist damit noch keine dauerhaft interessante Stelle.

Der Workflow lautet bewusst:

```text
Suchprofil / externer Adapter / Clipboard
                  │
                  ▼
          normalisierter JobLead
                  │
                  ├─ Duplikat → überspringen
                  ├─ prüfen → JobLead bleibt erhalten
                  ├─ ignorieren → aus aktiver Inbox ausblenden
                  └─ übernehmen → Opportunity + SourceLink
```

Damit bleibt die Opportunity-Liste fachlich sauber und die Suchroutine trotzdem nachvollziehbar.

## Kein aggressives Scraping

Der Bewerbungsmanager selbst loggt sich nicht in Jobportale ein, steuert keinen Browser und umgeht
keine Portalmechanismen. v0.4.0 definiert stattdessen kleine **Quellenadapter an der Systemgrenze**:

- JSON-Handoff v1,
- CSV-Handoff v1,
- manuelle Windows-Zwischenablage.

Spätere externe Adapter, Browser-Helfer oder die Mail Workbench können dasselbe normalisierte Modell
liefern, ohne dass Domain/Application portalabhängig werden.

## JobLead

Ein `JobLead` speichert unter anderem:

- Suchprofil (optional),
- Quellsystem,
- externe Stellen-ID,
- SHA-256-Fingerprint,
- Position,
- Organisationsname als Quelltext,
- Standort,
- Remote-/Hybrid-Text,
- Gehaltsinformation,
- Quell-URL,
- Stellenbeschreibung,
- Veröffentlichungs- und Fundzeitpunkt,
- Status `New`, `Reviewed`, `Imported` oder `Ignored`,
- verknüpfte Opportunity nach Übernahme.

Der Organisationsname bleibt zunächst Text. Eine echte `Organization` wird nicht automatisch aufgrund
eines Portalstrings erzeugt; beim Übernehmen bestätigt der Benutzer vorhandene Organisationen.

## Deduplizierung

Der Import ist idempotent. Es wird in dieser Reihenfolge geprüft:

1. `(SourceSystem, ExternalJobId)`, sofern vorhanden,
2. kanonisierte HTTP-/HTTPS-URL,
3. SHA-256-Fingerprint aus Quelle, externer ID, Titel, Organisation und URL.

Die URL-Kanonisierung entfernt Fragmente und typische Trackingparameter (`utm_*`, `ref`,
`trackingId`). Dadurch wird derselbe Treffer nicht nur wegen eines Newsletter-Trackinglinks doppelt
angelegt.

## JSON-Handoff v1

Beispiel: `docs/examples/job-source-handoff-v1.json`.

```json
{
  "schemaVersion": 1,
  "sourceSystem": "Example Portal",
  "searchProfileId": null,
  "capturedAtUtc": "2026-08-27T08:00:00Z",
  "items": []
}
```

Jedes Item kann neben Titel und URL optionale Quellmetadaten enthalten. `title` ist Pflicht.

## CSV-Handoff v1

Beispiel: `docs/examples/job-source-handoff-v1.csv`.

Semikolon-getrennte UTF-8-Dateien werden unterstützt. Der Parser verarbeitet Anführungszeichen,
verdoppelte Quotes und Zeilenumbrüche innerhalb gequoteter Beschreibungen.

Spalten:

```text
sourceSystem
searchProfileId
capturedAtUtc
externalJobId
title
organizationName
location
remoteText
salaryText
url
descriptionText
publishedAtUtc
```

`sourceSystem` und `title` sind erforderlich. Eine Datei enthält bewusst genau ein Quellsystem.

## Zwischenablage

Wenn ein Portal keinen Adapter liefert, kann ein Treffer manuell aus der Windows-Zwischenablage
erfasst werden. Der Dialog übernimmt den Clipboard-Text als Beschreibung, schlägt die erste Textzeile
als Titel vor und erkennt die erste HTTP/HTTPS-URL lokal.

## Übernahme als Opportunity

Erst der Benutzer entscheidet, dass ein `JobLead` eine echte Opportunity wird. Beim Übernehmen:

1. können Arbeitgeber und Vermittler aus vorhandenen Organisationen gewählt werden,
2. wird der Beschreibungstext als dauerhafter Rollenbeschreibung-Snapshot übernommen,
3. wird die Quell-URL als `SourceLink` gespeichert,
4. wird der JobLead mit der Opportunity verknüpft und auf `Imported` gesetzt.

Fehlt eine ausführliche Beschreibung, erzeugt die Anwendung einen transparenten Minimal-Snapshot aus
den vorhandenen Quellinformationen; sie erfindet keine Stelleninhalte.

## Datenbank

Neue Migration:

```text
202608270004_JobSearchAdapters
```

Neue Tabelle:

```text
job_leads
```

Bestehende Daten bleiben erhalten. `SearchProfileId` und `OpportunityId` verwenden `SetNull`, damit
die Fundhistorie nicht verschwindet, wenn eine Relation später entfernt wird.

## Tests

Der Milestone ergänzt Tests für:

- JobLead-Lifecycle,
- URL-/External-ID-Deduplizierung,
- Suchprofil-Check nach Batchimport,
- Übernahme in Opportunity + SourceLink,
- JSON-Adapter,
- CSV-Adapter inklusive mehrzeiligem quoted Feld,
- SQLite-Migration und Roundtrip,
- echten Systemworkflow,
- WinForms-DI/Composition-Root,
- deutsche Statusanzeige.

## Bewusste Grenzen

Nicht Bestandteil von v0.4.0:

- Portal-Login,
- automatisches Crawling/Scraping,
- Browserextension,
- CAPTCHA-/Anti-Bot-Umgehung,
- automatische Bewerbung,
- fuzzy/KI-basierte Dublettenerkennung,
- automatische Anlage von Unternehmen aus Portaltext,
- Cloud-Synchronisation.
