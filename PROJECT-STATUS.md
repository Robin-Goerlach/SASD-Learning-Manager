# Project Status – SASD Learning Manager

**Stand:** 2026-08-28

## Phase

Implementation. **Milestone 0 bis Milestone 4 sind als Code-Stand umgesetzt; M4 Hotfix 001 behebt einen fehlerhaften Testquelltext-Export.**

## Letzter bestätigter Build

Milestone 3 auf Windows:

```text
Build succeeded
0 Warning(s)
0 Error(s)
48 / 48 Tests grün
```

## Milestone 4 Hotfix 001

- [x] Ursache des fehlgeschlagenen M4-Builds lokalisiert: ausschließlich `Application.Tests/TestDoubles.cs`, physische Zeile 275
- [x] 112 versehentlich literal geschriebene `\n`-Sequenzen in echte Zeilenumbrüche zurückgeführt
- [x] kompletter C#-Quelltext auf vergleichbare Escape-/Delimiter-Artefakte geprüft
- [x] Migrationen 0001–0003 bytegleich zum bestätigten M3-Stand
- [x] SQL-Zeilenenden künftig explizit LF
- [x] Milestone-ZIPs künftig aus Git ausgeschlossen

## Milestone 4 implementiert

- [x] Learning Paths
- [x] Goal ↔ LearningPath
- [x] hierarchische LearningPathNodes
- [x] Required / Optional
- [x] Node Type / Status / SortOrder
- [x] Move Up / Down
- [x] Parent-Wechsel mit Zyklenschutz
- [x] Skill ↔ Node
- [x] Resource ↔ Node
- [x] Node Relations
- [x] Subtree Archive / Restore
- [x] Path Progress
- [x] TreeView Workspace
- [x] Migration 0004
- [x] neue Domain/Application/Infrastructure Tests

## Erwarteter Testumfang M4

64 Facts:

- Domain 23
- Application 26
- Infrastructure 11
- Architecture 4

Der M4-Windows-Build/Test ist nach Auslieferung auszuführen.

## Als Nächstes

Nach grünem M4-Build: **Milestone 5 – Knowledge & Evidence**.
