# Milestone 4 – Learning Paths

**Stand:** 2026-08-28  
**Status:** Implementiert, Windows-Build/Test nach Auslieferung zu verifizieren

## Ziel

Milestone 4 erweitert den SASD Learning Manager um echte strukturierte Lernpfade. Ein Learning Path ist nicht nur eine Playlist: Er besitzt eine hierarchische Struktur und getrennte fachliche Relationen zwischen Nodes.

## Implementiert

- LearningPath Domain Entity
- LearningPathNode Domain Entity
- LearningPathNodeRelation
- Required/Optional Nodes
- Path-/Node-Status und Priorität
- Planned Start / Target Date / Next Action
- Goal ↔ LearningPath
- LearningPathNode ↔ Skill
- LearningPathNode ↔ Resource
- hierarchische Parent/Child-Struktur
- SortOrder und Move Up/Down
- Zyklenschutz beim Parent-Wechsel
- Subtree-Archivierung
- kontrollierte Node-Wiederherstellung
- Node-Relationen:
  - Requires
  - AlternativeTo
  - RecommendedBefore
  - RecommendedAfter
  - Deepens
  - Related
- Core Progress getrennt nach Required/Optional
- Learning-Path-Workspace mit TreeView
- Path Editor
- Node Editor
- Relationsdialog
- Migration `0004_learning_paths.sql`

## Zentrale Invarianten

```text
Tree Parent/Child  !=  fachliche Relation
```

```text
Required Nodes bestimmen den Core Progress.
Optional Nodes blockieren Completion nicht.
```

```text
Resource/Skill Assignment verändert weder Resource Completion
noch Skill Mastery automatisch.
```

## Datenbank

Neue Tabellen:

```text
LearningPaths
GoalLearningPaths
LearningPathNodes
LearningPathNodeSkills
LearningPathNodeResources
LearningPathNodeRelations
```

Migrationen 0001–0003 wurden gegenüber dem verifizierten M3-Stand nicht verändert.

## Tests

Neu vorgesehen:

- 6 Domain Tests
- 7 Application Tests
- 3 Infrastructure Tests

Gesamterwartung nach M4:

```text
Domain          23
Application     26
Infrastructure  11
Architecture     4
------------------
Total           64
```

## Verifikation vor Auslieferung

Durchgeführt in der Erstellungsumgebung:

- Migration 0001 unchanged
- Migration 0002 unchanged
- Migration 0003 unchanged
- Migration 0004 SQLite execution PASS
- `PRAGMA foreign_key_check` PASS
- `PRAGMA integrity_check` PASS
- 15 ProjectReferences vorhanden
- Projekt-/Props-XML valide
- C# struktureller Delimiter-Check PASS
- bekannte xUnit Analyzer-Antipatterns nicht gefunden

Ein echter .NET-Windows-Compilerlauf ist in der Erstellungsumgebung nicht möglich. Der maßgebliche Build-/Testnachweis erfolgt deshalb auf dem Windows-Entwicklungsrechner.

## Hotfix-Hinweis

Die erste M4-ZIP enthielt einen beschädigten Testdouble-Block. Siehe [`MILESTONE-4-HOTFIX-001.md`](MILESTONE-4-HOTFIX-001.md).
