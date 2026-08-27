# Data Model – SASD Learning Manager

**Status:** Proposed  
**Stand:** 2026-08-27

## Ziel

Das Modell soll langfristig verständliche, relationale Lernhistorie ermöglichen. Wesentliche Prinzipien sind Canonical Resource, explizite Relationen, Append-Historie für Skill Assessments und Archive over Delete.

## Kernmodell

```text
Goal ── Skill ── SkillAssessment
 │        │           │
 │        ├──── Evidence
 │        └──── KnowledgeArtifact
 │
 └── LearningPath ── LearningPathNode ── Resource ── Provider
                              │               │
                              └──── Skill     ├── Topic
                                              ├── Tag
                                              └── ResourceRelation
```

## Entity-Konvention

Langlebige Entities besitzen `Id`, `CreatedAtUtc`, `UpdatedAtUtc`; archivierungsfähige zusätzlich `ArchivedAtUtc?`.

IDs: GUID als SQLite TEXT. Zeit: `DateTimeOffset`, persistent UTC ISO-8601 TEXT.

## Goal

Felder: Title, Description, GoalType, Motivation, Priority, Status, TargetDate, NextAction, Created/Updated/Achieved/Archived.

Relationen: `GoalSkill`, `GoalLearningPath`.

## CompetencyArea / Topic / Skill

- CompetencyArea: grobe Domäne.
- Topic: Wissensbereich.
- Skill: bewertbare Fähigkeit.

Skill:

```text
Name
Description
CurrentLevel?
TargetLevel?
LastUsedAtUtc?
NextReviewAtUtc?
Status
```

Skill-Level: `null` = nicht bewertet, 1..5 = qualitative Kompetenzstufen.

## SkillAssessment

Append-orientiert:

```text
Id
SkillId
Level
AssessmentType
Reason
AssessedAtUtc
CreatedAtUtc
```

Historie ist fachliche Quelle; `CurrentLevel` darf als Snapshot gehalten werden.

## LearningNeed

Status Open / Planned / Addressed / Closed / Archived. Kann Goal, Skill, Topic, Resource oder Path referenzieren.

## LearningPath

Felder: Title, Description, Status, Priority, PlannedStartDate, TargetDate, NextAction, Started/Completed/Archived.

Goals sind many-to-many.

## LearningPathNode

Adjacency List:

```text
Id
LearningPathId
ParentNodeId?
Title
Description
NodeType
SortOrder
IsRequired
Status
```

Hierarchie und fachliche Beziehungen sind getrennt. Relations: Requires, AlternativeTo, RecommendedBefore/After, Deepens, Related.

## Provider

Provider ist Datenobjekt, nicht hardcodierter Integrationstyp.

## Resource

Felder u. a.:

```text
Title
ResourceType
ProviderId?
Url?
NormalizedUrl?
LocalPath?
Description
WhySaved
Creator
LanguageCode
PublishedDate
VersionText
EstimatedMinutes
Difficulty
Priority
Status
ProgressPercent
NextAction
StartedAtUtc
CompletedAtUtc
```

Types: Course, Video, Book, Article, Document, Documentation, Lab, Project, Podcast, PracticeExam, Event, Repository, Other.

Status: Inbox, Planned, Started, Paused, Deferred, Completed, Abandoned, Archived.

## Canonical Resource

Eine Resource wird einmal gespeichert. Mehrfachnutzung über:

```text
ResourceSkill
ResourceTopic
ResourceTag
LearningPathNodeResource
```

## ResourceRelation

AlternativeTo, OverlapsWith, Supersedes, Deepens, Requires, RecommendedBefore/After, RelatedTo.

## KnowledgeArtifact

Markdown-Inhalt. Typen: Note, Summary, CheatSheet, CodeSnippet, LessonLearned, Question, CommandReference, Procedure, Other.

Mehrfachrelation zu Resources, Skills, Topics, Goals und Paths.

## Evidence

Typen: CourseCompletion, Assessment, Quiz, Lab, Project, Certificate, PracticalUse, Documentation, Presentation, SelfAssessment, Other.

Evidence kann mehrere Skills unterstützen.

## Tags

Querschnittsmetadaten; ersetzen keine fachlichen Objekte.

## ActivityLog

Fachliche Timeline, kein Event Sourcing.

## Join Tables

```text
GoalSkill
GoalLearningPath
CompetencyAreaTopic
CompetencyAreaSkill
TopicSkill
ResourceSkill
ResourceTopic
ResourceTag
LearningPathNodeSkill
LearningPathNodeResource
KnowledgeArtifactResource
KnowledgeArtifactSkill
KnowledgeArtifactTopic
KnowledgeArtifactGoal
KnowledgeArtifactLearningPath
EvidenceSkill
EvidenceResource
EvidenceGoal
```

## DB-Regeln

- `PRAGMA foreign_keys = ON`
- Default Delete: RESTRICT
- Progress 0..100
- Skill-Level null/1..5
- Enums bevorzugt TEXT
- SQL parameterisiert
- archivierte Daten bleiben historisch referenzierbar

## Indizes

NormalizedUrl, Resource Status/Provider/Type/Priority, Skill Name/Status, Path Status, Node PathId/ParentNodeId, Tag Name und alle relevanten Join-FKs.

## Migration

Nummerierte SQL-Migrationen plus `SchemaMigrations(Version, Name, AppliedAtUtc, Checksum)`. Angewandte Migrationen werden nicht nachträglich still verändert.
