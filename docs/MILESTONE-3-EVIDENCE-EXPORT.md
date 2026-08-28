# Milestone 3 – Nachweise, Export und Austausch / v0.2.0

## Ziel

v0.2.0 macht die im Operational MVP gepflegten Daten außerhalb der Anwendung nutzbar, ohne dafür
Cloud-Synchronisation oder externe Dienste einzuführen. Der Milestone konzentriert sich auf zwei
konkrete Alltagsprobleme:

1. **Bewerbungsaktivitäten für einen Zeitraum nachvollziehbar nachweisen.**
2. **Den Kontext einer einzelnen Bewerbung kontrolliert an andere Werkzeuge oder Personen übergeben.**

Die Anwendung bleibt local-first. Exporte entstehen ausschließlich auf ausdrückliche Benutzeraktion
in einem vom Benutzer gewählten Zielpfad.

## Bewerbungsnachweis

Die neue Seite **„Nachweise / Export“** kann einen inklusiven Zeitraum auswählen und zeigt eine
Vorschau der tatsächlich versendeten Bewerbungen.

Eine Bewerbung wird nur in den Nachweis aufgenommen, wenn `SubmittedAtUtc` gesetzt ist. Ein reiner
Entwurf oder eine nur begonnene Bewerbung wird nicht als versendete Bewerbung ausgegeben.

Damit dieser Nachweis auch nachträglich korrekt gepflegt werden kann, besitzt die Bewerbungsansicht
jetzt **„Versanddaten“**. Dort können Versanddatum und Bewerbungskanal ausdrücklich korrigiert oder
ein fälschlich gesetztes Versanddatum wieder entfernt werden. Ein Statuswechsel allein erfindet bewusst
kein Versanddatum.

Der Nachweis enthält:

- Versanddatum
- Unternehmen
- Position
- Standort, soweit vorhanden
- Bewerbungskanal
- aktuellen Bewerbungsstatus
- bekannte Quellen der Stelle

Der Zeitraum wird als lokaler Kalendertag interpretiert. Die Datenbank behält weiterhin UTC-
Zeitstempel; nur die Auswahl und Darstellung für den Nachweis erfolgt in lokaler Zeit.

### CSV

CSV wird als UTF-8 mit BOM und Semikolon als Trennzeichen geschrieben. Das ist bewusst auf eine
problemlose Weiterverarbeitung in deutsch konfigurierten Tabellenkalkulationen ausgerichtet.
Zeichen mit Semikolon oder Anführungszeichen werden korrekt gequotet.

### PDF

Zusätzlich kann derselbe Nachweis als kompakte A4-PDF-Datei erzeugt werden. Für diesen begrenzten
Anwendungsfall wurde kein großes PDF-Framework eingeführt. Ein kleiner interner PDF-Writer erzeugt
einen linearen, mehrseitigen Nachweis mit Seitennummerierung und WinAnsi-Text.

CSV und PDF können einzeln oder gemeinsam in einen ausgewählten Ordner exportiert werden.

## Austauschdossier

Für eine konkrete Bewerbung kann ein strukturiertes Dossier erzeugt werden. Es enthält:

- Position, Arbeitgeber und Vermittler
- Bewerbungsstatus und Kanal
- Start- und Versandzeitpunkt
- Standort-/Remote-/Gehaltsinformationen
- gespeicherten Rollenbeschreibungs-Snapshot
- Quellenlinks
- relevante Kontakte
- Timeline/Activities
- ACTION- und WAITING_FOR-Einträge
- Metadaten der tatsächlich verwendeten Dokumentversionen

### Datenschutzgrenze

Das Austauschdossier enthält bewusst **keine lokalen absoluten Dateipfade und keine Dokumentdateien**.
Bei Dokumenten werden nur fachlich relevante Metadaten einschließlich SHA-256 exportiert. Dadurch
kann ein Dossier weitergegeben werden, ohne versehentlich den Aufbau des lokalen Dateisystems oder
private Dokumentinhalte offenzulegen.

### Formate

- **JSON**: versioniertes Schema (`schemaVersion = 1`) für maschinelle Weiterverarbeitung.
- **Markdown**: menschenlesbare Darstellung für Dokumentation, Beratung oder bewusste Übergabe an
  einen separaten Chat.

Der Export ist in v0.2.0 absichtlich **einseitig**. Es gibt noch keinen automatischen Import und
keine Synchronisation. Damit vermeiden wir in dieser frühen Phase Konfliktauflösung, Daten-Duplikate
und unklare Merge-Regeln.

## Architektur

Neue Application-Komponenten:

- `ApplicationEvidenceService`
- `ApplicationDossierService`
- `ApplicationEvidenceReport` / `ApplicationEvidenceItem`
- `ApplicationExchangeDossier` und dessen Teilmodelle
- `IApplicationExportWriter`

Neue Infrastructure-Komponente:

- `FileSystemApplicationExportWriter`

Neue WinForms-Komponente:

- `EvidenceExportControl`

Die bestehende Abhängigkeitsrichtung bleibt erhalten:

```text
WinForms -> Application -> Domain
               ^
               |
        Infrastructure
```

Dateiformate werden nicht in WinForms erzeugt. Die Oberfläche orchestriert nur den Use Case und die
Dateiauswahl.

## Persistenz

v0.2.0 benötigt **keine neue Datenbankmigration**. Alle Exporte werden aus den bereits in v0.1.0
vorhandenen Daten aufgebaut. Die vorhandene Datenbank kann unverändert weiterverwendet werden.

## Tests

Der Milestone ergänzt Tests für:

- inklusive Zeitraumsauswahl und Ausschluss nicht versendeter Entwürfe
- ungültige Datumsbereiche
- Aufbau eines strukturierten Austauschdossiers
- Datenschutzgrenze des Dokumentmodells
- UTF-8-BOM, CSV-Header und CSV-Quoting
- minimale PDF-Struktur
- JSON- und Markdown-Export ohne lokale Dateipfade
- Korrektur und Validierung von Versanddatum/Bewerbungskanal
- deutsche UI-Bezeichnung des Bewerbungskanals
- DI-/Composition-Root-Validierung des neuen Export-Controls

## Bewusste Grenzen

Nicht Bestandteil von v0.2.0 sind:

- Import fremder Austauschdossiers
- Datenbanksynchronisation
- Cloud-Speicher
- E-Mail-Import
- automatischer Portalabruf
- digitale Signatur von Nachweisen
- komplexe frei konfigurierbare Reportdesigner

Diese Grenzen halten den Milestone klein, nachvollziehbar und unmittelbar nutzbar.
