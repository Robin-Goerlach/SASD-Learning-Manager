# SASD Learning Manager – Lastenheft

**Projekt:** SASD Learning Manager  
**Dokumenttyp:** Lastenheft / fachliche Anforderungsspezifikation  
**Dokumentstatus:** Entwurf zur strategischen Prüfung  
**Version:** 0.1  
**Stand:** 27. August 2026  
**Sprache:** Deutsch  
**Verantwortungsbereich:** Strategie / Produktdefinition  
**Bezugsdokument:** `SASD-Learning-Manager-Vorlagen-Funktionsanalyse.md`  
**Normativer Projektbezug:** SASD Development Standard, aktueller `main`-Stand  
**Repository des Standards:** <https://github.com/Robin-Goerlach/SASD-Development-Standard>

---

## 0. Dokumentzweck und Einordnung

Dieses Lastenheft beschreibt **was** der SASD Learning Manager fachlich leisten soll, **warum** diese Leistungen benötigt werden und **woran** die Erfüllung wesentlicher Anforderungen später überprüft werden kann.

Es beschreibt bewusst noch nicht im Detail, **wie** die Anforderungen technisch umgesetzt werden. Entscheidungen zu Programmiersprache, UI-Technologie, Datenbank, konkreter Softwarearchitektur, Bibliotheken, Deployment oder Integrationsmechanismen gehören in nachgelagerte Dokumente wie Project Brief, Architektur, Pflichtenheft, ADRs und Implementierungsplanung.

Das Lastenheft wurde aus der vorherigen Funktionsanalyse von 23 Referenzprodukten abgeleitet. Die dort untersuchten Produkte werden **als Inspirations- und Benchmark-Quellen** verwendet. Der SASD Learning Manager soll keines dieser Produkte nachbauen.

Berücksichtigt wurden insbesondere:

- Degreed
- roadmap.sh
- Zotero
- Karakeep
- Readwise Reader
- Pluralsight
- O’Reilly Learning
- Heptabase
- RemNote
- Class Central
- Khan Academy
- Obsidian
- LinkedIn Learning
- Moodle
- 360Learning
- Docebo
- Raindrop.io
- Capacities
- Notion
- Linkwarden
- linkding
- LearnAwesome
- Anki

Das Dokument orientiert sich an der Anforderungsstruktur des SASD Development Standard. Der Standard empfiehlt, Anforderungen nachvollziehbar mit ID, Typ, Priorität, Status, Akzeptanzkriterium und Nachweis zu führen. Der aktuelle Standard bezeichnet sein Repository weiterhin als **Version 1.0 Specification Candidate**; die normative Baseline ist freigegeben, `1.0.0` jedoch noch nicht veröffentlicht.

---

# 1. Ausgangssituation

## 1.1 Problemstellung

Persönliche berufliche Weiterbildung verteilt sich heute auf zahlreiche Anbieter, Medien und Werkzeuge. Typische Lernressourcen stammen beispielsweise aus:

- O’Reilly Learning,
- LinkedIn Learning,
- YouTube,
- Udemy,
- Herstellerdokumentationen,
- Blogs und Fachartikeln,
- Büchern und E-Books,
- PDFs und Whitepapers,
- Standards und RFCs,
- Konferenzen und Vorträgen,
- Podcasts,
- Labs und Sandboxes,
- Zertifizierungsvorbereitungen,
- eigenen Testumgebungen,
- eigenen Projekten.

Bestehende Plattformen verwalten meist nur ihren **eigenen** Inhalt. Bookmark-Manager sammeln Links, beantworten aber nicht zuverlässig die Frage, **warum** eine Ressource wichtig ist oder **welche Kompetenz** damit aufgebaut werden soll. Notizsysteme dokumentieren Wissen, planen aber nicht automatisch die Weiterbildung. LMS- und LXP-Systeme bieten umfangreiche Kompetenz- und Lernpfadfunktionen, sind jedoch häufig auf Organisationen, HR-Prozesse oder einen definierten Content-Katalog ausgerichtet.

Dadurch entstehen für eine lernende Person mehrere Probleme:

1. Lernressourcen liegen verstreut in Browser-Lesezeichen, Playlists, Plattformbibliotheken, Notizen und Dateien.
2. Es fehlt eine anbieterunabhängige Gesamtübersicht.
3. Ein gespeicherter Kurs wird schnell mit einem tatsächlichen Lernziel verwechselt.
4. Es ist unklar, welche Ressource für welchen Skill vorgesehen ist.
5. Ähnliche Kurse werden mehrfach gespeichert oder gekauft, obwohl sie sich stark überschneiden.
6. Voraussetzungen und sinnvolle Lernreihenfolgen gehen verloren.
7. Fortschritt wird nur innerhalb einzelner Plattformen angezeigt.
8. Ein abgeschlossener Kurs sagt wenig darüber aus, ob der Inhalt verstanden, angewendet oder später noch erinnert wird.
9. Wissen aus Videos, Büchern, Artikeln und Labs wird nicht einheitlich dokumentiert.
10. Lernpläne werden schnell so umfangreich, dass Prioritäten verloren gehen.
11. Bei längerfristigen Weiterbildungszielen fehlt der Zusammenhang zwischen Zielrolle, Kompetenzlücke und konkreten Lernaktivitäten.
12. Nach einigen Monaten ist schwer nachvollziehbar, welche Themen bereits praktisch eingesetzt oder nur einmal angesehen wurden.

## 1.2 Produktidee

Der SASD Learning Manager soll diese Lücke durch ein persönliches, anbieterunabhängiges **Learning-Portfolio- und Competency-Management-System** schließen.

Das Grundmodell lautet:

> **Ziel → Kompetenzbereich → Skill/Topic → Lernpfad → Lernressource → Lernaktivität → Wissensartefakt/Evidenz → Kompetenzentwicklung → Review**

Eine Lernressource ist damit **nicht das Lernziel**, sondern eines von mehreren möglichen Mitteln, um ein Lernziel zu erreichen.

## 1.3 Leitgedanke

Der Learning Manager soll nicht primär fragen:

> „Welche Kurse habe ich gespeichert?“

sondern:

> „Was möchte ich können, was fehlt mir noch, welche Ressourcen helfen mir dabei und welche Evidenz zeigt, dass ich Fortschritt gemacht habe?“

---

# 2. Produktvision

Der SASD Learning Manager soll eine dauerhafte persönliche Steuerzentrale für berufliche und fachliche Weiterbildung werden.

Er soll es ermöglichen,

- komplexe Themengebiete übersichtlich zu strukturieren,
- Lernziele und Zielrollen zu definieren,
- benötigte Kompetenzen und Skills festzuhalten,
- Wissenslücken sichtbar zu machen,
- Lernpfade als strukturierte Landkarten aufzubauen,
- beliebige externe Lernressourcen einzubinden,
- Fortschritt unabhängig vom Anbieter zu dokumentieren,
- Wissen und praktische Evidenz mit den zugrunde liegenden Skills zu verbinden,
- veraltetes oder lange nicht verwendetes Wissen zu erkennen,
- und aus einer großen Sammlung von Möglichkeiten eine überschaubare nächste Lernaktion abzuleiten.

Der Learning Manager soll den Benutzer **beim Lernen unterstützen**, ihn aber nicht mit Verwaltung überlasten.

---

# 3. Ziele

## 3.1 Primäre Ziele

### Z-01 – Zentrale Weiterbildungsübersicht

Alle relevanten Lernziele, Lernpfade und Ressourcen sollen in einer zentralen, anbieterunabhängigen Struktur auffindbar sein.

### Z-02 – Kompetenzorientierung

Lernen soll an Kompetenzen und Skills ausgerichtet werden und nicht an der bloßen Anzahl gespeicherter oder abgeschlossener Kurse.

### Z-03 – Gezielte Priorisierung

Der Benutzer soll erkennen können, welches Thema bzw. welche Lernaktivität als Nächstes sinnvoll ist.

### Z-04 – Wiederverwendbare Ressourcen

Eine Ressource soll nur einmal gepflegt und anschließend beliebig vielen Themen, Skills und Lernpfaden zugeordnet werden können.

### Z-05 – Anbieterunabhängigkeit

O’Reilly, LinkedIn Learning, YouTube, Udemy und andere Anbieter sollen fachlich gleichberechtigte Quellen sein.

### Z-06 – Langfristige Nachvollziehbarkeit

Auch Monate oder Jahre später soll nachvollziehbar sein,

- warum eine Ressource aufgenommen wurde,
- zu welchem Lernziel sie gehörte,
- ob sie bearbeitet wurde,
- welche Erkenntnisse daraus entstanden,
- und welche Kompetenz damit belegt oder vertieft wurde.

### Z-07 – Geringe Verwaltungsbarriere

Eine interessante Ressource soll sehr schnell erfasst werden können. Die vollständige Klassifikation darf später erfolgen.

### Z-08 – Trennung von Completion, Mastery und Retention

Der Learning Manager soll klar unterscheiden zwischen:

- **Resource Completion:** Wie weit wurde eine Ressource bearbeitet?
- **Skill Mastery:** Wie gut wird ein Thema beherrscht?
- **Retention/Recency:** Wie aktuell und abrufbar ist das Wissen noch?

### Z-09 – Datenhoheit

Die persönlichen Lern- und Kompetenzdaten sollen exportierbar, sicherbar und nicht an einen einzelnen externen Lernanbieter gebunden sein.

### Z-10 – Erweiterbarkeit

Das fachliche Modell soll spätere Funktionen wie Browser-Capture, Schnittstellen, AI-Unterstützung, Spaced Repetition oder Teamfunktionen ermöglichen, ohne dass diese bereits Teil des ersten Produktkerns sein müssen.

---

# 4. Nicht-Ziele

Der SASD Learning Manager soll in seiner ersten Produktgeneration ausdrücklich **nicht** zu einem vollständigen Ersatz für folgende Produktklassen werden:

- Learning Management System für Schulen oder Unternehmen,
- HR- oder Talentmanagementsystem,
- Moodle-/Docebo-Ersatz,
- O’Reilly-/LinkedIn-/Udemy-Contentplattform,
- Video-Hosting-Plattform,
- vollständiger Video-Player,
- vollständiger PDF-Reader,
- vollständiger E-Book-Reader,
- Webbrowser,
- Zotero-Ersatz für wissenschaftliche Zitation,
- Obsidian-/Notion-Ersatz,
- vollständiges Flashcard-/Spaced-Repetition-System,
- vollständige Prüfungsplattform,
- Zertifizierungsanbieter,
- öffentlicher Kursmarktplatz,
- soziales Netzwerk,
- universelle Wissensdatenbank für alle Menschen,
- automatisches HR-Skill-Rating,
- autonome AI, die ungeprüft Kompetenzniveaus festlegt,
- Scraping-Plattform, die gegen Nutzungsbedingungen externer Anbieter arbeitet.

Die Anwendung darf diese Systeme später **ergänzen, integrieren oder verlinken**, soll deren Spezialfunktionen aber nicht unnötig duplizieren.

---

# 5. Zielgruppen und Stakeholder

## 5.1 Primäre Zielgruppe

Die erste Zielgruppe ist eine einzelne Person, die ihre berufliche und fachliche Weiterbildung langfristig plant und über viele Anbieter und Medien hinweg organisiert.

Typische Eigenschaften:

- mehrere parallele Lernfelder,
- langjährige Weiterbildung,
- technischer oder fachlicher Beruf,
- Nutzung verschiedener Kursplattformen,
- Lernen über Bücher, Videos, Dokumentation und praktische Übungen,
- Bedarf an langfristiger Nachvollziehbarkeit,
- Wunsch nach eigenständiger Priorisierung.

## 5.2 Mögliche spätere Zielgruppen

Spätere Produktversionen können optional unterstützen:

- Lernende in Ausbildung oder Studium,
- Freelancer,
- technische Administratoren und Entwickler,
- Trainer,
- kleine Teams,
- Mentoren,
- Unternehmen ohne großes LMS,
- Weiterbildungsgemeinschaften.

Diese Gruppen sind **nicht maßgeblich für die Komplexität der ersten Version**.

## 5.3 Stakeholder

| Stakeholder | Interesse |
|---|---|
| Lernender | Übersicht, Planung, Fortschritt, Wiederfinden |
| Produktverantwortung | sinnvoller Funktionsumfang, Produktfokus |
| Entwicklung | klare fachliche Anforderungen |
| spätere Tester | überprüfbare Akzeptanzkriterien |
| Datenschutz/Sicherheit | Schutz persönlicher Lern- und Profildaten |
| Integrationsanbieter | saubere, kontrollierte Schnittstellen |
| zukünftige Maintainer | nachvollziehbares Domain Model und Anforderungen |

---

# 6. Begriffe und fachliche Definitionen

## 6.1 Goal / Lernziel

Ein konkretes fachliches, berufliches oder persönliches Entwicklungsziel.

Beispiele:

- „Docker sicher im beruflichen Alltag einsetzen“
- „Blue-Team-Grundlagen erwerben“
- „OpenStack Administrator werden“
- „Auf eine Zielrolle als Platform Engineer vorbereiten“
- „EX442 vorbereiten“

## 6.2 Career Goal / Zielrolle

Ein spezielles Lernziel, das auf eine berufliche Rolle oder Entwicklung ausgerichtet ist.

## 6.3 Competency Area / Kompetenzbereich

Eine größere fachliche Domäne.

Beispiele:

- Cyber Security
- Linux Administration
- Cloud Engineering
- Softwareentwicklung

## 6.4 Topic / Thema

Ein fachlicher Teilbereich innerhalb eines Kompetenzbereiches.

## 6.5 Skill

Eine konkrete Fähigkeit oder Wissenskompetenz, deren Entwicklungsstand bewertet oder durch Evidenz belegt werden kann.

## 6.6 Learning Need

Ein identifizierter Lernbedarf, für den möglicherweise noch kein Lernpfad oder keine geeignete Ressource ausgewählt wurde.

## 6.7 Learning Path

Eine strukturierte Zusammenstellung von Themen, Skills, Modulen und Lernressourcen, die auf ein Lernziel hinführt.

## 6.8 Learning Path Node

Ein Element innerhalb eines Lernpfades. Ein Node kann beispielsweise ein Modul, Thema, Skill, Checkpoint oder praktische Aufgabe repräsentieren.

## 6.9 Resource / Lernressource

Ein externes oder eigenes Lernmittel.

Beispiele:

- Kurs,
- Video,
- Buch,
- Artikel,
- PDF,
- Dokumentation,
- Lab,
- Projekt,
- Practice Exam,
- Podcast.

## 6.10 Provider

Quelle bzw. Anbieter einer Lernressource.

Beispiele:

- O’Reilly,
- LinkedIn Learning,
- YouTube,
- Udemy,
- Red Hat,
- Microsoft Learn,
- eigene Quelle.

## 6.11 Learning Activity

Die tatsächliche Bearbeitung oder Nutzung einer Ressource bzw. eines Lernschritts.

## 6.12 Knowledge Artifact

Ein aus dem Lernen entstandenes Wissensobjekt, beispielsweise:

- Notiz,
- Zusammenfassung,
- Cheatsheet,
- Code-Snippet,
- Befehlssammlung,
- Lessons Learned.

## 6.13 Evidence

