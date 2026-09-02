# Ressourcen per CSV importieren und exportieren

## Zweck

Der SASD Learning Manager kann seine Ressourcenbibliothek als portable CSV-Datei exportieren und eine Datei desselben Formats wieder importieren. Damit können größere Kurslisten, Buchempfehlungen und Testdaten eingespielt werden, ohne die SQLite-Datenbank außerhalb der Anwendung zu verändern.

Die Funktion verwendet bewusst die normalen Application Services. Dadurch gelten beim CSV-Import dieselben Regeln wie bei manueller Erfassung, insbesondere URL-Dublettenerkennung, Provider-Validierung, Resource-Statusregeln und Tag-Normalisierung.

## Menü

```text
Daten
├── Ressourcen aus CSV importieren …
└── Ressourcen als CSV exportieren …
```

## CSV-Spalten

Die Spaltenreihenfolge ist Teil des V1-Formats und muss beim Import unverändert bleiben:

```text
Title
Type
Provider
Url
LocalPath
Description
WhySaved
Creator
LanguageCode
VersionText
EstimatedMinutes
Difficulty
Priority
Status
ProgressPercent
Tags
```

### Pflichtfelder

- `Title`
- `Type`
- `Difficulty`
- `Priority`
- `Status`

### Resource-Typen

```text
Course
Video
Book
Article
Document
Documentation
Lab
Project
Podcast
PracticeExam
Event
Repository
Other
```

### Difficulty

```text
Unknown
Beginner
Intermediate
Advanced
Expert
```

### Priority

```text
Low
Normal
High
VeryHigh
```

### Status

```text
Inbox
Planned
Started
Paused
Deferred
Completed
Abandoned
Archived
```

`Archived` wird beim Import nicht direkt erzeugt. Die Ressource wird zunächst regulär angelegt und anschließend über den normalen Archive-Use-Case archiviert.

## Provider

`Provider` enthält den Namen, nicht eine interne GUID. Existiert der Provider bereits, wird er wiederverwendet. Ist er archiviert, wird er vor der Zuordnung wiederhergestellt. Ein noch unbekannter Provider wird als minimaler Provider vom Typ `Other` angelegt und kann später über die Providerverwaltung ergänzt werden.

## Tags

Mehrere Tags werden innerhalb des CSV-Feldes mit Semikolon getrennt:

```text
linux;performance;ex442
```

Der CSV-Codec selbst verwendet Komma als Spaltentrenner und unterstützt Anführungszeichen, Kommas und Zeilenumbrüche innerhalb korrekt gequoteter Felder.

## URL-Dubletten

Die Canonical-Resource-Regel bleibt aktiv. Eine Ressource mit einer bereits vorhandenen normalisierten URL wird nicht still ein zweites Mal angelegt. Der Importbericht weist die betreffende CSV-Zeile als übersprungene Dublette aus.

Ressourcen ohne URL können mehrfach vorkommen, weil ohne stabilen externen Identifier keine sichere automatische Dublettenerkennung möglich ist.

## Importbericht

Nach einem Import zeigt die Anwendung:

- Anzahl gelesener Datenzeilen,
- Anzahl neu angelegter Ressourcen,
- Anzahl übersprungener URL-Dubletten,
- Anzahl Hinweise/Fehler,
- die ersten zehn zeilenbezogenen Diagnosen.

Ein fachlich fehlerhafter Datensatz beendet nicht automatisch den kompletten Import. Fehlerhafte Zeilen werden ausgelassen und gemeldet. Technische Datei-/Zugriffsfehler werden als Abbruch behandelt.

## Export

Der Export enthält auch archivierte Ressourcen und schreibt UTF-8 mit BOM, damit Umlaute unter Windows und in Tabellenkalkulationen zuverlässig erkannt werden.

Der Export ist zugleich die einfachste Möglichkeit, eine eigene Importvorlage zu erzeugen:

1. Eine oder zwei Ressourcen manuell anlegen.
2. `Daten → Ressourcen als CSV exportieren …` wählen.
3. Die erzeugte CSV als Vorlage für weitere Datensätze verwenden.

## Mitgelieferte Testdaten

Unter

```text
testdata/import/resources-chat-recommendations.csv
```

liegt ein größerer, aus früheren Weiterbildungsgesprächen abgeleiteter Beispieldatensatz. Er enthält unter anderem Linux Performance/EX442, Docker, Proxmox/Ceph, Cloud, Blue Team, OPNsense/pfSense, DevOps, Codex/Harness Engineering, AI Security, MERN/RAG, Data Analytics, Python und Scrum.

Die Kurs-/Buchtitel sind aus früheren Gesprächen abgeleitet. Status- und Fortschrittswerte sind reine Demo-/Testwerte und dürfen nicht als reale Lernhistorie interpretiert werden.

## Grenzen dieses V1-Formats

Der erste CSV-Transfer konzentriert sich bewusst auf die Ressourcenbibliothek. Beziehungen zu Goals, Skills, Learning Paths, Knowledge und Evidence werden nicht in dieselbe CSV eingebettet. Diese Beziehungen benötigen stabile portable IDs bzw. ein Paketformat und sollen später als eigener Import/Export-Ausbauschritt umgesetzt werden.
