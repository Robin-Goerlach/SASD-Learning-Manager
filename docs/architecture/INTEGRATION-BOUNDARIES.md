# Integration Boundaries

## V1

Providerinhalte bleiben außerhalb der Source of Truth. Die App speichert Metadaten, Beziehungen, Progress, Knowledge und Evidence.

## Standardbrowser

Externe Inhalte werden im Standardbrowser geöffnet. Kein eingebetteter Providerbrowser V1.

## Metadata Adapter

Optionaler `IUrlMetadataService` darf title/OpenGraph/canonical/site name abrufen, aber nicht Login umgehen, JS ausführen oder Save blockieren.

## Lokale Dateien

V1 referenziert über `LocalPath`. Missing File löscht das fachliche Objekt nicht.

## Provider APIs V2

Spätere `IProviderIntegration` Adapter; jede Integration separat opt-in und rechtlich/technisch geprüft.

## AI V2

AI ist Suggestion Adapter: `Suggested → Accepted/Rejected`.

## Cloud Sync

Keine kleine Erweiterung, sondern neues Konsistenz-, Authentisierungs- und Konfliktmodell.