Ein Nachweis, der zur Einschätzung einer Kompetenz herangezogen werden kann.

Beispiele:

- bestandener Test,
- abgeschlossenes Lab,
- Zertifikat,
- eigenes Projekt,
- praktische Arbeit,
- Dokumentation,
- Vortrag.

## 6.14 Completion

Bearbeitungsstand einer Ressource oder eines Lernpfades.

## 6.15 Mastery

Einschätzung des Beherrschungsgrades eines Skills.

## 6.16 Retention / Recency

Hinweis darauf, wie aktuell bzw. wie lange zuletzt mit einem Skill oder Wissensinhalt gearbeitet wurde.

## 6.17 Inbox

Eingangsbereich für neu erfasste Ressourcen, die noch nicht vollständig klassifiziert wurden.

---

# 7. Priorisierungsmodell

Für dieses Lastenheft wird eine erweiterte MoSCoW-Klassifikation verwendet.

| Priorität | Bedeutung |
|---|---|
| **Must** | Für den fachlichen Produktkern zwingend erforderlich |
| **Should** | Hoher Nutzen; sollte nach Möglichkeit früh verfügbar sein |
| **Could** | Sinnvoller Ausbau, aber kein Blocker für den Produktkern |
| **Later** | Bewusst spätere Produktgeneration |
| **Won’t now** | Für den derzeit definierten Produktumfang ausdrücklich nicht vorgesehen |

Zusätzlich wird ein Zielhorizont verwendet:

- **V1 Core** – erster belastbarer Produktkern
- **V1.x** – direkte Weiterentwicklung nach dem Kern
- **V2+** – spätere Ausbaustufe
- **Future/Optional** – langfristige Option

Alle Anforderungen starten mit Status **Proposed**, bis sie im Projekt formell akzeptiert werden.

---

# 8. Produktprinzipien

## PP-01 – Learning first, administration second

Die Anwendung darf nicht mehr Verwaltungsaufwand erzeugen, als sie beim Lernen einspart.

## PP-02 – Skill before course

Der fachliche Bezug zu Ziel, Thema oder Skill hat Vorrang vor der bloßen Sammlung von Kursen.

## PP-03 – Canonical Resource

Eine Ressource wird einmal gepflegt und mehrfach referenziert.

## PP-04 – Provider neutral

Eine Quelle wird aufgrund ihres Lernwerts betrachtet, nicht aufgrund des Anbieters.

## PP-05 – Capture now, classify later

Schnelles Erfassen ist wichtiger als sofort vollständige Metadaten.

## PP-06 – Completion is not mastery

Ein Ressourcenabschluss darf nicht automatisch mit Kompetenzbeherrschung gleichgesetzt werden.

## PP-07 – Evidence over assumption

Kompetenzbewertungen sollen – soweit verfügbar – durch nachvollziehbare Evidenz gestützt werden.

## PP-08 – User remains in control

Automatische oder AI-basierte Vorschläge dürfen den Benutzer unterstützen, aber fachliche Zuordnungen und Bewertungen nicht ungefragt überschreiben.

## PP-09 – Open and portable data

Persönliche Lernhistorie und Wissensdaten sollen langfristig exportierbar bleiben.

## PP-10 – Progressive complexity

Der Nutzer soll mit einem einfachen Workflow beginnen können; erweiterte Konzepte wie Skill-Level, Evidence oder Retention müssen dort vertiefbar sein, wo sie Nutzen bringen.

---

# 9. Fachlicher Gesamtworkflow

Ein typischer End-to-End-Workflow soll folgendermaßen möglich sein:

```text
Lernziel definieren
        ↓
Kompetenzbereiche / Skills auswählen
        ↓
Lernbedarf erkennen
        ↓
Learning Path erstellen oder zuordnen
        ↓
Ressourcen sammeln
        ↓
Ressourcen priorisieren
        ↓
Lernen / praktische Übungen durchführen
        ↓
Fortschritt dokumentieren
        ↓
Notizen und Evidence erfassen
        ↓
Skill-Level neu einschätzen
        ↓
Review / Auffrischung planen
```

Gleichzeitig muss ein wesentlich kürzerer Alltagsworkflow möglich sein:

```text
Interessante URL gefunden
        ↓
In Inbox speichern
        ↓
später klassifizieren
```

---

# 10. Funktionale Anforderungen

---

## 10.1 Ziele und persönliche Lernvorhaben

### Ziel

Der Benutzer soll seine Weiterbildung von konkreten fachlichen oder beruflichen Zielen aus strukturieren können.

| ID | Anforderung | Priorität | Zielhorizont | Status | Akzeptanzkriterium |
|---|---|---:|---|---|---|
| REQ-F-GOAL-001 | Der Benutzer kann ein Lernziel anlegen. | Must | V1 Core | Proposed | Ein Ziel kann mit mindestens Titel, Beschreibung und Status gespeichert werden. |
| REQ-F-GOAL-002 | Ein Lernziel kann als fachliches Ziel, Karriereziel, Zertifizierungsziel, Projektziel oder freies Ziel klassifiziert werden. | Should | V1 Core | Proposed | Mindestens die genannten Kategorien sind auswählbar oder erweiterbar. |
| REQ-F-GOAL-003 | Ziele können priorisiert werden. | Must | V1 Core | Proposed | Ziele lassen sich mindestens in mehrere Prioritätsstufen einordnen. |
| REQ-F-GOAL-004 | Ziele besitzen einen Lebenszyklus. | Must | V1 Core | Proposed | Mindestens Planned, Active, Paused, Achieved und Archived sind abbildbar. |
| REQ-F-GOAL-005 | Ein Ziel kann optional ein Zieldatum besitzen. | Should | V1 Core | Proposed | Ein Termin kann gesetzt, geändert und entfernt werden. |
| REQ-F-GOAL-006 | Ein Ziel kann mehreren Kompetenzbereichen und Skills zugeordnet werden. | Must | V1 Core | Proposed | Mehrfachzuordnung ist ohne Duplikation möglich. |
| REQ-F-GOAL-007 | Ein Ziel kann mit einem oder mehreren Learning Paths verbunden werden. | Must | V1 Core | Proposed | Die Verbindung ist von Ziel und Path aus sichtbar. |
| REQ-F-GOAL-008 | Ein Ziel zeigt seinen zusammengefassten Fortschritt. | Should | V1.x | Proposed | Fortschritt wird nachvollziehbar aus verbundenen Elementen oder manueller Bewertung abgeleitet. |
| REQ-F-GOAL-009 | Ein Ziel kann Gründe bzw. Motivation dokumentieren. | Should | V1 Core | Proposed | Freitextfeld für Motivation/Warum ist verfügbar. |
| REQ-F-GOAL-010 | Ein Ziel kann archiviert werden, ohne seine Historie zu verlieren. | Must | V1 Core | Proposed | Archivierte Ziele und Zuordnungen bleiben abrufbar. |
| REQ-F-GOAL-011 | Ziele können untereinander Beziehungen besitzen. | Could | V1.x | Proposed | Mindestens „Teilziel von“, „unterstützt“ und „ersetzt“ sind modellierbar. |
| REQ-F-GOAL-012 | Ein Ziel kann als Vorlage für ein neues Ziel kopiert werden. | Could | V1.x | Proposed | Kopie übernimmt ausgewählte Struktur, ohne Historie zu duplizieren. |

---

## 10.2 Kompetenzbereiche, Topics und Skills

### Ziel

Der Benutzer soll das eigene Lernfeld unabhängig von Kursanbietern in einer fachlichen Struktur abbilden können.

| ID | Anforderung | Priorität | Zielhorizont | Status | Akzeptanzkriterium |
|---|---|---:|---|---|---|
| REQ-F-SKILL-001 | Kompetenzbereiche können angelegt, bearbeitet und archiviert werden. | Must | V1 Core | Proposed | CRUD und Archivierung sind möglich. |
| REQ-F-SKILL-002 | Topics können einem oder mehreren Kompetenzbereichen zugeordnet werden. | Must | V1 Core | Proposed | Mehrfachzuordnung ist möglich. |
| REQ-F-SKILL-003 | Skills können angelegt, bearbeitet und archiviert werden. | Must | V1 Core | Proposed | Skill enthält mindestens Name, Beschreibung und Status. |
| REQ-F-SKILL-004 | Skills können hierarchisch oder logisch miteinander verbunden werden. | Should | V1 Core | Proposed | Über-/Unterordnung oder Relation ist abbildbar. |
| REQ-F-SKILL-005 | Ein Skill kann mehreren Topics oder Kompetenzbereichen zugeordnet werden. | Must | V1 Core | Proposed | Keine Duplikation des Skills erforderlich. |
| REQ-F-SKILL-006 | Für einen Skill kann ein aktuelles Kompetenzniveau erfasst werden. | Must | V1 Core | Proposed | Eine definierte Skala ist auswählbar. |
| REQ-F-SKILL-007 | Für einen Skill kann ein Zielniveau erfasst werden. | Must | V1 Core | Proposed | Soll- und Ist-Level können gleichzeitig angezeigt werden. |
| REQ-F-SKILL-008 | Aus Ist- und Zielniveau kann eine sichtbare Kompetenzlücke abgeleitet werden. | Must | V1 Core | Proposed | Gap wird mindestens qualitativ oder numerisch angezeigt. |
| REQ-F-SKILL-009 | Änderungen des Skill-Levels sollen historisch nachvollziehbar sein. | Should | V1.x | Proposed | Frühere Bewertungen bleiben mit Datum sichtbar. |
| REQ-F-SKILL-010 | Eine Skill-Bewertung kann einen Kommentar bzw. Begründung enthalten. | Should | V1 Core | Proposed | Bewertung kann mit Freitext begründet werden. |
| REQ-F-SKILL-011 | Skill-Level sollen durch Evidence referenziert werden können. | Should | V1 Core | Proposed | Evidence kann einem Skill zugeordnet werden. |
| REQ-F-SKILL-012 | Die Anwendung darf einen Kursabschluss nicht automatisch als vollständige Skill-Beherrschung interpretieren. | Must | V1 Core | Proposed | Completion und Mastery bleiben technisch/fachlich getrennt. |
| REQ-F-SKILL-013 | Skill-Bezeichnungen sollen Synonyme bzw. alternative Namen unterstützen können. | Could | V1.x | Proposed | Ein Skill kann Aliasnamen führen. |
| REQ-F-SKILL-014 | Skills können mit Tags versehen werden. | Should | V1 Core | Proposed | Mehrere Tags sind möglich. |
| REQ-F-SKILL-015 | Skills können als veraltet, ersetzt oder nicht mehr relevant markiert werden. | Could | V1.x | Proposed | Historie bleibt bestehen. |
| REQ-F-SKILL-016 | Kompetenzmodelle sollen später importierbar sein. | Later | V2+ | Proposed | Importkonzept ist architektonisch nicht ausgeschlossen. |

### Empfohlene erste Skill-Level-Skala

Das endgültige Bewertungsmodell wird später fachlich festgelegt. Als verständlicher Startpunkt wird eine fünfstufige Skala empfohlen:

1. **Unbekannt / keine Erfahrung**
2. **Grundverständnis**
3. **Arbeitsfähig mit Unterstützung**
4. **Selbstständig und sicher anwendbar**
5. **Vertieft / kann erklären, beurteilen und komplex anwenden**

Die Skala soll ausdrücklich keine wissenschaftlich exakte Messung vortäuschen.

---

## 10.3 Learning Needs / Lernbedarf

| ID | Anforderung | Priorität | Zielhorizont | Status | Akzeptanzkriterium |
|---|---|---:|---|---|---|
| REQ-F-NEED-001 | Ein Lernbedarf kann unabhängig von einer konkreten Ressource erfasst werden. | Should | V1 Core | Proposed | Lernbedarf kann mit Titel und Beschreibung gespeichert werden. |
| REQ-F-NEED-002 | Ein Lernbedarf kann einem Ziel, Skill oder Topic zugeordnet werden. | Should | V1 Core | Proposed | Mindestens eine solche Zuordnung ist möglich. |
| REQ-F-NEED-003 | Lernbedarfe können priorisiert werden. | Should | V1 Core | Proposed | Priorität ist sichtbar und filterbar. |
| REQ-F-NEED-004 | Ein Lernbedarf kann den Status Open, Planned, Addressed oder Closed besitzen. | Should | V1 Core | Proposed | Statuswechsel ist möglich. |
| REQ-F-NEED-005 | Ein Lernbedarf kann später mit passenden Ressourcen oder Learning Paths verknüpft werden. | Should | V1 Core | Proposed | Beziehung bleibt nachvollziehbar. |
| REQ-F-NEED-006 | Die Anwendung soll Lernbedarfe ohne zugeordnete Ressource sichtbar machen können. | Should | V1.x | Proposed | Smart View „Lernbedarf ohne Ressource“ ist verfügbar. |

---

## 10.4 Learning Paths und Roadmaps

### Ziel

Ein Learning Path soll eine **fachliche Lernlandkarte** sein und nicht lediglich eine lineare Playlist.

