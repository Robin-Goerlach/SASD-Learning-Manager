# Projektklassifikation – SASD Learning Manager

**Stand:** 2026-08-27

| Dimension | Einstufung | Begründung |
|---|---|---|
| Projektart | Greenfield Product | neues Produkt |
| Produkttyp | Windows Desktop | WinForms |
| Nutzerzahl V1 | Single User | keine Serverauthentisierung |
| Datenhaltung | lokal persistent | SQLite |
| Fachliche Komplexität | mittel bis hoch | viele Relationen |
| technische Verteilung | niedrig | ein Prozess, eine DB |
| Datenschutzrelevanz | mittel | Lern-/Kompetenzdaten |
| Datenverlustauswirkung | hoch | langfristige persönliche Historie |
| Integrationsrisiko V1 | niedrig | Links statt Provider APIs |
| Langzeitwartung | hoch relevant | persönliches Langzeitwerkzeug |

## Qualitätsstufe

**Recommended während Entwicklung; Production vor stabiler V1.**

## Aktive Profile

- Core
- DotNet
- Desktop
- Security proportional
- Operations proportional

## Nicht benötigte Profile V1

- Web Service/API Production
- Cloud Native
- Kubernetes
- Multi-Tenant SaaS
- Mobile

## Pflichtnachweise vor V1

- reproduzierbarer Build/Test
- Migrationstest
- Backup-/Restore-Test
- Architecture Tests
- Security Review
- DPI-/Accessibility-Check
- Pilot mit echten Lerninhalten

## Re-Klassifikation

Erneut prüfen bei Cloud Sync, Multiuser, öffentlicher API, Provider-Credentials, externer AI-Datenübertragung oder Team-/HR-Funktionen.
