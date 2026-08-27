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

Für die Pre-Implementation-/Pilotphase ist `Private` sinnvoll. Vor einem öffentlichen Release sollen Lizenz, Security-Kontakt und Contribution-Modell bewusst entschieden werden.

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

Diese ZIP enthält noch **keinen Build-Workflow**, weil noch keine `.sln` existiert. Milestone 0 erzeugt Solution und Tests und fügt anschließend eine echte `dotnet restore/build/test`-Action hinzu. So bleibt das Repository von Anfang an grün statt mit einer absichtlich fehlschlagenden CI zu starten.