| ID | Anforderung | Priorität | Zielhorizont | Status | Akzeptanzkriterium |
|---|---|---:|---|---|---|
| REQ-F-PATH-001 | Der Benutzer kann Learning Paths anlegen, bearbeiten, kopieren und archivieren. | Must | V1 Core | Proposed | Grundoperationen sind verfügbar. |
| REQ-F-PATH-002 | Ein Learning Path besitzt mindestens Titel, Beschreibung, Status, Priorität und optionales Lernziel. | Must | V1 Core | Proposed | Felder können gepflegt werden. |
| REQ-F-PATH-003 | Ein Path kann in Module, Topics oder andere Nodes gegliedert werden. | Must | V1 Core | Proposed | Mehrstufige Struktur ist möglich. |
| REQ-F-PATH-004 | Nodes können hierarchisch verschachtelt werden. | Must | V1 Core | Proposed | Mindestens mehrere Ebenen sind darstellbar. |
| REQ-F-PATH-005 | Nodes können einem oder mehreren Skills zugeordnet werden. | Must | V1 Core | Proposed | Skillbezug ist sichtbar. |
| REQ-F-PATH-006 | Ressourcen können an beliebige Nodes gehängt werden. | Must | V1 Core | Proposed | Mehrere Ressourcen pro Node sind möglich. |
| REQ-F-PATH-007 | Eine Ressource kann in mehreren Learning Paths verwendet werden. | Must | V1 Core | Proposed | Ressourcendatensatz wird nicht dupliziert. |
| REQ-F-PATH-008 | Nodes können Pflicht- oder Optional-Status besitzen. | Should | V1 Core | Proposed | Kennzeichnung ist sichtbar. |
| REQ-F-PATH-009 | Nodes bzw. Ressourcen können eine empfohlene Reihenfolge besitzen. | Must | V1 Core | Proposed | Reihenfolge kann gepflegt und angezeigt werden. |
| REQ-F-PATH-010 | Voraussetzungen zwischen Nodes sollen modellierbar sein. | Should | V1 Core | Proposed | Mindestens „requires“ ist verfügbar. |
| REQ-F-PATH-011 | Alternative Nodes oder Ressourcen sollen modellierbar sein. | Should | V1 Core | Proposed | Mindestens „alternative to“ ist verfügbar. |
| REQ-F-PATH-012 | Vertiefungsbeziehungen sollen modellierbar sein. | Should | V1.x | Proposed | „deepens“ oder vergleichbare Relation ist möglich. |
| REQ-F-PATH-013 | Ein Path kann freie und sequenzielle Teile kombinieren. | Should | V1.x | Proposed | Nicht jeder Node muss strikt gesperrt sequenziell sein. |
| REQ-F-PATH-014 | Der Fortschritt eines Learning Paths wird aus seinen Bestandteilen nachvollziehbar dargestellt. | Must | V1 Core | Proposed | Fertig/Offen und mindestens eine Fortschrittskennzahl sind sichtbar. |
| REQ-F-PATH-015 | Der Fortschritt muss zwischen Pflicht- und Optional-Elementen unterscheiden können. | Should | V1.x | Proposed | Optionales Material verfälscht nicht zwingend den Kernabschluss. |
| REQ-F-PATH-016 | Ein Path kann pausiert werden. | Must | V1 Core | Proposed | Pausierter Path bleibt erhalten und erscheint nicht zwingend als aktuelle Aufgabe. |
| REQ-F-PATH-017 | Ein Path kann abgeschlossen werden. | Must | V1 Core | Proposed | Abschlussdatum wird gespeichert. |
| REQ-F-PATH-018 | Ein abgeschlossener Path kann später erneut geöffnet oder als Auffrischung kopiert werden. | Could | V1.x | Proposed | Historie bleibt erhalten. |
| REQ-F-PATH-019 | Eine übersichtliche Roadmap-/Strukturansicht soll verfügbar sein. | Should | V1 Core | Proposed | Benutzer erkennt Module, Status und Reihenfolge ohne mehrere Detailseiten. |
| REQ-F-PATH-020 | Eine grafische Netz-/Roadmap-Darstellung darf später ergänzt werden. | Later | V2+ | Proposed | Datenmodell verhindert diese Darstellung nicht. |
| REQ-F-PATH-021 | Learning Paths sollen versionierbar bzw. revisionsfähig sein. | Could | V2+ | Proposed | Änderungen können später nachvollziehbar gemacht werden. |
| REQ-F-PATH-022 | Ein Path kann aus einer Vorlage erzeugt werden. | Could | V1.x | Proposed | Vorlage wird ohne Lernhistorie kopiert. |
| REQ-F-PATH-023 | Ein Path kann mehrere Provider mischen. | Must | V1 Core | Proposed | Keine Providerbeschränkung innerhalb eines Paths. |
| REQ-F-PATH-024 | Ein Path kann auch rein praktische Nodes ohne externe Ressource enthalten. | Must | V1 Core | Proposed | z. B. „Lab aufbauen“ oder „Projekt implementieren“ ist möglich. |

---

## 10.5 Lernressourcenbibliothek

### Ziel

Alle externen und eigenen Lernmittel sollen anbieterunabhängig als kanonische Ressourcen verwaltet werden.

| ID | Anforderung | Priorität | Zielhorizont | Status | Akzeptanzkriterium |
|---|---|---:|---|---|---|
| REQ-F-RES-001 | Der Benutzer kann eine Lernressource anlegen. | Must | V1 Core | Proposed | Mindestens Titel und Ressourcentyp können gespeichert werden. |
| REQ-F-RES-002 | Ressourcen besitzen einen eindeutigen internen Datensatz. | Must | V1 Core | Proposed | Mehrfachzuordnung erzeugt keine Kopie. |
| REQ-F-RES-003 | Ressourcen können einem Provider zugeordnet werden. | Must | V1 Core | Proposed | Provider ist separat pflegbar. |
| REQ-F-RES-004 | Ressourcen können eine URL besitzen. | Must | V1 Core | Proposed | URL kann gespeichert und geöffnet werden. |
| REQ-F-RES-005 | Ressourcen können einen lokalen Dateiverweis besitzen. | Should | V1 Core | Proposed | Lokaler Pfad oder vergleichbarer Verweis kann gepflegt werden. |
| REQ-F-RES-006 | Die Anwendung unterstützt unterschiedliche Ressourcentypen. | Must | V1 Core | Proposed | Mindestens Course, Video, Book, Article, PDF/Document, Documentation, Lab, Project und Other. |
| REQ-F-RES-007 | Weitere Ressourcentypen sollen ergänzbar sein. | Should | V1.x | Proposed | Typmodell ist nicht unnötig starr. |
| REQ-F-RES-008 | Ressourcen können Beschreibung und persönliche Begründung enthalten. | Must | V1 Core | Proposed | Beschreibung und „Warum gespeichert?“ sind pflegbar. |
| REQ-F-RES-009 | Ressourcen können Sprache speichern. | Should | V1 Core | Proposed | Sprache ist filterbar. |
| REQ-F-RES-010 | Ressourcen können Autor, Trainer oder Creator speichern. | Should | V1 Core | Proposed | Mindestens Freitext oder strukturierter Bezug ist möglich. |
| REQ-F-RES-011 | Ressourcen können Veröffentlichungsdatum bzw. Jahr speichern. | Should | V1 Core | Proposed | Datum/Jahr ist pflegbar. |
| REQ-F-RES-012 | Ressourcen können Dauer oder geschätzten Lernaufwand speichern. | Should | V1 Core | Proposed | Wert und Einheit sind speicherbar. |
| REQ-F-RES-013 | Ressourcen können ein Schwierigkeitsniveau besitzen. | Should | V1 Core | Proposed | Filterbare Stufe ist verfügbar. |
| REQ-F-RES-014 | Ressourcen können eine persönliche Qualitätsbewertung erhalten. | Should | V1.x | Proposed | Bewertung ist getrennt von Provider-/Communitywerten. |
| REQ-F-RES-015 | Ressourcen können eine Priorität besitzen. | Must | V1 Core | Proposed | Priorität ist filter- und sortierbar. |
| REQ-F-RES-016 | Ressourcen können Kosten bzw. Zugriffsart dokumentieren. | Could | V1.x | Proposed | z. B. Subscription, Free, Purchased, Unknown. |
| REQ-F-RES-017 | Ressourcen können Tags besitzen. | Must | V1 Core | Proposed | Mehrfachzuordnung ist möglich. |
| REQ-F-RES-018 | Ressourcen können mehreren Skills zugeordnet werden. | Must | V1 Core | Proposed | Mehrfachzuordnung funktioniert. |
| REQ-F-RES-019 | Ressourcen können mehreren Topics zugeordnet werden. | Must | V1 Core | Proposed | Mehrfachzuordnung funktioniert. |
| REQ-F-RES-020 | Ressourcen können in mehreren Learning Paths verwendet werden. | Must | V1 Core | Proposed | Canonical-Resource-Prinzip bleibt erhalten. |
| REQ-F-RES-021 | Ressourcen können Beziehungen untereinander besitzen. | Should | V1 Core | Proposed | Mindestens Alternative, Overlaps, Replaces/Supersedes, Deepens und Recommended Before/After sind abbildbar. |
| REQ-F-RES-022 | Eine Ressource kann als veraltet markiert werden. | Should | V1.x | Proposed | Grund und Datum können dokumentiert werden. |
| REQ-F-RES-023 | Eine Ressource kann einen Nachfolger referenzieren. | Should | V1.x | Proposed | „Superseded by“ ist sichtbar. |
| REQ-F-RES-024 | Ressourcen können archiviert werden, ohne Lernhistorie zu verlieren. | Must | V1 Core | Proposed | Archivierte Ressourcen bleiben referenzierbar. |
| REQ-F-RES-025 | Die Anwendung unterstützt eigene Ressourcen ohne externe URL. | Must | V1 Core | Proposed | Eigene Labs/Projekte können angelegt werden. |
| REQ-F-RES-026 | Die Anwendung soll doppelte URLs erkennen können. | Should | V1 Core | Proposed | Beim Speichern einer bekannten URL wird gewarnt oder bestehende Ressource angeboten. |
| REQ-F-RES-027 | Weitere Duplikatmerkmale wie DOI/ISBN dürfen später genutzt werden. | Could | V2+ | Proposed | Datenmodell lässt Identifier zu. |
| REQ-F-RES-028 | Ressourcen sollen eine Aktualitäts-/Review-Information besitzen können. | Could | V1.x | Proposed | z. B. „zuletzt auf Aktualität geprüft“. |
| REQ-F-RES-029 | Eine Ressource kann eine persönliche Kurzbewertung bzw. Empfehlung enthalten. | Should | V1.x | Proposed | Freitext oder strukturierte Bewertung ist verfügbar. |
| REQ-F-RES-030 | Der Benutzer kann erkennen, in welchen Zielen, Skills und Paths eine Ressource verwendet wird. | Must | V1 Core | Proposed | Rückverweise werden angezeigt. |

---

## 10.6 Provider- und Quellenverwaltung

| ID | Anforderung | Priorität | Zielhorizont | Status | Akzeptanzkriterium |
|---|---|---:|---|---|---|
| REQ-F-PROV-001 | Provider können separat angelegt und verwaltet werden. | Must | V1 Core | Proposed | Provider besitzt mindestens Namen. |
| REQ-F-PROV-002 | Provider können Website und Beschreibung besitzen. | Should | V1 Core | Proposed | Metadaten sind speicherbar. |
| REQ-F-PROV-003 | Provider können einem Typ zugeordnet werden. | Could | V1.x | Proposed | z. B. Plattform, Verlag, Hersteller, Community, eigene Quelle. |
| REQ-F-PROV-004 | Provider können aktiv, inaktiv oder nicht mehr genutzt markiert werden. | Could | V1.x | Proposed | Historische Ressourcen bleiben erhalten. |
| REQ-F-PROV-005 | Ressourcenfilter nach Provider ist möglich. | Must | V1 Core | Proposed | Filter liefert korrekte Treffer. |
| REQ-F-PROV-006 | Provider dürfen keine fachliche Besitzstruktur für Skills oder Lernpfade erzwingen. | Must | V1 Core | Proposed | Path kann Provider mischen. |

---

## 10.7 Quick Capture und Inbox

### Ziel

Eine gefundene Ressource soll mit minimalem Aufwand gespeichert werden können.

| ID | Anforderung | Priorität | Zielhorizont | Status | Akzeptanzkriterium |
|---|---|---:|---|---|---|
| REQ-F-CAP-001 | Eine Ressource kann schnell über eine URL erfasst werden. | Must | V1 Core | Proposed | URL-Eingabe erzeugt mit wenigen Schritten einen Inbox-Eintrag. |
| REQ-F-CAP-002 | Für Quick Capture dürfen nur minimale Pflichtangaben verlangt werden. | Must | V1 Core | Proposed | Vollständige Klassifikation ist nicht Voraussetzung zum Speichern. |
| REQ-F-CAP-003 | Neue Quick-Capture-Ressourcen können automatisch in einer Inbox landen. | Must | V1 Core | Proposed | Unklassifizierte Einträge sind gesammelt sichtbar. |
| REQ-F-CAP-004 | Inbox-Einträge können später vollständig klassifiziert werden. | Must | V1 Core | Proposed | Ressource lässt sich Skills, Paths, Provider etc. zuordnen. |
| REQ-F-CAP-005 | Inbox-Einträge können verworfen bzw. archiviert werden. | Must | V1 Core | Proposed | Nicht relevante Einträge können bereinigt werden. |
| REQ-F-CAP-006 | Die Anwendung soll nach Möglichkeit Titel bzw. Basis-Metadaten aus einer URL übernehmen können. | Should | V1.x | Proposed | Erfolgreiche Metadatenabfrage reduziert manuelle Eingabe; Fehler blockiert Speichern nicht. |
| REQ-F-CAP-007 | Automatisch erkannte Metadaten müssen vom Benutzer änderbar sein. | Must | V1.x | Proposed | Benutzer kann Werte überschreiben. |
| REQ-F-CAP-008 | Beim Capture soll eine mögliche URL-Dublette erkannt werden. | Should | V1 Core | Proposed | Bestehender Eintrag wird angeboten. |
| REQ-F-CAP-009 | Browser-Erweiterung oder Share-Integration ist als spätere Funktion vorgesehen. | Later | V2+ | Proposed | Kernworkflow funktioniert bereits ohne Extension. |
| REQ-F-CAP-010 | AI-basierte Klassifikationsvorschläge dürfen später ergänzt werden. | Later | V2+ | Proposed | Vorschläge benötigen Nutzerfreigabe. |

---

## 10.8 Lernaktivität und Fortschritt

| ID | Anforderung | Priorität | Zielhorizont | Status | Akzeptanzkriterium |
|---|---|---:|---|---|---|
| REQ-F-ACT-001 | Eine Ressource besitzt einen Lernstatus. | Must | V1 Core | Proposed | Mindestens Inbox/Planned/Started/Paused/Completed/Abandoned/Archived sind abbildbar. |
| REQ-F-ACT-002 | Für bearbeitbare Ressourcen kann ein Fortschrittswert gepflegt werden. | Must | V1 Core | Proposed | 0–100 % oder fachlich gleichwertige Darstellung. |
| REQ-F-ACT-003 | Der Fortschritt kann manuell aktualisiert werden. | Must | V1 Core | Proposed | Änderung ist ohne Providerintegration möglich. |
| REQ-F-ACT-004 | Startdatum kann erfasst bzw. automatisch gesetzt werden. | Should | V1 Core | Proposed | Datum ist sichtbar. |
| REQ-F-ACT-005 | Abschlussdatum kann erfasst bzw. automatisch gesetzt werden. | Must | V1 Core | Proposed | Completion enthält ein Datum. |
| REQ-F-ACT-006 | Pausierte Ressourcen behalten ihren bisherigen Fortschritt. | Must | V1 Core | Proposed | Pause setzt Fortschritt nicht zurück. |
| REQ-F-ACT-007 | Abgebrochene Ressourcen behalten Historie und Begründung. | Should | V1 Core | Proposed | Abbruchgrund kann dokumentiert werden. |
| REQ-F-ACT-008 | Der Benutzer kann Lernzeit optional erfassen. | Should | V1.x | Proposed | mindestens Gesamtzeit oder Sessions sind abbildbar. |
| REQ-F-ACT-009 | Die Anwendung kann geplante Lernzeit und tatsächliche Lernzeit unterscheiden. | Could | V1.x | Proposed | beide Werte sind getrennt. |
| REQ-F-ACT-010 | Einzelne Lern-Sessions dürfen später protokolliert werden. | Could | V1.x | Proposed | Datum, Dauer und Notiz sind möglich. |
| REQ-F-ACT-011 | Ein Ressourcenabschluss darf nicht automatisch das Skill-Level auf Maximum setzen. | Must | V1 Core | Proposed | Mastery bleibt unabhängig. |
| REQ-F-ACT-012 | Ein Path zeigt den Bearbeitungsstatus seiner Ressourcen und Nodes. | Must | V1 Core | Proposed | Offen/In Arbeit/Fertig ist erkennbar. |
| REQ-F-ACT-013 | Der Benutzer kann „nächste Aktion“ zu einer Ressource oder einem Path hinterlegen. | Should | V1 Core | Proposed | Text und optional Datum sind möglich. |
| REQ-F-ACT-014 | Ressourcen können auf „später“ verschoben werden, ohne archiviert zu werden. | Should | V1 Core | Proposed | Deferred/Backlog-artiger Status ist möglich. |

