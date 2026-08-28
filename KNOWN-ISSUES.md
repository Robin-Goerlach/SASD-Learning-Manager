# Known Issues und Release-Gates

> Stand: 2026-08-28  
> Geltungsbereich: Weg zu 1.0.0

Jeder Punkt muss vor GA entweder geschlossen, ausdrücklich als akzeptierte Einschränkung dokumentiert
oder in eine spätere Version verschoben werden.

## Offen für RC/1.0

| ID | Thema | Status / nächste Entscheidung |
|---|---|---|
| KI-001 | Installer und Code Signing | offen; `Publish-Release.ps1` erzeugt Paket + Hash, aber noch keinen Installer und keine Signatur |
| KI-006 | UI-Automation | offen; zentrale Flows benötigen vor RC1 reproduzierbare manuelle Abnahme |
| KI-009 | Veröffentlichungslizenz | offen; vor Public Release final festlegen |
| KI-010 | Performance-Referenz | offen; Zielbestand 10.000 Vorgänge / 50.000 Aktivitäten noch nicht gemessen |
| KI-011 | Restore-End-to-End | technisch implementiert; praktischen verschlüsselten Restore auf realitätsnaher Profilkopie nachweisen |
| KI-013 | DPI / Accessibility | PerMonitorV2 aktiv; systematische 100/125/150/200-%-Abnahme noch offen |

## Erledigt / technisch umgesetzt

| Frühere ID | Thema | Ergebnis |
|---|---|---|
| KI-002 | SQLite-Pragmas / GUID-Baseline | aktuelle EF-/SQLite-Kette produktiv in Verwendung; M0-Schiene entfernt |
| KI-004 | Backupverschlüsselung | v0.7.0: passwortgeschützte `.sasdbak` mit PBKDF2, AES-256-CBC und HMAC-SHA-256 |
| KI-005 | Restore-Generation-Switch | Restore erfolgt über validiertes Staging + Pending-Marker vor DB-Initialisierung |
| KI-007 | CSV-Parser | Job-Source-CSV-Adapter mit Tests vorhanden |
| KI-008 | File Logging | lokales Diagnose-Logging vorhanden |
| KI-012 | Infrastructure.Tests / Defender | Repository-Gate schlägt jetzt fehl, wenn ein Testprojekt 0 Tests discovered |

## Bewusste V1-Grenzen

- Windows 11 x64 ist die primär unterstützte Plattform.
- Kein Cloud-Sync und keine Mehrbenutzer-/Mehrschreibersemantik.
- Kein Auto-Updater als V1-Pflicht.
- Kein dauerhafter IMAP/OAuth-Mailboxzugriff.
- Kein Browser-Autofill oder Auto-Apply.
- Keine KI-Cloud als notwendige Betriebsabhängigkeit.
- Kein serverseitiger Dienst.
- Downgrade auf eine ältere Binärversion bei neuerem DB-Schema wird nicht automatisch unterstützt.
- Das Backup-Passwort kann nicht durch die Anwendung wiederhergestellt werden.
