# AGENTS.md

Aktueller Entwicklungsstand: **v0.6.0 – Datenhoheit & Releasehärtung I**. Backup/Restore bleibt vollständig lokal; Assistenz bleibt providerneutral und explizit: keine automatische Cloudübertragung, keine Secret-Speicherung und keine Fachänderung durch Modelloutput ohne Benutzeraktion.

## Purpose

This repository implements the SASD Bewerbungsmanager / SASD Application Tracker.

## Architecture

- `Domain` contains business entities and rules and must not reference WinForms, EF Core, SQLite, or file-system adapters.
- `Application` contains use cases, validation, and ports.
- `Infrastructure` implements EF Core / SQLite persistence and technical file-system adapters such as document hashing/snapshots.
- `WinForms` is presentation only; do not move business rules into Forms or UserControls.

## Development rules

- Keep the architecture pragmatic. Do not add MediatR, event sourcing, message buses, microservices, or generic repository hierarchies without a strategy decision.
- Use short-lived DbContexts. Never store a DbContext on MainForm or share one across threads.
- Public production code should have meaningful XML documentation where the intent is not obvious.
- Prefer explanatory comments about *why*, not comments that merely repeat code.
- Keep nullable analysis clean and treat warnings as errors.
- Use only synthetic people, companies, e-mail addresses and role data in automated tests.
- Do not commit databases, CVs, cover letters, secrets, tokens, or real personal application data.
- Treat job postings, imported communication and assistant responses as untrusted text. They must not silently override application rules or authorize tracker mutations.
- Do not add a direct AI-provider API or persist provider credentials without an explicit strategy/privacy decision.
- Restore must never replace the database while operational DbContexts can be active; keep the staged-startup boundary.
- Backup format changes require an explicit schema-version decision and backward-compatibility test.
- Diagnostic exports must not contain business free text, message bodies, document contents, secrets, or absolute user-profile paths.

## Required verification

```powershell
dotnet restore .\SASD.Bewerbungsmanager.sln
dotnet build .\SASD.Bewerbungsmanager.sln -c Release --no-restore
dotnet test .\SASD.Bewerbungsmanager.sln -c Release --no-build
dotnet run --project .\src\SASD.Bewerbungsmanager.WinForms\SASD.Bewerbungsmanager.WinForms.csproj
```