---

## 10.9 Wissensnotizen und Knowledge Artifacts

### Ziel

Erkenntnisse aus einer Lernressource sollen von der Ressource selbst getrennt und langfristig wiederverwendbar dokumentiert werden können.

| ID | Anforderung | Priorität | Zielhorizont | Status | Akzeptanzkriterium |
|---|---|---:|---|---|---|
| REQ-F-KNOW-001 | Zu Ressourcen können Notizen erfasst werden. | Must | V1 Core | Proposed | Mehrzeiliger Inhalt kann gespeichert werden. |
| REQ-F-KNOW-002 | Notizen können unabhängig von einer Ressource als Knowledge Artifact angelegt werden. | Should | V1 Core | Proposed | Eigenständiges Wissensobjekt ist möglich. |
| REQ-F-KNOW-003 | Knowledge Artifacts können Skills zugeordnet werden. | Should | V1 Core | Proposed | Mehrfachzuordnung möglich. |
| REQ-F-KNOW-004 | Knowledge Artifacts können mehreren Ressourcen zugeordnet werden. | Should | V1.x | Proposed | Zusammenfassung aus mehreren Quellen ist möglich. |
| REQ-F-KNOW-005 | Knowledge Artifacts können typisiert werden. | Should | V1.x | Proposed | z. B. Note, Summary, Cheatsheet, Code Snippet, Lesson Learned, Question. |
| REQ-F-KNOW-006 | Der Ursprung einer Erkenntnis soll nachvollziehbar bleiben. | Must | V1 Core | Proposed | Verlinkte Ressource ist sichtbar. |
| REQ-F-KNOW-007 | Notizen bzw. Knowledge Artifacts sollen in einem offenen Textformat exportierbar sein. | Must | V1.x | Proposed | Mindestens Markdown- oder klar dokumentierter Text-Export. |
| REQ-F-KNOW-008 | Eine Integration bzw. Übergabe an Obsidian oder ähnliche Systeme darf später unterstützt werden. | Could | V2+ | Proposed | Exportformat ist kompatibel genug für Weiterverarbeitung. |
| REQ-F-KNOW-009 | Highlights und exakte Quellenstellen dürfen später als eigener Artifact-Typ unterstützt werden. | Later | V2+ | Proposed | Datenmodell schließt Quellposition nicht aus. |
| REQ-F-KNOW-010 | Vollständiges PDF-/Video-Annotieren ist nicht Kernbestandteil von V1. | Won’t now | V1 | Proposed | Benutzer wird für Spezialfunktionen an Quellsystem verwiesen. |

---

## 10.10 Evidence und Kompetenznachweise

### Ziel

Kompetenzentwicklung soll nicht nur behauptet, sondern soweit sinnvoll mit Nachweisen verknüpft werden können.

| ID | Anforderung | Priorität | Zielhorizont | Status | Akzeptanzkriterium |
|---|---|---:|---|---|---|
| REQ-F-EVD-001 | Evidence kann als eigener Datensatz erfasst werden. | Must | V1 Core | Proposed | Evidence enthält Typ, Titel/Beschreibung und Datum. |
| REQ-F-EVD-002 | Evidence kann einem oder mehreren Skills zugeordnet werden. | Must | V1 Core | Proposed | Mehrfachzuordnung möglich. |
| REQ-F-EVD-003 | Evidence kann einer Lernressource zugeordnet werden. | Should | V1 Core | Proposed | Ursprung ist sichtbar. |
| REQ-F-EVD-004 | Evidence kann einem Lernziel zugeordnet werden. | Should | V1.x | Proposed | Zielbezug ist sichtbar. |
| REQ-F-EVD-005 | Mindestens folgende Evidence-Typen sollen unterstützt werden: Course Completion, Assessment, Quiz/Test, Lab, Project, Certificate, Practical Use, Documentation, Presentation, Self Assessment und Other. | Must | V1 Core | Proposed | Typen sind auswählbar oder erweiterbar. |
| REQ-F-EVD-006 | Evidence kann einen lokalen oder externen Verweis besitzen. | Should | V1 Core | Proposed | Datei/URL/Referenz kann gespeichert werden. |
| REQ-F-EVD-007 | Evidence kann eine persönliche Bewertung enthalten. | Should | V1.x | Proposed | z. B. Aussagekraft oder Ergebnis. |
| REQ-F-EVD-008 | Das System kann beim Skill-Level relevante Evidence anzeigen. | Must | V1 Core | Proposed | Skill-Detail zeigt zugeordnete Nachweise. |
| REQ-F-EVD-009 | Evidence darf nicht automatisch und ohne Regel/Nutzerentscheidung ein Skill-Level überschreiben. | Must | V1 Core | Proposed | Nutzer behält Kontrolle. |
| REQ-F-EVD-010 | Zertifikate können mit Gültigkeits-/Ablaufdatum versehen werden. | Could | V1.x | Proposed | Ablauf kann angezeigt/gefiltert werden. |

---

## 10.11 Mastery und Kompetenzentwicklung

| ID | Anforderung | Priorität | Zielhorizont | Status | Akzeptanzkriterium |
|---|---|---:|---|---|---|
| REQ-F-MAST-001 | Skill Mastery wird unabhängig vom Ressourcenfortschritt gespeichert. | Must | V1 Core | Proposed | Skill kann Level 2 sein, obwohl Kurs 100 % abgeschlossen ist. |
| REQ-F-MAST-002 | Der Benutzer kann Skill Mastery manuell einschätzen. | Must | V1 Core | Proposed | Level und Datum werden gespeichert. |
| REQ-F-MAST-003 | Eine Mastery-Einschätzung kann mit Begründung dokumentiert werden. | Should | V1 Core | Proposed | Kommentar ist möglich. |
| REQ-F-MAST-004 | Zugeordnete Evidence soll bei der Einschätzung sichtbar sein. | Should | V1 Core | Proposed | Skill-Seite listet Evidence. |
| REQ-F-MAST-005 | Frühere Einschätzungen sollen historisch erhalten bleiben. | Should | V1.x | Proposed | Verlauf ist sichtbar. |
| REQ-F-MAST-006 | Das System darf Hinweise geben, wenn Ziel-Level noch nicht erreicht ist. | Should | V1.x | Proposed | Gap ist sichtbar. |
| REQ-F-MAST-007 | Automatische AI-Skill-Bewertung ist kein Bestandteil des V1-Kerns. | Won’t now | V1 | Proposed | Kein Skill-Level wird autonom durch AI gesetzt. |
| REQ-F-MAST-008 | Später dürfen Assessments als stärkere Evidenz in Bewertungsvorschläge einfließen. | Later | V2+ | Proposed | Vorschlag bleibt nachvollziehbar. |

---

## 10.12 Retention, Aktualität und Wiederholung

### Ziel

Die Anwendung soll langfristig berücksichtigen können, dass Wissen veraltet oder vergessen werden kann, ohne einen vollständigen Anki-Klon zu entwickeln.

| ID | Anforderung | Priorität | Zielhorizont | Status | Akzeptanzkriterium |
|---|---|---:|---|---|---|
| REQ-F-RET-001 | Ein Skill kann ein Datum „zuletzt praktisch genutzt/geprüft“ besitzen. | Should | V1.x | Proposed | Datum ist sichtbar. |
| REQ-F-RET-002 | Ein Skill kann einen optionalen Review-Termin besitzen. | Should | V1.x | Proposed | Fällige Reviews sind filterbar. |
| REQ-F-RET-003 | Ein Skill kann einen einfachen Retention-/Confidence-Status besitzen. | Could | V1.x | Proposed | z. B. Current, Review Soon, Stale. |
| REQ-F-RET-004 | Die Anwendung soll veraltete oder lange nicht verwendete Skills hervorheben können. | Could | V2+ | Proposed | konfigurierbare oder manuelle Logik. |
| REQ-F-RET-005 | Ressourcen bzw. Learning Paths können als Auffrischungsmaterial markiert werden. | Should | V1.x | Proposed | Filter „Refresh“ ist möglich. |
| REQ-F-RET-006 | Ein vollständiges Spaced-Repetition-Scheduling ist nicht Teil von V1. | Won’t now | V1 | Proposed | kein Flashcard-Scheduler erforderlich. |
| REQ-F-RET-007 | Export/Integration zu Anki oder RemNote darf später möglich sein. | Later | V2+ | Proposed | offenes Austauschformat wird bevorzugt. |

---

## 10.13 Tags, Kategorien und Beziehungen

| ID | Anforderung | Priorität | Zielhorizont | Status | Akzeptanzkriterium |
|---|---|---:|---|---|---|
| REQ-F-TAG-001 | Ressourcen können mehrere Tags besitzen. | Must | V1 Core | Proposed | Tags sind zuweisbar und filterbar. |
| REQ-F-TAG-002 | Skills und Knowledge Artifacts können Tags besitzen. | Should | V1 Core | Proposed | Querschnittsthemen sind möglich. |
| REQ-F-TAG-003 | Tags dürfen die fachlichen Objekttypen nicht ersetzen müssen. | Must | V1 Core | Proposed | Skill bleibt Skill, Provider bleibt Provider. |
| REQ-F-TAG-004 | Tags können umbenannt werden, ohne Zuordnungen zu verlieren. | Should | V1 Core | Proposed | alle Beziehungen bleiben erhalten. |
| REQ-F-TAG-005 | Unbenutzte Tags sollen bereinigbar sein. | Could | V1.x | Proposed | Wartungsfunktion vorhanden. |
| REQ-F-REL-001 | Das System unterstützt fachliche Beziehungen zwischen Ressourcen. | Should | V1 Core | Proposed | definierte Relationstypen verfügbar. |
| REQ-F-REL-002 | Das System unterstützt fachliche Beziehungen zwischen Skills. | Should | V1.x | Proposed | z. B. requires, related, deepens. |
| REQ-F-REL-003 | Beziehungen sollen gerichtet oder ungerichtet modellierbar sein, soweit fachlich erforderlich. | Could | V2+ | Proposed | unterschiedliche Relationstypen sind möglich. |

---

## 10.14 Suche, Filter und Smart Views

### Ziel

Mit wachsendem Datenbestand darf die Anwendung nicht zu einem unübersichtlichen Archiv werden.

| ID | Anforderung | Priorität | Zielhorizont | Status | Akzeptanzkriterium |
|---|---|---:|---|---|---|
| REQ-F-SRCH-001 | Die Anwendung bietet eine globale Suche. | Must | V1 Core | Proposed | Mindestens Titel, Beschreibung und relevante Textfelder werden durchsucht. |
| REQ-F-SRCH-002 | Ressourcen können nach Provider gefiltert werden. | Must | V1 Core | Proposed | Filter liefert nur passenden Provider. |
| REQ-F-SRCH-003 | Ressourcen können nach Ressourcentyp gefiltert werden. | Must | V1 Core | Proposed | Typfilter ist möglich. |
| REQ-F-SRCH-004 | Ressourcen können nach Status gefiltert werden. | Must | V1 Core | Proposed | z. B. Planned/Started/Completed. |
| REQ-F-SRCH-005 | Ressourcen können nach Priorität gefiltert werden. | Must | V1 Core | Proposed | Filter ist verfügbar. |
| REQ-F-SRCH-006 | Ressourcen können nach Skill und Topic gefiltert werden. | Must | V1 Core | Proposed | fachlicher Filter ist möglich. |
| REQ-F-SRCH-007 | Ressourcen können nach Tag gefiltert werden. | Must | V1 Core | Proposed | mehrere Tags sind nutzbar. |
| REQ-F-SRCH-008 | Such- und Filterergebnisse können sortiert werden. | Must | V1 Core | Proposed | mindestens Titel, Datum, Priorität, Status. |
| REQ-F-SRCH-009 | Mehrere Filter können kombiniert werden. | Must | V1 Core | Proposed | z. B. Provider + Topic + Status. |
| REQ-F-SRCH-010 | Häufige Filterkombinationen sollen als Smart View gespeichert werden können. | Should | V1.x | Proposed | View kann gespeichert und erneut geöffnet werden. |
| REQ-F-SRCH-011 | Das System soll eine View „Inbox“ bieten. | Must | V1 Core | Proposed | unklassifizierte Ressourcen sichtbar. |
| REQ-F-SRCH-012 | Das System soll eine View „Aktuell in Arbeit“ bieten. | Must | V1 Core | Proposed | gestartete Ressourcen/Paths sichtbar. |
| REQ-F-SRCH-013 | Das System soll „Wartet / Später“ darstellen können. | Should | V1 Core | Proposed | deferred Inhalte sichtbar. |
| REQ-F-SRCH-014 | Das System soll „Lernbedarf ohne Ressource“ darstellen können. | Should | V1.x | Proposed | offene Needs ohne Ressource auffindbar. |
| REQ-F-SRCH-015 | Das System soll „Ressourcen ohne Zuordnung“ darstellen können. | Should | V1 Core | Proposed | Datenpflege wird unterstützt. |
| REQ-F-SRCH-016 | Das System soll „Skills unter Zielniveau“ darstellen können. | Must | V1 Core | Proposed | Skill Gap View vorhanden. |
| REQ-F-SRCH-017 | Das System soll „abgeschlossene Ressourcen mit offenen Skills“ darstellen können. | Should | V1.x | Proposed | Completion/Mastery-Unterschied sichtbar. |
| REQ-F-SRCH-018 | Volltextsuche über lokal archivierte Fremdinhalte ist kein Muss für V1. | Won’t now | V1 | Proposed | nur eigene gespeicherte Metadaten/Notizen müssen durchsuchbar sein. |
| REQ-F-SRCH-019 | Semantische Suche darf später ergänzt werden. | Later | V2+ | Proposed | klassische Suche bleibt weiterhin nutzbar. |

---

## 10.15 Dashboard und Arbeitssteuerung

### Ziel

Der Benutzer soll beim Start nicht mit der gesamten Datenbank konfrontiert werden, sondern eine arbeitsfähige Übersicht erhalten.

| ID | Anforderung | Priorität | Zielhorizont | Status | Akzeptanzkriterium |
|---|---|---:|---|---|---|
| REQ-F-DASH-001 | Die Anwendung bietet eine persönliche Start-/Dashboardansicht. | Must | V1 Core | Proposed | Dashboard ist nach Start erreichbar. |
| REQ-F-DASH-002 | Das Dashboard zeigt aktive Lernziele. | Must | V1 Core | Proposed | aktive Ziele sind direkt sichtbar. |
| REQ-F-DASH-003 | Das Dashboard zeigt aktuell bearbeitete Learning Paths. | Must | V1 Core | Proposed | aktive Paths sichtbar. |
| REQ-F-DASH-004 | Das Dashboard zeigt aktuell gestartete Ressourcen. | Must | V1 Core | Proposed | Started-Inhalte sichtbar. |
| REQ-F-DASH-005 | Das Dashboard zeigt die Inbox-Anzahl. | Must | V1 Core | Proposed | neue Ressourcen fallen auf. |
| REQ-F-DASH-006 | Das Dashboard zeigt nächste Aktionen bzw. fällige Termine. | Should | V1 Core | Proposed | zeitnahe Punkte sichtbar. |
| REQ-F-DASH-007 | Das Dashboard kann wichtige Skill Gaps darstellen. | Should | V1 Core | Proposed | offene priorisierte Gaps sichtbar. |
| REQ-F-DASH-008 | Das Dashboard soll bewusst begrenzt bleiben und nicht jedes Reporting anzeigen. | Must | V1 Core | Proposed | Kernansicht bleibt auf Handlungsorientierung fokussiert. |
| REQ-F-DASH-009 | Spätere Widgets dürfen konfigurierbar sein. | Could | V2+ | Proposed | Personalisierung ist optional. |

