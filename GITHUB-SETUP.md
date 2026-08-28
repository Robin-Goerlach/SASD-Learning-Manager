# GitHub Setup – SASD Learning Manager

## Empfohlene Repository-Einstellungen

**Name:** `SASD-Learning-Manager`

**Description:**

> Local-first Windows-Desktop-App zur strukturierten Planung und Dokumentation persönlicher Weiterbildung über O’Reilly, LinkedIn Learning, YouTube, Udemy, Bücher, Doku und Labs. Verknüpft Lernziele, Skills, Learning Paths, Ressourcen, Fortschritt, Wissen und Evidence. C#/.NET 8, WinForms, SQLite.

**Default Branch:** `main`

**Topics:**

```text
learning-manager
personal-learning
learning-paths
skills
competency-management
dotnet
winforms
sqlite
local-first
```

## Sichtbarkeit

Für die aktuelle Implementierungs-/Pilotphase ist `Private` sinnvoll. Vor einem öffentlichen Release sollen Lizenz, Security-Kontakt und Contribution-Modell bewusst entschieden werden.

## Lizenz

Noch **nicht festgelegt**. Das Pflichtenheft markiert die Produktlizenz als strategische Entscheidung. Deshalb enthält diese Repository-Baseline bewusst keine erfundene Lizenz.

Vor Public Release:

1. Lizenz auswählen.
2. `LICENSE` hinzufügen.
3. README und Third-Party-Notices abgleichen.

## Branch Protection ab Milestone 0

Empfohlen für `main`:

- Pull Request vor Merge
- Build/Test-Statuscheck erforderlich
- keine Force Pushes
- keine direkten Releases aus ungeprüften Commits

## Actions

Seit Milestone 0 enthält das Repository eine Windows-basierte GitHub-Actions-CI für `dotnet restore`, `dotnet build` und `dotnet test`. Pull Requests auf `main` sollen diesen Statuscheck bestehen.

Der zuletzt vollständig auf Windows bestätigte Stand ist Milestone 3 mit 0 Warnungen, 0 Fehlern und 48/48 Tests. Milestone 4 Hotfix 001 behebt den fehlerhaften Testquelltext der ersten M4-ZIP und ist erneut auf Windows zu verifizieren.
