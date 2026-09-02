# AGENTS.md — SASD Learning Manager

Repository-wide instructions for coding agents working on **SASD Learning Manager**.

> **Critical repository boundary:** This repository currently contains accidentally committed files from `SASD.Bewerbungsmanager`. They are unrelated contamination. Do not extend, refactor, reference, stage, or use them as architecture examples for Learning Manager work. The authoritative solution is `SASD.LearningManager.sln` and the authoritative production namespaces start with `SASD.LearningManager`.

## 1. Product mission

SASD Learning Manager is a local-first Windows desktop application for provider-independent personal learning, learning-path and competency management.

Core conceptual workflow:

```text
Goal
  ↓
Competency / Skill
  ↓
Learning Path
  ↓
Resource
  ↓
Learning Activity / Progress
  ↓
Knowledge / Evidence
  ↓
Skill Assessment
  ↓
Review / Retention
```

Product principles:

- **Skill before Course** — a course is a learning resource, not the competency itself.
- **Canonical Resource** — store one resource once and reference it from multiple contexts.
- **Provider neutral** — O'Reilly, LinkedIn Learning, Udemy, YouTube, books, docs, labs and projects are peers.
- **Capture now, classify later** — Quick Capture and Inbox must remain fast.
- **Completion != Mastery != Retention** — completing a resource must never silently change a skill level.
- **Local first** — the core must work offline without mandatory cloud services.
- **Human in control** — future AI features may propose; they must not silently rewrite the learning record.

## 2. Authoritative repository scope

Before non-trivial work:

1. Confirm the repository root.
2. Confirm `SASD.LearningManager.sln` exists.
3. Run or inspect `git status --short`.
4. Read the current `README.md`, `PROJECT-STATUS.md`, `CHANGELOG.md`, relevant `docs/`, migrations and tests.
5. Work only in Learning Manager projects unless the user explicitly requests repository cleanup.

Authoritative production projects:

```text
src/
├── SASD.LearningManager.Domain
├── SASD.LearningManager.Application
├── SASD.LearningManager.Infrastructure
└── SASD.LearningManager.WinForms
```

Authoritative test projects:

```text
tests/
├── SASD.LearningManager.Domain.Tests
├── SASD.LearningManager.Application.Tests
├── SASD.LearningManager.Infrastructure.Tests
└── SASD.LearningManager.Architecture.Tests
```

Do **not** use these accidental files as part of the product:

```text
SASD.Bewerbungsmanager.sln
src/SASD.Bewerbungsmanager.*
tests/SASD.Bewerbungsmanager.*
```

Other suspicious root files clearly belonging to another SASD product must be reported rather than incorporated into Learning Manager behavior.

## 3. Architecture

Dependency direction:

```text
WinForms       → Application → Domain
Infrastructure → Application + Domain
```

Forbidden dependencies:

```text
Domain         → Infrastructure
Domain         → WinForms
Application    → WinForms
Infrastructure → WinForms
```

### Domain

Owns entities, value semantics, lifecycle rules and invariants. It must not know about WinForms, SQLite dialogs or concrete repositories.

### Application

Owns use cases, DTOs/read models, validation across aggregates and ports/interfaces. Business rules must not exist only in UI event handlers.

### Infrastructure

Owns SQLite connections, repositories, migrations, technical adapters and persistence transactions.

### WinForms

Owns presentation, navigation, dialogs and user interaction. Keep event handlers short and delegate work to Application services.

## 4. Technology baseline

Unless an explicit architecture decision changes it:

- C# / .NET 8
- Windows Forms
- SQLite
- local-first single-user desktop application
- Microsoft.Extensions.Hosting / dependency injection / logging
- parameterized SQL and specialized repositories
- versioned SQL migrations
- xUnit v3
- nullable enabled
- warnings treated seriously; target 0 warnings / 0 errors
- no mandatory cloud
- no provider credentials in the V1 core
- no AI dependency in the core

Do not migrate to a heavy ORM, another UI framework, microservices, CQRS frameworks or another database as a side effect of feature work.

## 5. Domain invariants that must survive every change

### Canonical Resource

A resource exists once and may be linked to many goals, skills, topics or learning-path contexts. Do not create copies merely because a resource is used in more than one place.

### Completion versus mastery

```text
Resource completed
       !=
Skill mastered
```

Only an explicit skill assessment may establish/change the assessed skill level. Evidence may support an assessment but must not automatically set mastery.

### Skill scale

The existing nullable 1..5 model remains authoritative unless deliberately changed by a documented domain decision.

### Learning Paths

Tree hierarchy and semantic relations are different concepts:

```text
Parent/Child = structural hierarchy
Relation     = Requires / AlternativeTo / RecommendedBefore / ...
```

Keep cycle protection. Required and optional nodes must remain distinguishable in core-progress calculations.

### Archive over destructive deletion

Historical learning records should normally be archived/restored rather than physically deleted.

## 6. Current implementation baseline

Always verify this against `PROJECT-STATUS.md` and the code before assuming it, but the current direction is:

- M0 technical baseline — implemented
- M1 Providers & Resource Library — implemented
- M2 Quick Capture & Inbox — implemented
- M3 Goals & Skills — implemented
- M4 Learning Paths — implemented
- M5 Knowledge & Evidence backend — implemented
- M5 dedicated WinForms workspaces — still pending
- direct SkillAssessment ↔ Evidence assignment — still pending
- portable Resource CSV import/export — implemented on the import/export feature branch
- Dashboard/Search — later milestone
- Backup/Restore/release hardening — later milestone

Do not mark unfinished UI as complete merely because backend entities exist.

## 7. Knowledge and Evidence

`KnowledgeArtifact` is reusable learning output, not a note field hidden inside a Resource.

Typical examples:

- Note
- Summary
- Cheat Sheet
- Procedure
- Runbook
- Lesson Learned
- Code / command reference

Evidence is a separate concept representing support for demonstrated competence, for example a lab, project, certificate, work product or demonstration.

Rules:

- Knowledge and Evidence remain distinct.
- Evidence and SkillAssessment remain distinct.
- Evidence may be linked to skills/resources/goals according to the existing model.
- Evidence must never silently promote CurrentLevel.
- Keep Markdown/plain-text/local-link behavior local-first.

## 8. CSV resource import/export

The first portable CSV format intentionally transfers the **resource library**, not the entire relational graph.

Authoritative user documentation:

```text
docs/user/RESOURCE-CSV-IMPORT-EXPORT.md
```

Authoritative fixture:

```text
testdata/import/resources-chat-recommendations.csv
```

When changing the CSV contract:

1. update the Application service/codec,
2. update user documentation,
3. update the shipped fixture,
4. update tests,
5. ensure the shipped fixture itself still passes through the production import service.

Do not bypass `ProviderService` / `ResourceService` with direct SQLite inserts during normal import. Canonical URL checks, provider validation, lifecycle rules and tag normalization must remain active.

## 9. Database and migrations

Before a schema change:

1. inspect the latest migration and migration runner,
2. never rewrite an already-applied historical migration just to fit a new feature,
3. add a new migration,
4. keep checksum-sensitive historical migration text stable,
5. keep `PRAGMA foreign_keys = ON`,
6. test fresh-database migration and supported upgrade path,
7. consider backup/restore implications.

Use parameterized SQL only. Treat imported text, URLs and file paths as untrusted input.

## 10. Code quality and documentation

The user wants to be able to read and learn from the code.

- Prefer boring, explicit, maintainable C# over clever compression.
- Do not generate giant single-line classes or multiple unrelated statements per line.
- Public Domain/Application APIs should have meaningful XML documentation where intent is not obvious.
- Use code comments for **why**, invariants and non-obvious decisions; do not narrate trivial syntax.
- Keep naming consistent with the domain language in requirements and UI.
- Preserve cancellation tokens on async APIs.
- Do not suppress analyzer warnings merely to obtain a green build.
- Keep nullable analysis clean.

## 11. Testing expectations

Every material change should be tested at the narrowest useful layer.

### Domain tests

Test invariants, lifecycle rules and calculations.

### Application tests

Test orchestration, validation, duplicate behavior and relationship rules.

### Infrastructure tests

Use a real temporary SQLite database for migrations, repositories, foreign keys and transactional persistence where appropriate.

### Architecture tests

Do not weaken architecture tests to make a shortcut compile.

### Test fixtures

Fixtures must be synthetic or clearly marked demo data. A demo course status, skill assessment or Evidence record must not be presented as the user's real history unless explicitly supplied as such.

## 12. Required verification gate

A task is not complete because code was generated.

From the repository root run:

```powershell
dotnet clean .\SASD.LearningManager.sln
dotnet restore .\SASD.LearningManager.sln
dotnet build .\SASD.LearningManager.sln -c Release --no-restore
dotnet test .\SASD.LearningManager.sln -c Release --no-build
```

Target:

```text
Build succeeded.
0 Warning(s)
0 Error(s)
all tests passed
```

Do not invent test counts. Report the counts from the actual latest run.

When GitHub Actions is available, use the Windows CI result as an additional authoritative gate. If CI fails, inspect the exact compiler/test error, fix it and rerun; do not declare the task finished first.

## 13. Git workflow

For non-trivial work:

1. start from current `main`,
2. create a focused feature/fix branch,
3. make focused commits,
4. inspect the PR diff,
5. run the full CI gate,
6. keep unrelated cleanup in a separate PR when practical.

Do not use `git add .` blindly in a contaminated working tree. Do not commit build output, local databases, secrets, user learning data or generated milestone ZIPs.

Do not merge a PR unless the user has requested/authorized the merge or the established workflow explicitly delegates merging.

## 14. Repository cleanup rule

The accidentally committed Bewerbungsmanager files are a known maintenance issue. When cleanup is requested, remove them in a dedicated branch/PR and prove that `SASD.LearningManager.sln` still builds/tests unchanged. Do not combine mass deletion with a feature implementation unless there is a compelling technical reason.

## 15. Completion report

At the end of a coding task report only verified facts:

```text
Implemented:
- ...

Schema:
- migration ... / no schema change

Tests:
- added ...
- actual passed count ...

Verification:
- restore ...
- build ...
- tests ...

Manual checks still useful:
- ...

Known limitations / deliberately deferred work:
- ...
```

Never claim a build, test, migration or UI behavior was verified if it was not actually verified.