---

## 10.16 Planung, Prioritäten und nächste Schritte

| ID | Anforderung | Priorität | Zielhorizont | Status | Akzeptanzkriterium |
|---|---|---:|---|---|---|
| REQ-F-PLAN-001 | Ziele, Paths und Ressourcen können priorisiert werden. | Must | V1 Core | Proposed | Priorität ist sichtbar. |
| REQ-F-PLAN-002 | Eine Ressource oder ein Path kann ein geplantes Startdatum besitzen. | Should | V1 Core | Proposed | Datum kann gepflegt werden. |
| REQ-F-PLAN-003 | Eine Ressource oder ein Path kann ein Ziel-/Fälligkeitsdatum besitzen. | Should | V1 Core | Proposed | Datum kann gepflegt werden. |
| REQ-F-PLAN-004 | Nächste Aktionen können erfasst werden. | Should | V1 Core | Proposed | kurzer nächster Schritt ist speicherbar. |
| REQ-F-PLAN-005 | Der Benutzer soll eine überschaubare „Als Nächstes“-Liste erzeugen können. | Must | V1 Core | Proposed | priorisierte offene Punkte sind sichtbar. |
| REQ-F-PLAN-006 | Der Benutzer kann Inhalte in Backlog/Später verschieben. | Must | V1 Core | Proposed | aktive Planung wird entlastet. |
| REQ-F-PLAN-007 | Die Anwendung darf später Vorschläge für nächste Lernschritte erzeugen. | Could | V2+ | Proposed | Vorschlag basiert nachvollziehbar auf Goal/Gap/Prerequisite/Priorität. |
| REQ-F-PLAN-008 | Vorschläge dürfen niemals ungefragt Planung oder Skill-Level ändern. | Must | V2+ | Proposed | Nutzerfreigabe erforderlich. |

---

## 10.17 Erinnerungen und Review-Termine

| ID | Anforderung | Priorität | Zielhorizont | Status | Akzeptanzkriterium |
|---|---|---:|---|---|---|
| REQ-F-NOT-001 | Wichtige Termine können innerhalb der Anwendung sichtbar gemacht werden. | Should | V1 Core | Proposed | Ziel-/Review-/Plantermine sind filterbar. |
| REQ-F-NOT-002 | Die Anwendung darf später lokale Erinnerungen unterstützen. | Could | V1.x | Proposed | Nutzer kann Erinnerung aktivieren/deaktivieren. |
| REQ-F-NOT-003 | Externe Kalenderintegration ist später möglich. | Later | V2+ | Proposed | Export/Schnittstelle wird nicht ausgeschlossen. |
| REQ-F-NOT-004 | Benachrichtigungen müssen abschaltbar sein. | Must | sobald Benachrichtigungen existieren | Proposed | Nutzer hat Kontrolle. |

---

## 10.18 Reporting und persönliche Lernhistorie

| ID | Anforderung | Priorität | Zielhorizont | Status | Akzeptanzkriterium |
|---|---|---:|---|---|---|
| REQ-F-RPT-001 | Die Anwendung zeigt eine chronologische Lernhistorie. | Should | V1.x | Proposed | Abschluss, Start, Evidence und Skill-Bewertungen sind zeitlich nachvollziehbar. |
| REQ-F-RPT-002 | Abgeschlossene Ressourcen können nach Zeitraum ausgewertet werden. | Should | V1.x | Proposed | Zeitraumfilter ist möglich. |
| REQ-F-RPT-003 | Fortschritt nach Ziel bzw. Learning Path kann zusammengefasst werden. | Should | V1 Core | Proposed | übersichtlicher Status ist verfügbar. |
| REQ-F-RPT-004 | Skill-Entwicklung kann historisch dargestellt werden. | Could | V1.x | Proposed | Level-Verlauf ist sichtbar. |
| REQ-F-RPT-005 | Lernzeit kann später ausgewertet werden. | Could | V1.x | Proposed | falls Zeit erfasst wird, ist Aggregation möglich. |
| REQ-F-RPT-006 | Bericht „Was habe ich in Zeitraum X gelernt?“ soll erzeugbar sein. | Could | V1.x | Proposed | Ressourcen, Evidence und Skills können zusammengefasst werden. |
| REQ-F-RPT-007 | Ein Export der Lernhistorie soll möglich sein. | Should | V1.x | Proposed | maschinen- oder menschenlesbarer Export. |
| REQ-F-RPT-008 | Gamification-Rankings sind kein Produktziel. | Won’t now | V1/V2 | Proposed | keine künstliche Wettbewerbspflicht. |

---

## 10.19 Datenpflege und Bibliotheksqualität

| ID | Anforderung | Priorität | Zielhorizont | Status | Akzeptanzkriterium |
|---|---|---:|---|---|---|
| REQ-F-MAINT-001 | Ressourcen ohne fachliche Zuordnung können gefunden werden. | Must | V1 Core | Proposed | Wartungsview vorhanden. |
| REQ-F-MAINT-002 | Ressourcen ohne Provider können gefunden werden. | Should | V1 Core | Proposed | Filter vorhanden. |
| REQ-F-MAINT-003 | Dubletten aufgrund identischer URL sollen erkennbar sein. | Should | V1 Core | Proposed | Warnung/Wartungsansicht. |
| REQ-F-MAINT-004 | Veraltete Ressourcen können markiert werden. | Should | V1.x | Proposed | Status/Flag vorhanden. |
| REQ-F-MAINT-005 | Ersetzte Ressourcen können mit Nachfolger verbunden werden. | Should | V1.x | Proposed | Relation sichtbar. |
| REQ-F-MAINT-006 | Broken-Link-Prüfung darf später automatisiert werden. | Could | V2+ | Proposed | manueller URL-Bestand bleibt unabhängig. |
| REQ-F-MAINT-007 | Webarchivierung darf später optional ergänzt werden. | Later | V2+ | Proposed | kein V1-Zwang. |
| REQ-F-MAINT-008 | Archivierte Datensätze bleiben in historischen Beziehungen erhalten. | Must | V1 Core | Proposed | keine Historienbrüche. |
| REQ-F-MAINT-009 | Dauerhaftes Löschen muss bewusst von Archivieren unterschieden werden. | Must | V1 Core | Proposed | Nutzer erkennt Unterschied und wird vor Datenverlust geschützt. |

---

## 10.20 Import, Export, Backup und Portabilität

### Ziel

Der Learning Manager soll nicht selbst zu einem Daten-Lock-in werden.

| ID | Anforderung | Priorität | Zielhorizont | Status | Akzeptanzkriterium |
|---|---|---:|---|---|---|
| REQ-F-IO-001 | Kerndaten können vollständig gesichert werden. | Must | V1 Core | Proposed | Backup enthält alle fachlichen Kerndaten. |
| REQ-F-IO-002 | Ein Backup kann wiederhergestellt werden. | Must | V1 Core | Proposed | dokumentierter Restore-Test ist möglich. |
| REQ-F-IO-003 | Ressourcen können in einem offenen bzw. dokumentierten Format exportiert werden. | Must | V1.x | Proposed | z. B. CSV/JSON oder vergleichbar. |
| REQ-F-IO-004 | Knowledge Artifacts sollen in ein menschenlesbares Format exportierbar sein. | Must | V1.x | Proposed | Markdown/Text ist möglich. |
| REQ-F-IO-005 | Ein grundlegender Ressourcenimport soll später verfügbar sein. | Should | V1.x | Proposed | mindestens CSV oder dokumentiertes Austauschformat. |
| REQ-F-IO-006 | Import soll vorhandene Dubletten berücksichtigen können. | Should | V1.x | Proposed | Nutzer kann Umgang mit Dubletten nachvollziehen. |
| REQ-F-IO-007 | Import aus Browser-Bookmarks darf unterstützt werden. | Could | V2+ | Proposed | Mapping auf Resource/Inbox möglich. |
| REQ-F-IO-008 | Import aus Raindrop/Karakeep/Zotero o. ä. darf später über Mapping erfolgen. | Later | V2+ | Proposed | keine harte Abhängigkeit. |
| REQ-F-IO-009 | Export muss Beziehungen zwischen Ressourcen und fachlichen Objekten soweit sinnvoll erhalten. | Must | V1.x | Proposed | Exportdokumentation beschreibt Relationen. |
| REQ-F-IO-010 | Ein Export darf persönliche Daten nicht ungefragt an externe Dienste übertragen. | Must | V1.x | Proposed | Export bleibt nutzergesteuert. |

---

## 10.21 Externe Links und Integrationen

| ID | Anforderung | Priorität | Zielhorizont | Status | Akzeptanzkriterium |
|---|---|---:|---|---|---|
| REQ-F-INT-001 | Externe Lernressourcen können in ihrem Ursprungssystem geöffnet werden. | Must | V1 Core | Proposed | URL-Öffnen funktioniert. |
| REQ-F-INT-002 | Die Anwendung benötigt für den Grundbetrieb keine Zugangsdaten externer Lernplattformen. | Must | V1 Core | Proposed | O’Reilly/LinkedIn/YouTube funktionieren als normale Resource Links. |
| REQ-F-INT-003 | Provider-spezifische API-Integrationen dürfen später ergänzt werden. | Later | V2+ | Proposed | Kernmodell bleibt providerneutral. |
| REQ-F-INT-004 | Eine Integration darf keine Umgehung von DRM, Zugriffsbeschränkungen oder Nutzungsbedingungen voraussetzen. | Must | alle Versionen | Proposed | Integration verwendet zulässige Schnittstellen. |
| REQ-F-INT-005 | Browser-Capture darf später über Erweiterung/API erfolgen. | Later | V2+ | Proposed | kein V1-Blocker. |
| REQ-F-INT-006 | Obsidian-/Markdown-Workflow soll später unterstützt werden können. | Could | V2+ | Proposed | Knowledge Export ist Grundlage. |
| REQ-F-INT-007 | Anki-/RemNote-Export darf später ergänzt werden. | Later | V2+ | Proposed | Retention-Modul bleibt modular. |
| REQ-F-INT-008 | Kalenderintegration darf später ergänzt werden. | Later | V2+ | Proposed | Termine bleiben intern unabhängig. |

---

## 10.22 AI-gestützte Funktionen

### Grundsatz

AI ist **kein notwendiger Bestandteil des fachlichen Kerns**. Der Learning Manager muss ohne AI vollständig nutzbar bleiben.

| ID | Anforderung | Priorität | Zielhorizont | Status | Akzeptanzkriterium |
|---|---|---:|---|---|---|
| REQ-F-AI-001 | Der Kernworkflow muss ohne AI funktionieren. | Must | V1 Core | Proposed | alle Kernoperationen sind manuell möglich. |
| REQ-F-AI-002 | AI darf später Tags vorschlagen. | Later | V2+ | Proposed | Vorschlag wird gekennzeichnet und muss bestätigbar sein. |
| REQ-F-AI-003 | AI darf später Skills/Topics zu Ressourcen vorschlagen. | Later | V2+ | Proposed | keine automatische ungeprüfte Zuordnung. |
| REQ-F-AI-004 | AI darf später Ressourcen zusammenfassen. | Later | V2+ | Proposed | Quelle bleibt verlinkt. |
| REQ-F-AI-005 | AI darf später Überschneidungen zwischen Ressourcen vorschlagen. | Later | V2+ | Proposed | Unsicherheit wird transparent gemacht. |
| REQ-F-AI-006 | AI darf später mögliche nächste Lernschritte empfehlen. | Later | V2+ | Proposed | Empfehlung basiert auf nachvollziehbaren Daten. |
| REQ-F-AI-007 | AI darf kein Skill-Level ohne Nutzerfreigabe setzen. | Must | V2+ | Proposed | keine autonome Kompetenzbewertung. |
| REQ-F-AI-008 | AI-generierte Informationen müssen als solche erkennbar sein. | Must | V2+ | Proposed | Herkunft/Status ist sichtbar. |
| REQ-F-AI-009 | Die Nutzung externer AI-Dienste muss optional sein. | Must | V2+ | Proposed | Anwendung bleibt ohne externen AI-Dienst nutzbar. |
| REQ-F-AI-010 | Lokale AI-Modelle dürfen langfristig unterstützt werden. | Could | Future | Proposed | Architektur schließt Providerwechsel nicht aus. |

---

## 10.23 Kollaboration und Mehrbenutzerbetrieb

Die erste Produktgeneration ist auf persönliche Weiterbildung ausgerichtet.

| ID | Anforderung | Priorität | Zielhorizont | Status | Akzeptanzkriterium |
|---|---|---:|---|---|---|
| REQ-F-COL-001 | V1 benötigt keinen Mehrbenutzerbetrieb. | Won’t now | V1 | Proposed | Single-user-Nutzung ist vollständig. |
| REQ-F-COL-002 | Teilen von Learning-Path-Vorlagen darf später möglich sein. | Could | V2+ | Proposed | persönliche Historie wird nicht mitgeteilt, sofern nicht gewollt. |
| REQ-F-COL-003 | Gemeinsame Team-Roadmaps dürfen später ergänzt werden. | Later | V2+ | Proposed | nicht Teil des persönlichen Kernmodells. |
| REQ-F-COL-004 | HR-/Manager-Bewertungen sind kein initiales Produktziel. | Won’t now | V1/V2 | Proposed | keine Unternehmenshierarchie erforderlich. |

---

# 11. Zentrale Use Cases

## UC-01 – Neue Ressource schnell speichern

**Auslöser:** Der Benutzer findet während der Arbeit ein interessantes YouTube-Video.

**Ablauf:**

1. Benutzer öffnet Quick Capture.
2. Benutzer fügt URL ein.
3. System prüft auf identische URL.
4. Wenn keine Dublette existiert, wird die Ressource minimal gespeichert.
5. Ressource erscheint in der Inbox.
6. Benutzer kann die Klassifikation später durchführen.

**Erfolg:** Die Ressource ist in wenigen Schritten gesichert, ohne den aktuellen Arbeitsfluss zu unterbrechen.

---

## UC-02 – Ressource klassifizieren

1. Benutzer öffnet Inbox.
2. Benutzer wählt eine Ressource.
3. Provider und Ressourcentyp werden gepflegt.
4. Skill/Topic wird zugeordnet.
5. Ressource wird optional einem Learning Path zugeordnet.
6. Priorität und Status werden gesetzt.
7. Ressource verlässt die Inbox.

---

## UC-03 – Learning Path für Cyber Security erstellen

1. Benutzer legt Ziel „Blue-Team-Kompetenz ausbauen“ an.
2. Benutzer legt bzw. wählt Kompetenzbereich Cyber Security.
3. Benutzer erstellt einen Learning Path.
4. Path wird in Module gegliedert:
   - Grundlagen
   - Network Security
   - SIEM
   - Detection
   - Incident Response
   - Forensics
