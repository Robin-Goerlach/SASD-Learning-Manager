# Milestone 2 – Quick Capture & Inbox

**Status:** Implemented in source; external .NET build verification pending
**Stand:** 2026-08-27

## Ziel

Milestone 2 reduziert die Reibung beim Sammeln neuer Lernressourcen. Eine URL kann sofort gesichert und später in einer dedizierten Inbox vollständig klassifiziert werden.

## Implementiert

- globaler Quick-Capture-Dialog
- Shortcut `Ctrl+Shift+N`
- minimale Pflichtangabe: HTTP/HTTPS-URL
- optionaler Titel
- optionale Capture-Notiz
- automatischer Status `Inbox`
- konservative URL-Normalisierung
- URL-Dublettenprüfung
- expliziter Dublettenentscheidungsdialog
  - bestehende Ressource öffnen
  - bewusst zweite Ressource anlegen
  - abbrechen
- eigene Inbox-Ansicht
- Inbox-Suche und Paging
- Klassifikationsworkflow über Resource Editor
- Klassifikation setzt standardmäßig `Planned`
- erfolgreich klassifizierter Eintrag darf nicht `Inbox` bleiben
- URL aus Inbox öffnen
- Inbox-Eintrag reversibel archivieren
- dediziertes Inbox Read Model
- Integrationstests für Inbox Query
- Application Tests für Quick Capture und Klassifikation

## Zusätzlich behobene M1-Fehler

Aus dem ersten echten Windows-Build wurden korrigiert:

1. `System.Windows.Forms.Application.Run(...)` wird voll qualifiziert und kollidiert nicht mehr mit dem Namespace `SASD.LearningManager.Application`.
2. xUnit-v3-Testaufrufe übergeben `TestContext.Current.CancellationToken`, um `xUnit1051` zu erfüllen.
3. Die URL-Dublettenprüfung kann die gerade bearbeitete Resource explizit ausschließen.
4. Bewusst erlaubte URL-Dubletten können beim späteren Bearbeiten wieder bewusst bestätigt werden.

## Bewusste Nicht-Funktionen

Noch nicht M2:

- HTTP-Metadatenabruf
- Provider-Autoerkennung
- Browser Extension
- AI-Klassifikation
- Skills/Goals/Paths

Diese Funktionen gehören gemäß Roadmap zu V1.x bzw. späteren Milestones.

## Datenbankschema

M2 benötigt keine neue Migration. `ResourceStatus.Inbox`, `CreatedAtUtc`, `WhySaved` und die vorhandenen Resource-Felder reichen für den M2-Workflow aus.

Die Capture-Notiz wird in V1 als `WhySaved` gespeichert. Falls die Pilotnutzung zeigt, dass freie Capture-Notizen semantisch deutlich von „Warum gespeichert?“ abweichen, wird vor V1.0 ein separates Feld per Migration evaluiert.

## Abnahme

```text
Ctrl+Shift+N
→ URL erfassen
→ Inbox
→ Eintrag auswählen
→ klassifizieren
→ Provider/Typ/Tags/Status ergänzen
→ speichern
→ Eintrag verschwindet aus Inbox
→ Resource bleibt unter derselben ID erhalten
```
