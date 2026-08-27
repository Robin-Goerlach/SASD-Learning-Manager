# Backup and Restore Plan

## Backup Package

```text
SASD-LearningManager-Backup-YYYYMMDD-HHMMSS.zip
├── manifest.json
├── database/learning-manager.db
└── settings/exportable-settings.json
```

Extern referenzierte Dateien werden V1 nicht automatisch kopiert.

## Konsistenz

Bei WAL SQLite Backup API bzw. nachgewiesen sichere Snapshot-Strategie. Kein blindes File Copy.

## Manifest

App Version, Schema Version, Backup Format, CreatedAtUtc, Dateiliste, SHA-256.

## Restore

```text
ZIP validate
→ path validation
→ manifest/version/hash
→ temp DB open
→ integrity_check
→ safety backup current DB
→ replace
→ migrations
→ restart
```

## Tests

Seed → Backup → Mutate → Restore → Entities + Relations vergleichen.

Backup und Export werden in der UI klar unterschieden.