5. Skills werden Nodes zugeordnet.
6. vorhandene Ressourcen werden an Nodes gehängt.
7. neue Learning Needs werden sichtbar, wenn Bereiche noch keine geeignete Ressource besitzen.
8. Prioritäten werden gesetzt.

---

## UC-04 – Kurs mehrfach verwenden

1. Eine Ressource „Linux Performance Optimization“ existiert bereits.
2. Benutzer öffnet Learning Path „Linux Performance“.
3. Benutzer weist die bestehende Ressource einem Node zu.
4. Benutzer öffnet Learning Path „EX442 Vorbereitung“.
5. Dieselbe Ressource wird dort ebenfalls zugeordnet.
6. System erzeugt keinen zweiten Ressourcendatensatz.

---

## UC-05 – Skill Gap erkennen

1. Zielrolle fordert Skill-Level 4 für einen Skill.
2. Aktuelle Selbsteinschätzung ist Level 2.
3. System zeigt Gap 2 → 4.
4. Benutzer sieht zugeordnete Ressourcen und Evidence.
5. Benutzer priorisiert einen Path zur Schließung der Lücke.

---

## UC-06 – Ressource abschließen, Skill aber noch nicht beherrschen

1. Kursfortschritt wird auf 100 % gesetzt.
2. Ressource erhält Completion-Datum.
3. Skill-Level bleibt unverändert.
4. Benutzer dokumentiert, dass weitere Praxis nötig ist.
5. Ein Lab wird als nächste Aktion geplant.

---

## UC-07 – Evidence durch eigenes Projekt

1. Benutzer entwickelt ein eigenes Testprojekt.
2. Evidence „Project“ wird angelegt.
3. Evidence wird mehreren Skills zugeordnet.
4. optional wird Repository/Datei referenziert.
5. Benutzer aktualisiert Skill-Level mit Begründung unter Bezug auf die Evidence.

---

## UC-08 – Lernen später nachvollziehen

1. Benutzer öffnet einen Skill nach mehreren Monaten.
2. System zeigt:
   - aktuelle Einschätzung,
   - frühere Einschätzungen,
   - absolvierte Ressourcen,
   - Evidence,
   - zuletzt genutztes Datum,
   - zugehörige Knowledge Artifacts.
3. Benutzer entscheidet, ob eine Auffrischung nötig ist.

---

## UC-09 – Ähnliche Ressourcen vergleichen

1. Benutzer betrachtet zwei EX442-Kurse.
2. System zeigt die vorhandenen Metadaten beider Ressourcen.
3. Ressourcen können mit Relation „overlaps with“ verbunden werden.
4. Benutzer dokumentiert im persönlichen Kommentar, welche Ressource bevorzugt wird.
5. überflüssige Ressource kann auf Deferred/Archived gesetzt werden.

AI-basierte automatische Überlappungsanalyse ist dafür nicht erforderlich.

---

## UC-10 – Nächsten sinnvollen Lernschritt finden

1. Benutzer öffnet Dashboard.
2. System zeigt aktive Ziele, aktuelle Paths und Skill Gaps.
3. Benutzer sieht priorisierte nächste Aktionen.
4. Benutzer startet eine Ressource oder ein Lab.

---

# 12. Fachliche Geschäftsregeln

## BR-001 – Kanonische Ressourcen

Ein realweltliches Lernangebot soll grundsätzlich nur einen aktiven Ressourcendatensatz besitzen. Mehrfachverwendung wird über Beziehungen abgebildet.

## BR-002 – Archivieren vor Löschen

Wenn ein Datensatz bereits Lernhistorie oder Beziehungen besitzt, soll Archivieren gegenüber endgültigem Löschen bevorzugt werden.

## BR-003 – Completion und Mastery getrennt

Resource Completion darf niemals allein den Skill-Mastery-Wert bestimmen.

## BR-004 – Evidence ist nachvollziehbar

Evidence soll mit Art, Datum und Bezug nachvollziehbar sein.

## BR-005 – Providerneutralität

Lernpfade und Skills gehören dem Benutzer, nicht einem Lernanbieter.

## BR-006 – Inbox darf unvollständig sein

Inbox-Einträge benötigen weniger Pflichtmetadaten als vollständig klassifizierte Ressourcen.

## BR-007 – Beziehung statt Duplikation

Wo dasselbe fachliche Objekt in mehreren Kontexten verwendet wird, soll eine Relation statt einer Kopie verwendet werden.

## BR-008 – Nutzerentscheidung bei Automatisierung

Automatische Vorschläge dürfen fachliche Daten nicht stillschweigend ändern.

## BR-009 – Historie erhalten

Abschluss, Evidence und wichtige Skillbewertungen sollen bei Archivierung erhalten bleiben.

## BR-010 – Learning Path ist fachliche Struktur

Ein Learning Path kann auch Skills, praktische Aufgaben oder Checkpoints enthalten, für die keine externe Ressource existiert.

---

# 13. Fachliches Datenobjektmodell

Dieses Kapitel beschreibt kein technisches Datenbankschema, sondern die wesentlichen fachlichen Objekte.

## 13.1 Kernobjekte

### Goal

- Titel
- Beschreibung
- Typ
- Motivation
- Priorität
- Status
- Zieltermin
- angelegte/erreichte Zeitpunkte
- verbundene Skills
- verbundene Learning Paths

### Competency Area

- Name
- Beschreibung
- Status
- Topics/Skills

### Topic

- Name
- Beschreibung
- Beziehungen
- Kompetenzbereiche
- Skills

### Skill

- Name
- Beschreibung
- Alias
- aktuelles Level
- Ziel-Level
- Levelhistorie
- Evidence
- Topics
- Ressourcen
- Learning Paths
- Review-/Recency-Daten

### Learning Need

- Titel
- Beschreibung
- Priorität
- Status
- Ziel/Skill/Topic
- zugeordnete Ressourcen/Paths

### Learning Path

- Titel
- Beschreibung
- Status
- Priorität
- Ziel
- Nodes
- Fortschritt
- Termine

### Learning Path Node

- Titel
- Beschreibung
- Typ
- Hierarchie
- Reihenfolge
- Pflicht/Optional
- Skills
- Ressourcen
- Voraussetzungen
- Alternativen
- Status

### Resource

- Titel
- Typ
- Provider
- URL
- lokaler Verweis
- Beschreibung
- persönliche Begründung
- Autor/Trainer
- Sprache
- Datum/Version
- Dauer/Aufwand
- Schwierigkeit
- Priorität
- Status
- Fortschritt
- Tags
- Skills
- Topics
- Learning-Path-Zuordnungen
- Relationen zu anderen Ressourcen

### Provider

- Name
- Website
- Beschreibung
- Typ
- Status

### Learning Activity / Session

- Ressource/Path
- Datum
- Dauer
- Fortschrittsänderung
- Notiz

### Knowledge Artifact

- Titel
- Typ
- Inhalt
- Ressourcenbezug
- Skillbezug
- Tags
- Erstell-/Änderungsdatum

### Evidence

- Titel
- Typ
- Beschreibung
- Datum
- externer/lokaler Nachweis
- Skills
- Ressource
- Ziel

### Skill Assessment

- Skill
- Level
- Datum
- Bewertungsart
- Begründung
- Evidence

### Tag

- Name
- optionale Beschreibung

---

# 14. Nichtfunktionale Anforderungen

---

## 14.1 Bedienbarkeit und UX

| ID | Anforderung | Priorität | Akzeptanzkriterium |
|---|---|---:|---|
| REQ-Q-UX-001 | Häufige Kernaktionen sollen mit geringer Interaktion erreichbar sein. | Must | Quick Capture und Statusänderungen erfordern keine langen Dialogketten. |
| REQ-Q-UX-002 | Die Anwendung soll auch mit mehreren hundert bzw. tausend Ressourcen übersichtlich bleiben. | Must | Suche, Filter und strukturierte Views bleiben nutzbar. |
| REQ-Q-UX-003 | Fachbegriffe müssen konsistent verwendet werden. | Must | Goal, Skill, Resource, Path etc. haben eindeutige UI-Bedeutung. |
| REQ-Q-UX-004 | Fortgeschrittene Funktionen dürfen einfache Nutzung nicht blockieren. | Must | Resource kann ohne Skill-Assessment gespeichert werden. |
| REQ-Q-UX-005 | destructive Aktionen müssen klar erkennbar sein. | Must | endgültiges Löschen ist von Archivieren unterscheidbar. |
| REQ-Q-UX-006 | Leere Zustände sollen verständlich erklären, was als Nächstes möglich ist. | Should | z. B. leere Inbox oder erster Path. |
| REQ-Q-UX-007 | Die Anwendung soll Tastaturbedienung für häufige Aktionen unterstützen. | Should | zentrale Workflows sind ohne Maus weitgehend bedienbar. |
| REQ-Q-UX-008 | Die Oberfläche soll bei üblichen Skalierungs-/DPI-Einstellungen nutzbar bleiben. | Must | keine abgeschnittenen Kernfunktionen bei unterstützten Einstellungen. |
| REQ-Q-UX-009 | Farbkennzeichnung darf nicht die einzige Informationsquelle sein. | Must | Status ist zusätzlich textuell/symbolisch erkennbar. |

---

## 14.2 Barrierefreiheit

| ID | Anforderung | Priorität | Akzeptanzkriterium |
|---|---|---:|---|
| REQ-Q-A11Y-001 | Interaktive Elemente sollen sinnvolle Beschriftungen besitzen. | Should | Screenreader-/Accessibility-Metadaten soweit Plattform unterstützt. |
| REQ-Q-A11Y-002 | Ausreichende Kontraste sollen eingehalten werden. | Should | gewähltes Design erfüllt gebräuchliche Accessibility-Grundsätze. |
| REQ-Q-A11Y-003 | Fokusreihenfolge soll nachvollziehbar sein. | Should | Tastaturnavigation folgt logischer Reihenfolge. |
| REQ-Q-A11Y-004 | Textgröße bzw. Systemskalierung soll berücksichtigt werden. | Must | Kernansichten bleiben bedienbar. |

---

## 14.3 Performance

Die Anwendung ist zunächst kein Hochlast-Enterprise-System. Trotzdem muss eine private langfristige Bibliothek performant bleiben.

| ID | Anforderung | Priorität | Akzeptanzkriterium |
|---|---|---:|---|
| REQ-Q-PERF-001 | Typische Listenansichten sollen bei normalem lokalen Datenbestand ohne störende Verzögerung erscheinen. | Must | subjektiv flüssige Nutzung; konkrete Messwerte im Pflichtenheft. |
| REQ-Q-PERF-002 | Suche über typische persönliche Datenbestände soll interaktiv nutzbar sein. | Must | Ergebnisse erscheinen in angemessener Zeit. |
| REQ-Q-PERF-003 | Der Produktkern darf für normale Nutzung keine permanente Netzwerkverbindung benötigen. | Must | Ziele, Skills, Paths, Ressourcen und Notizen funktionieren offline. |
| REQ-Q-PERF-004 | Externe Metadatenabfragen dürfen die lokale Bedienung nicht blockieren. | Should | fehlende Netzwerkantwort verhindert manuelles Speichern nicht. |

---

## 14.4 Zuverlässigkeit und Datenintegrität

| ID | Anforderung | Priorität | Akzeptanzkriterium |
|---|---|---:|---|
| REQ-Q-REL-001 | Benutzerdaten dürfen bei normalen Programmabbrüchen nicht leicht verloren gehen. | Must | persistierte Transaktionen bleiben konsistent. |
| REQ-Q-REL-002 | Beziehungen dürfen nicht stillschweigend auf nicht mehr existierende fachliche Objekte zeigen. | Must | referentielle Konsistenz oder äquivalente Prüfung. |
| REQ-Q-REL-003 | Backup und Restore müssen testbar sein. | Must | dokumentierter Restore-Test. |
| REQ-Q-REL-004 | Migrationen zwischen Versionen müssen Benutzerdaten erhalten. | Must | Upgrade-Test mit repräsentativem Datenbestand. |
| REQ-Q-REL-005 | Fehler beim Abruf externer Metadaten dürfen vorhandene Daten nicht beschädigen. | Must | Offline-/Fehlerszenario getestet. |
| REQ-Q-REL-006 | Archivierung darf Beziehungen und Historie nicht unbeabsichtigt entfernen. | Must | Regressionstest. |

---

## 14.5 Datenschutz und Privatsphäre

Der Learning Manager verarbeitet persönliche Lern-, Leistungs- und ggf. berufliche Profildaten.

| ID | Anforderung | Priorität | Akzeptanzkriterium |
|---|---|---:|---|
| REQ-SEC-PRIV-001 | Kernfunktionalität darf keinen verpflichtenden Cloud-Account voraussetzen. | Must | lokale Nutzung ist möglich. |
| REQ-SEC-PRIV-002 | Persönliche Daten dürfen nicht ohne ausdrückliche Nutzeraktion an externe Dienste übertragen werden. | Must | Netzwerkschnittstellen sind dokumentiert und nutzergesteuert. |
| REQ-SEC-PRIV-003 | AI-Integration muss transparent machen, welche Daten an einen externen Dienst übermittelt werden. | Must | sobald AI existiert, klare Offenlegung/Opt-in. |
| REQ-SEC-PRIV-004 | Telemetrie darf nicht stillschweigend persönliche Lerninhalte übertragen. | Must | Telemetrie-Design dokumentiert; sensible Inhalte ausgeschlossen bzw. opt-in. |
| REQ-SEC-PRIV-005 | Export und Backup bleiben unter Kontrolle des Benutzers. | Must | keine automatische Cloudübertragung ohne Konfiguration. |
| REQ-SEC-PRIV-006 | Löschung persönlicher Daten soll möglich sein. | Must | Benutzer kann Daten endgültig entfernen, sofern keine gewollte Historienbindung verbleibt. |

---

## 14.6 Sicherheit

| ID | Anforderung | Priorität | Akzeptanzkriterium |
|---|---|---:|---|
| REQ-SEC-001 | Externe URLs müssen als potenziell nicht vertrauenswürdig behandelt werden. | Must | Öffnen/Rendering führt nicht zu ungeprüfter Codeausführung. |
| REQ-SEC-002 | Importierte Dateien/Metadaten dürfen nicht ungeprüft ausführbaren Code starten. | Must | Import bleibt Datenoperation. |
| REQ-SEC-003 | Zugangsdaten externer Plattformen sollen in V1 nicht benötigt werden. | Must | kein Passwortspeicher für O’Reilly/LinkedIn etc. |
| REQ-SEC-004 | Falls später Tokens oder Secrets benötigt werden, müssen diese angemessen geschützt werden. | Must | Security Design vor Einführung. |
| REQ-SEC-005 | Backup-Dateien können sensible Informationen enthalten und müssen entsprechend behandelt werden. | Must | Dokumentation weist darauf hin; Schutzmechanismus im Pflichtenheft bewerten. |
| REQ-SEC-006 | Abhängigkeiten und Integrationen sollen nach SASD Development Standard gepflegt und bewertet werden. | Must | nachvollziehbare Dependency-/Security-Praxis. |

---

## 14.7 Portabilität und offene Daten

| ID | Anforderung | Priorität | Akzeptanzkriterium |
|---|---|---:|---|
| REQ-Q-PORT-001 | Der Benutzer soll nicht an ein proprietäres undokumentiertes Exportformat gebunden sein. | Must | mindestens ein dokumentiertes offenes Exportformat. |
| REQ-Q-PORT-002 | Zentrale Textinhalte sollen menschenlesbar exportierbar sein. | Must | Markdown/Text/CSV/JSON je Inhalt. |
| REQ-Q-PORT-003 | Eindeutige interne IDs sollen Beziehungen beim Export nachvollziehbar machen können. | Should | Export enthält stabile Referenzen. |
| REQ-Q-PORT-004 | Die Anwendung soll bei einer späteren technischen Neuentwicklung eine Datenmigration ermöglichen. | Must | Domain-Daten sind nicht untrennbar an UI gebunden. |

---

## 14.8 Wartbarkeit und Nachvollziehbarkeit

| ID | Anforderung | Priorität | Akzeptanzkriterium |
|---|---|---:|---|
| REQ-Q-MAINT-001 | Implementierung und Dokumentation sollen dem SASD Development Standard folgen, soweit für die Projektklassifikation anwendbar. | Must | Compliance-/Projektartefakte vorhanden. |
| REQ-Q-MAINT-002 | Fachliche Kernregeln sollen testbar sein. | Must | automatisierte Tests für zentrale Domain Rules. |
| REQ-Q-MAINT-003 | Das System soll modular genug sein, spätere Integrationen ohne Umbau des gesamten Kernmodells zu ermöglichen. | Should | Integrationen sind nicht im Domain-Kern fest verdrahtet. |
| REQ-Q-MAINT-004 | Anforderungen müssen zu Implementierung und Tests rückverfolgbar sein können. | Should | Requirement IDs werden in Test-/Issue-/Dokumentationsartefakten referenziert. |
| REQ-Q-MAINT-005 | Wichtige Architekturentscheidungen sollen dokumentiert werden. | Must | ADRs oder äquivalente SASD-Artefakte. |

---

# 15. Betriebliche Anforderungen

| ID | Anforderung | Priorität | Status | Akzeptanzkriterium |
|---|---|---:|---|---|
| REQ-OPS-001 | Der persönliche Kernbetrieb soll ohne permanent erreichbaren Server möglich sein. | Must | Proposed | Kernfunktionen sind lokal nutzbar. |
| REQ-OPS-002 | Installation und Update sollen für einen Einzelbenutzer handhabbar sein. | Must | Proposed | dokumentierter Installations-/Updateweg. |
| REQ-OPS-003 | Anwendung und Daten sollen getrennt sicherbar sein können, soweit technisch sinnvoll. | Should | Proposed | Backupkonzept dokumentiert. |
| REQ-OPS-004 | Fehlersuche soll durch nachvollziehbare Logs unterstützt werden. | Must | Proposed | relevante Fehler werden protokolliert, sensible Inhalte angemessen behandelt. |
| REQ-OPS-005 | Datenbank-/Datenschemaänderungen sollen versioniert und reproduzierbar sein. | Must | Proposed | Migrationen nachvollziehbar. |
| REQ-OPS-006 | Ein Upgrade darf nicht voraussetzen, dass der Benutzer seine Lernhistorie neu anlegt. | Must | Proposed | Migrationspfad vorhanden. |
| REQ-OPS-007 | Deinstallation darf persönliche Daten nicht ohne deutliche Nutzerentscheidung zerstören. | Should | Proposed | Verhalten ist dokumentiert/abgesichert. |

---

# 16. Schnittstellenanforderungen

## 16.1 Benutzeroberfläche

Die UI muss mindestens fachliche Zugänge bieten zu:

- Dashboard,
- Goals,
- Skills/Competencies,
- Learning Needs,
- Learning Paths,
- Resources,
- Inbox,
- Knowledge,
- Evidence,
- Suche/Filter,
- Datenpflege,
- Import/Export/Backup,
- Einstellungen.

Die konkrete Navigationsform ist im Pflichtenheft zu bestimmen.

## 16.2 Dateisystem

Die Anwendung soll lokale Dateien referenzieren können. Ob Dateien in die Anwendung kopiert oder nur verlinkt werden, ist eine spätere technische Entscheidung.

## 16.3 Web

URLs müssen gespeichert und über das Betriebssystem bzw. einen geeigneten sicheren Mechanismus geöffnet werden können.

## 16.4 Import/Export

Ein dokumentiertes Austauschformat ist erforderlich. Konkrete Formate und Versionierung werden im Pflichtenheft festgelegt.

## 16.5 APIs

V1 benötigt keine öffentliche API. Das Domain Model soll eine spätere API jedoch nicht unnötig erschweren.

---

# 17. Abgrenzung nach Releases

## 17.1 V1 Core – bewusst kleiner, vollständig nutzbarer Produktkern

V1 Core soll mindestens ermöglichen:

1. Lernziel anlegen.
2. Kompetenzbereiche/Skills verwalten.
3. Ist- und Ziel-Level dokumentieren.
4. Learning Path strukturieren.
5. Ressourcen anbieterunabhängig verwalten.
6. Ressource mehreren Skills/Paths zuordnen.
7. Quick Capture und Inbox.
8. Ressourcenstatus und Fortschritt.
9. Priorität und nächste Aktion.
10. Notizen.
11. einfache Evidence.
12. Completion und Mastery getrennt behandeln.
13. Dashboard.
14. Suche und Filter.
15. Datenpflege/Archivierung.
16. Backup und Restore.
17. grundlegende Datenportabilität vorbereiten.

### V1-Core-Erfolgskriterium

Der folgende Workflow muss ohne externe Spezialintegration funktionieren:

```text
Goal
  ↓
Skill
  ↓
Learning Path
  ↓
Resource
  ↓
Progress
  ↓
Note / Evidence
  ↓
Skill Assessment
```

---

## 17.2 V1.x – direkte Vertiefung

Nach einem stabilen Kern sind besonders wertvoll:

- Saved Searches / Smart Views,
- Learning Needs,
- Skill-Level-Historie,
- Ressourcenrelationen,
- Path-Voraussetzungen und Alternativen,
- Lernzeit/Sessions,
- Timeline/Lernhistorie,
- einfache Retention-/Review-Termine,
- Import/Export,
- automatische Basis-Metadaten,
- weitere Datenpflege,
- lokale Erinnerungen,
- Markdown-/Obsidian-freundlicher Export,
- persönliche Reports.

---

## 17.3 V2+ – strategische Erweiterungen

- Browser Extension,
- Provider APIs,
- fortgeschrittene Visual Roadmaps,
- Webarchivierung,
- Broken-Link-Monitoring,
- semantische Suche,
- AI-Klassifikation,
- AI-Zusammenfassung,
- AI-Ressourcenvergleich,
- AI-Lernvorschläge,
- Anki-/RemNote-Integration,
- Kalenderintegration,
- Assessment-Engine,
- Vorlagenbibliothek,
- Sharing,
- optionale Teamfunktionen.

---

# 18. Explizit aus V1 ausgeschlossene Funktionen

Zur Vermeidung von Scope Creep gehören folgende Funktionen **nicht** in V1:

- eigener Video-Player,
- Video-Hosting,
- vollständiger PDF-Reader,
- EPUB-Reader,
- Web-Clipping-Browserextension,
- Website-Snapshot-/Archivierungsengine,
- AI-Chatbot,
- automatische LLM-Tags,
- automatische AI-Skill-Bewertung,
- semantische Vektorsuche,
- vollwertiges Spaced Repetition,
- Flashcard-Authoring,
- Teamverwaltung,
- Unternehmenshierarchie,
- HR-Workflows,
- SCORM-/xAPI-LMS-Authoring,
- öffentliche Community,
- Kursbewertungen anderer Nutzer,
- Kursmarktplatz,
- Zertifikatsprüfung durch Drittanbieter,
- Cloudpflicht,
- Social Features,
- Gamification-Rankings.

Diese Abgrenzung ist bewusst. Sie soll verhindern, dass ein persönlicher Learning Manager vor dem ersten Nutzwert zu einem komplexen LMS/PKM/Reader-Hybrid anwächst.

---

# 19. Produktweite Akzeptanzkriterien

Das Produktkonzept gilt fachlich als erfolgreich umgesetzt, wenn mindestens folgende Szenarien belastbar funktionieren:

## AK-01 – Anbieterübergreifende Lernplanung

Ein Learning Path enthält gleichzeitig beispielsweise:

- O’Reilly-Kurs,
- YouTube-Video,
- LinkedIn-Learning-Kurs,
- Dokumentation,
- eigenes Lab.

Alle werden im selben fachlichen Path verwaltet.

## AK-02 – Canonical Resource

Dieselbe Ressource kann in mindestens zwei Paths und mehreren Skills vorkommen, ohne dass Titel, URL und Status mehrfach gepflegt werden müssen.

## AK-03 – Skill Gap

Für einen Skill können Ist- und Ziel-Level gepflegt werden. Das System zeigt die Lücke nachvollziehbar.

## AK-04 – Completion ≠ Mastery

Eine Ressource wird auf 100 % gesetzt, ohne dass sich das Skill-Level automatisch auf „beherrscht“ ändert.

## AK-05 – Inbox

Eine URL kann schnell gespeichert werden, obwohl Provider, Tags und Skill noch nicht feststehen. Der Eintrag ist später auffindbar und klassifizierbar.

## AK-06 – Evidence

Ein abgeschlossenes Lab kann als Evidence gespeichert und einem Skill zugeordnet werden.

## AK-07 – Historie

Archivieren eines Paths oder einer Ressource löscht die zugehörige Lernhistorie nicht.

## AK-08 – Wiederfinden

Bei einem Datenbestand mit vielen Ressourcen kann der Benutzer durch Suche/Filter gezielt z. B. alle noch nicht begonnenen Security-Ressourcen eines Providers finden.

## AK-09 – Backup

Ein vollständiges Backup kann erstellt und in einer frischen bzw. definierten Testinstallation wiederhergestellt werden.

## AK-10 – Offline-Kern

Ohne Internetzugang können vorhandene Ziele, Skills, Paths, Ressourcenmetadaten, Notizen und Evidence weiterhin verwaltet werden.

---

# 20. Qualitätskriterien für die erste produktiv nutzbare Version

Eine V1 soll nicht allein daran gemessen werden, ob alle Menüpunkte existieren. Sie soll im Alltag folgende Eigenschaften besitzen:

1. **Verständlich:** Ein neuer Lernpfad ist ohne Studium einer umfangreichen Anleitung anlegbar.
2. **Schnell:** Eine URL kann in kurzer Zeit in die Inbox übernommen werden.
3. **Fokussiert:** Das Dashboard beantwortet „Woran arbeite ich gerade?“.
4. **Nachvollziehbar:** Bei einer Ressource ist sichtbar, warum sie existiert und wo sie verwendet wird.
5. **Konsistent:** Completion, Mastery und Retention werden nicht vermischt.
6. **Robust:** Daten gehen bei üblichen Fehlern nicht leicht verloren.
7. **Portabel:** Backup und Export sind vorgesehen.
8. **Privat:** Kernfunktion benötigt keine verpflichtende externe Cloud.
9. **Erweiterbar:** AI und Integrationen können später ergänzt werden.
10. **Nicht überladen:** Der Nutzer muss keine Enterprise-LMS-Komplexität bedienen.

---

# 21. Risiken und Gegenmaßnahmen

## R-01 – Scope Creep

**Risiko:** Die große Vorlagenliste verführt dazu, gleichzeitig LMS, PKM, Bookmark Manager, Reader und Anki nachzubauen.

**Gegenmaßnahme:** V1-Core-Abgrenzung verbindlich behandeln; neue Funktionen gegen den Kernworkflow prüfen.

## R-02 – Verwaltungsaufwand übersteigt Lernnutzen

**Risiko:** Zu viele Felder und Bewertungen führen dazu, dass der Benutzer keine Ressourcen mehr einträgt.

**Gegenmaßnahme:** Quick Capture, wenige Pflichtfelder, progressive Vertiefung.

## R-03 – Skill-Modell wird zu abstrakt

**Risiko:** Aufwendige Taxonomien verhindern praktische Nutzung.

**Gegenmaßnahme:** Freie eigene Skills zuerst; formale Ontologien erst später.

## R-04 – Fortschritt wird zur Scheingenauigkeit

**Risiko:** Prozentwerte vermitteln eine Genauigkeit, die beim Lernen nicht existiert.

**Gegenmaßnahme:** Fortschritt als Ressourcenzustand; Mastery getrennt und mit qualitativer Evidenz.

## R-05 – AI erhält zu viel Autorität

**Risiko:** Falsche Klassifikationen oder Kompetenzbewertungen werden als Fakten gespeichert.

**Gegenmaßnahme:** AI nur optional, transparent und bestätigungspflichtig.

## R-06 – Daten-Lock-in

**Risiko:** Nach Jahren liegt die komplette Lernhistorie in einem schwer exportierbaren Format.

**Gegenmaßnahme:** offene Exporte, dokumentiertes Datenmodell, Backup/Restore.

## R-07 – Externe Plattformen ändern URLs/APIs

**Risiko:** Links oder Integrationen funktionieren später nicht mehr.

**Gegenmaßnahme:** Providerneutraler Kern, keine Pflicht-API, spätere Linkprüfung.

## R-08 – Zu viele parallele Lernpfade

**Risiko:** Das System dokumentiert Überlastung, löst sie aber nicht.

**Gegenmaßnahme:** Prioritäten, Active/Paused, Dashboard und begrenzte „Als Nächstes“-Sicht.

## R-09 – Duplicate Resources

**Risiko:** Gleiche Kurse werden mehrfach angelegt.

**Gegenmaßnahme:** URL-Dublettenprüfung und Canonical Resource.

## R-10 – Historische Daten verlieren Kontext

**Risiko:** Nach Jahren ist unklar, warum eine Ressource wichtig war.

**Gegenmaßnahme:** Motivation/Why, Path-/Skill-Beziehungen, Evidence und Timeline.

---

# 22. Constraints

| ID | Constraint | Quelle | Auswirkung |
|---|---|---|---|
| REQ-CON-001 | Der SASD Development Standard ist für das Projekt anzuwenden, soweit gemäß Projektklassifikation relevant. | Projektvorgabe | Dokumentation, Qualität, Security und Nachweise sind nachvollziehbar zu führen. |
| REQ-CON-002 | Das Produkt soll keine vorhandenen Anbieter rechtswidrig scrapen oder Schutzmechanismen umgehen. | Recht/Nutzungsbedingungen | Integrationen müssen zulässig gestaltet sein. |
| REQ-CON-003 | V1 ist primär ein persönlicher Learning Manager und kein Enterprise-LMS. | Produktstrategie | Rollen-/Team-/HR-Komplexität wird vermieden. |
| REQ-CON-004 | Kernfunktionen sollen nicht von einem einzelnen Lernanbieter abhängen. | Produktvision | Providerneutraler Domain-Kern. |
| REQ-CON-005 | Kernfunktionen sollen ohne verpflichtenden AI-Dienst nutzbar sein. | Produktstrategie/Privatsphäre | AI bleibt optionaler Ausbau. |
| REQ-CON-006 | Persönliche Daten müssen sicherbar und exportierbar sein. | Datenhoheit | Backup/Restore/Export gehören zum Produktumfang. |

---

# 23. Annahmen

| ID | Annahme | Status |
|---|---|---|
| A-001 | Die erste Hauptnutzung erfolgt durch einen einzelnen Benutzer. | Accepted for draft |
| A-002 | Die meisten externen Plattformen werden zunächst über normale Links eingebunden. | Accepted for draft |
| A-003 | Automatischer Fortschrittsimport aus O’Reilly/LinkedIn/Udemy ist für V1 nicht erforderlich. | Accepted for draft |
| A-004 | Manuelle Fortschrittspflege ist für V1 ausreichend, wenn sie schnell bedienbar ist. | Accepted for draft |
| A-005 | Skills können zunächst durch den Benutzer selbst definiert werden. | Accepted for draft |
| A-006 | Eine einfache fünfstufige Skill-Skala reicht als Startpunkt. | To validate |
| A-007 | Lokale/offline nutzbare Kerndaten sind für persönliche Daten vorteilhaft. | Accepted for draft |
| A-008 | Knowledge Management bleibt bewusst leichter als in Obsidian/Heptabase. | Accepted for draft |
| A-009 | Retention wird zunächst leichtgewichtig modelliert und nicht mit vollständigem SRS. | Accepted for draft |
| A-010 | Browser Extension, AI und automatische Metadaten sind nachgelagerte Komfortfunktionen. | Accepted for draft |

---

# 24. Offene strategische Fragen

Diese Fragen müssen **nicht vor Beginn jedes Prototyps** geklärt werden, sollten aber vor dem entsprechenden Feature entschieden werden.

| ID | Frage | Bedeutung | Zielphase | Status |
|---|---|---|---|---|
| Q-001 | Soll die fünfstufige Skill-Skala fest oder konfigurierbar sein? | Datenmodell/UI | vor V1-Freeze | Open |
| Q-002 | Werden Topic und Skill dauerhaft getrennte Objekte oder genügt teilweise ein gemeinsames Konzept? | Domain Model | vor Architekturfreeze | Open |
| Q-003 | Wie wird Path-Fortschritt bei optionalen Elementen berechnet? | Fachregel | vor Implementierung Progress | Open |
| Q-004 | Soll ein Learning Path selbst versioniert werden oder genügt Kopieren/Archivieren? | Historie | V1.x | Open |
| Q-005 | Welche Ressourcentypen sind im ersten Release wirklich notwendig? | UX/Domain | vor V1 | Open |
| Q-006 | Wie weit sollen lokale Dateien in der Anwendung verwaltet versus nur referenziert werden? | Storage | Pflichtenheft | Open |
| Q-007 | Welche Exportformate sind verpflichtend: JSON, CSV, Markdown oder Kombination? | Portabilität | Pflichtenheft | Open |
| Q-008 | Soll „Learning Need“ bereits in V1 Core oder erst V1.x sichtbar sein? | Umfang | V1-Planung | Open |
| Q-009 | Wie wird „Retention“ ohne Scheingenauigkeit dargestellt? | UX/Fachlogik | V1.x | Open |
| Q-010 | Welche automatischen Metadaten dürfen ohne Datenschutz-/Nutzungsprobleme von URLs ermittelt werden? | Integration | V1.x | Open |
| Q-011 | Ist eine grafische Roadmap schon für V1 nötig oder reicht zunächst eine hierarchische Strukturansicht? | Aufwand/Nutzen | V1 UI | Open |
| Q-012 | Wie stark sollen Notes und Knowledge Artifacts getrennt sein? | Domain Model | Architektur | Open |

---

# 25. Rückverfolgbarkeit zu den wichtigsten Vorbildern

Das Lastenheft übernimmt keine Produkte, sondern abstrahiert bewährte Ideen.

| SASD-Konzept | Wesentliche Inspiration |
|---|---|
| Goal / Role / Skill Gap | Degreed, LinkedIn Learning, 360Learning, Pluralsight |
| Kompetenzmodell | Degreed, Moodle, 360Learning |
| Learning Path als Struktur | roadmap.sh, Docebo, Degreed |
| Canonical Resource | Zotero, Capacities |
| Quick Capture / Inbox | Karakeep, Raindrop.io |
| gemischte Provider | Class Central, O’Reilly, Degreed |
| Resource → Knowledge | Readwise Reader, Heptabase, Obsidian |
| Completion getrennt von Mastery | Khan Academy, Pluralsight |
| Evidence | Pluralsight Labs, Moodle Competencies, Degreed |
| Retention | RemNote, Anki |
| Smart Views | Zotero Saved Searches, Capacities Queries, Obsidian Bases, Readwise Filtered Views |
| Link Preservation | Linkwarden, Karakeep, linkding |
| Minimalismus | linkding |
| offene lokale Wissensdaten | Obsidian |
| object-orientierte Wissensstruktur | Capacities |
| relationale Views | Notion |
| persönlicher Topic/Resource/Learning-Graph | LearnAwesome |

---

# 26. Priorisierte V1-Anforderungsauswahl

Die folgende Liste ist der empfohlene **harte Fokus** für die erste wirklich nutzbare Version.

## 26.1 Muss in V1 Core

### Fachstruktur

- Goals
- Competency Areas
- Skills
- Ist-Level
- Ziel-Level
- Skill Gap

### Learning Paths

- Path
- hierarchische Nodes
- Reihenfolge
- Skill-Zuordnung
- Ressourcen-Zuordnung
- Path-Fortschritt
- Active/Paused/Completed

### Resources

- kanonische Resource
- Provider
- Typ
- URL
- Beschreibung
- Priorität
- Status
- Tags
- Skill-/Topic-/Path-Zuordnung
- Fortschritt
- Archivierung

### Capture

- URL Quick Capture
- Inbox
- spätere Klassifikation
- URL-Dublettenwarnung

### Lernen

- Planned/Started/Paused/Completed/Abandoned
- Fortschritt
- Start/Abschluss
- Next Action

### Knowledge / Evidence

- Resource Notes
- einfache Knowledge Artifacts
- Evidence
- Skillzuordnung
- manuelles Skill Assessment

### Navigation

- Dashboard
- globale Suche
- zentrale Filter
- Inbox
- Aktuell in Arbeit
- Skill Gaps
- Ressourcen ohne Zuordnung

### Betrieb

- lokaler/offline nutzbarer Kern
- Backup
- Restore
- Logging
- sichere Archivierung
- keine externen Plattformpasswörter

---

## 26.2 Sollte früh nach V1 Core folgen

- Learning Needs
- Voraussetzungen und Alternativen im Path
- Ressourcenrelationen
- Skill-Level-Historie
- Lernhistorie/Timeline
- automatische Basis-Metadaten
- Saved Searches
- Markdown-Export
- Ressourcenimport
- Review-Termine
- Lernzeit/Sessions
- persönliche Berichte

---

## 26.3 Bewusst später

- AI
- semantische Suche
- Browser Extension
- Webarchivierung
- Broken-Link-Monitoring
- Anki/RemNote
- Kalender
- Provider APIs
- Teamfunktionen
- öffentliche Vorlagen
- vollständige visuelle Graphansicht

---

# 27. Vorschlag für fachliche V1-Navigation

Dies ist keine bindende UI-Vorgabe, sondern eine fachliche Orientierung.

```text
Heute / Dashboard
│
├── Ziele
├── Lernpfade
├── Skills
├── Ressourcen
│   ├── Inbox
│   ├── Geplant
│   ├── In Arbeit
│   ├── Abgeschlossen
│   └── Archiv
├── Wissen
├── Evidence
├── Suche / Smart Views
└── Datenpflege / Einstellungen
```

Ein möglicher Detailfluss:

```text
Goal
 ├── relevante Skills
 ├── Skill Gaps
 └── Learning Paths
       ├── Module / Nodes
       │    ├── Skills
       │    ├── Resources
       │    └── Activities
       └── Progress
```

---

# 28. Definition des gewünschten Produktcharakters

Der SASD Learning Manager soll sich langfristig **nicht** wie eine Lernplattform anfühlen, die einem vorschreibt, welche Kurse zu konsumieren sind.

Er soll eher die Eigenschaften folgender Werkzeuge verbinden:

- die **Ziel- und Skillorientierung** einer LXP,
- die **Landkarte** von roadmap.sh,
- die **kanonische Bibliothek** von Zotero,
- die **schnelle Inbox** von Karakeep/Raindrop,
- die **Wissensextraktion** von Readwise/Heptabase,
- die **Mastery-Trennung** von Khan Academy,
- die **Evidenz-/Assessment-Idee** von Pluralsight,
- die **Langfristigkeit** von Obsidian,
- die **Retention-Idee** von RemNote/Anki,

ohne die Komplexität dieser Systeme vollständig zu übernehmen.

---

# 29. Erfolgskriterien für das Gesamtprojekt

Der SASD Learning Manager ist fachlich erfolgreich, wenn ein Benutzer nach längerer Nutzung zuverlässig Fragen wie diese beantworten kann:

1. **Welche Themen möchte ich aktuell lernen?**
2. **Warum lerne ich sie?**
3. **Welche Skills gehören zu meinem Ziel?**
4. **Welche Skills fehlen mir noch?**
5. **Welche Lernpfade habe ich dafür vorgesehen?**
6. **Welche Ressourcen habe ich bereits gesammelt?**
7. **Welche davon habe ich wirklich begonnen oder abgeschlossen?**
8. **Welche Ressource gehört zu mehreren Themen und muss nicht doppelt gepflegt werden?**
9. **Was soll ich als Nächstes tun?**
10. **Welche Kurse überschneiden sich oder sind Alternativen?**
11. **Welche Evidence habe ich für einen Skill?**
12. **Was kann ich wahrscheinlich bereits gut genug?**
13. **Welches Wissen wurde lange nicht verwendet?**
14. **Was habe ich in den vergangenen Monaten tatsächlich gelernt?**
15. **Welche Notizen oder Erkenntnisse entstanden dabei?**
16. **Kann ich meine gesamte Lernhistorie sichern und exportieren?**

Wenn das Produkt diese Fragen verlässlich beantwortet, erfüllt es seinen Kernzweck.

---

# 30. Änderungsmanagement

Änderungen dieses Lastenhefts sollen nachvollziehbar dokumentiert werden.

| Datum | Version | Bereich | Änderung | Begründung | Entscheidung |
|---|---|---|---|---|---|
| 2026-08-27 | 0.1 | Gesamt | Erstfassung aus Produkt-Benchmark abgeleitet | Grundlage für Project Brief, Domain Model und Pflichtenheft | Proposed |

Neue Funktionen sollen vor Aufnahme insbesondere gegen folgende Fragen geprüft werden:

1. Unterstützt die Funktion einen Kernworkflow?
2. Ist sie bereits durch eine einfachere Funktion abgedeckt?
3. Gehört sie tatsächlich in den Learning Manager oder in ein Spezialtool?
4. Erhöht sie Verwaltungsaufwand?
5. Braucht sie V1 wirklich?
6. Welche Referenzprodukte zeigen ihren Nutzen?
7. Welche Datenobjekte und Risiken entstehen?
8. Ist sie mit Datenschutz und Datenhoheit vereinbar?
9. Kann sie später modular ergänzt werden?
10. Welche Akzeptanzkriterien würden ihre Fertigstellung belegen?

---

# 31. Freigabestand

**Status:** Entwurf / Proposed  
**Freigabe:** noch nicht erfolgt  
**Nächster empfohlener Schritt:** strategische Prüfung des Lastenhefts und anschließende Ableitung eines kompakten SASD Project Brief sowie eines fachlichen Domain Models.

Dieses Lastenheft sollte **vor Erstellung des Pflichtenhefts** auf folgende Punkte überprüft werden:

- Ist V1 noch klein genug?
- Fehlt ein Kernworkflow?
- Sind Goal, Topic und Skill sauber genug voneinander abgegrenzt?
- Ist Learning Need bereits V1-relevant?
- Ist das Mastery-Modell verständlich und nicht zu akademisch?
- Ist die Grenze zwischen Note und Knowledge Artifact sinnvoll?
- Reicht eine hierarchische Roadmap für V1?
- Sind Backup/Export früh genug berücksichtigt?
- Sind Funktionen identifiziert, die besser extern integriert als selbst entwickelt werden?

---

# 32. Quellen und Bezugsdokumente

## Interne Projektgrundlage

- **SASD Learning Manager – Vorlagenprogramme und Funktionsanalyse**, Version 0.1, 27.08.2026.

## SASD Development Standard

- <https://github.com/Robin-Goerlach/SASD-Development-Standard>
- Requirements Template:  
  <https://github.com/Robin-Goerlach/SASD-Development-Standard/blob/main/templates/documents/REQUIREMENTS-TEMPLATE.md>

## Referenzprodukte

Die ausführlichen Produktquellen sind im vorgelagerten Research-Dokument dokumentiert. Berücksichtigt wurden:

- Degreed
- roadmap.sh
- Zotero
- Karakeep
- Readwise Reader
- Pluralsight
- O’Reilly Learning
- Heptabase
- RemNote
- Class Central
- Khan Academy
- Obsidian
- LinkedIn Learning
- Moodle
- 360Learning
- Docebo
- Raindrop.io
- Capacities
- Notion
- Linkwarden
- linkding
- LearnAwesome
- Anki

---

# Anhang A – Kompakte Requirement-Typen

| Präfix | Bedeutung |
|---|---|
| REQ-F-* | funktionale Anforderung |
| REQ-Q-* | Qualitätsanforderung |
| REQ-SEC-* | Sicherheits-/Datenschutzanforderung |
| REQ-OPS-* | Betriebsanforderung |
| REQ-CON-* | Constraint |
| BR-* | fachliche Geschäftsregel |
| UC-* | Use Case |
| AK-* | produktweites Akzeptanzkriterium |
| Q-* | offene Frage |
| A-* | Annahme |

---

# Anhang B – Kernmodell in Kurzform

```text
Goal
 │
 ├──────────────► Learning Path
 │                    │
 │                    ├── Node
 │                    │    ├── Skill
 │                    │    └── Resource
 │                    │
 │                    └── Progress
 │
 └──────────────► Skill
                      │
                      ├── Current Level
                      ├── Target Level
                      ├── Evidence
                      ├── Knowledge
                      ├── Resources
                      └── Review / Recency

Resource
 │
 ├── Provider
 ├── Type
 ├── URL / Local Reference
 ├── Status / Progress
 ├── Tags
 ├── Notes
 ├── Evidence
 └── Relations to other Resources
```

---

# Anhang C – Entscheidender fachlicher Unterschied

```text
RESOURCE COMPLETION
O'Reilly-Kurs: 100 %
          │
          │ liefert mögliche Evidence
          ▼
SKILL MASTERY
Docker Networking: 3 / 5
          │
          │ verändert sich durch Praxis,
          │ Assessments und weitere Evidence
          ▼
RETENTION / RECENCY
zuletzt praktisch genutzt: vor 8 Monaten
```

Diese drei Dimensionen bleiben im SASD Learning Manager bewusst getrennt.

---

**Ende des Lastenhefts**
