# SASD Learning Manager – Vorlagenprogramme und Funktionsanalyse

**Dokumenttyp:** Research / Produkt-Benchmark  
**Projekt:** SASD Learning Manager  
**Stand:** 27. August 2026  
**Version:** 0.1 – Research Draft  
**Zweck:** Systematische Erfassung der Programme, Plattformen und Open-Source-Projekte, die als funktionale oder konzeptionelle Vorbilder für den SASD Learning Manager in Betracht kommen.

---

## 1. Ziel dieses Dokuments

Der SASD Learning Manager soll **kein Nachbau eines bestehenden Produkts** werden. Ziel dieser Analyse ist deshalb nicht, einen einzelnen Wettbewerber zu kopieren, sondern aus verschiedenen Produktklassen zu lernen:

- Learning Experience Platforms (LXP)
- Learning Management Systems (LMS)
- Skill-/Competency-Management
- Lernpfad- und Roadmap-Systeme
- Kurs- und Ressourcenaggregatoren
- Bookmark- und Read-it-later-Systeme
- Personal Knowledge Management (PKM)
- Literatur- und Quellenverwaltung
- Notiz- und Annotationstools
- Spaced-Repetition- und Mastery-Systeme
- Self-hosted/Open-Source-Lösungen

Der Kernansatz für den SASD Learning Manager lautet:

> **Lernziel → Kompetenz → Skill → Lernpfad → Lernressource → Lernaktivität → Wissensnachweis → Mastery/Retention**

Eine Ressource – etwa ein O’Reilly-Kurs, ein YouTube-Video, ein Buch, ein PDF, ein Lab oder ein eigener Übungsaufbau – soll dabei **nicht das Lernziel selbst**, sondern ein Mittel zum Erwerb einer Kompetenz sein.

---

## 2. Methodik und Abgrenzung

Die Analyse basiert bevorzugt auf:

1. offiziellen Produktseiten,
2. offiziellen Hilfe- und Dokumentationsseiten,
3. offiziellen GitHub-Repositories bei Open-Source-Produkten,
4. öffentlich dokumentierten Funktionsübersichten.

Der Funktionsumfang großer Enterprise-Produkte wie Degreed, 360Learning, Docebo, Moodle oder LinkedIn Learning umfasst sehr viele administrative Detailoptionen. „Alle Funktionen“ bedeutet in diesem Dokument deshalb:

- vollständige Erfassung der **fachlich relevanten Funktionsgruppen**,
- Aufnahme wichtiger angrenzender Funktionen, aus denen sich Designideen ableiten lassen,
- Aufnahme relevanter Integrations-, Such-, Reporting-, Kollaborations-, Offline-, AI- und Administrationsfunktionen,
- **nicht** die wortwörtliche Aufzählung jedes einzelnen Konfigurationsschalters, jeder Rollenberechtigung oder jedes API-Endpunkts.

Wo ein Produkt besonders groß ist, werden Funktionen bewusst zu logisch zusammenhängenden Gruppen verdichtet.

Die Untersuchung orientiert sich außerdem am aktuellen **SASD Development Standard**. Dessen `main`-Branch ist am 27.08.2026 weiterhin als **Version 1.0 Specification Candidate** ausgewiesen. Die normative Baseline ist Approved, Version `1.0.0` jedoch noch nicht veröffentlicht. Der Standard empfiehlt für neue Projekte einen kleinen, nachvollziehbaren Einstieg über Quickstart, Project Brief, angemessene Qualitäts-/Profilwahl und eine reproduzierbare Build-/Test-Basis. Diese Produktanalyse ist deshalb als Research-Grundlage vor der späteren Ableitung von Project Brief, Funktionskatalog, Anforderungen und Architekturentscheidungen zu verstehen.

Quelle: <https://github.com/Robin-Goerlach/SASD-Development-Standard>

---

# 3. Berücksichtigte Programme

## 3.1 Kernreferenzen

1. Degreed
2. roadmap.sh
3. Zotero
4. Karakeep
5. Readwise Reader
6. Pluralsight
7. O’Reilly Learning
8. Heptabase
9. RemNote
10. Class Central
11. Khan Academy
12. Obsidian

## 3.2 Ergänzende Referenzen

13. LinkedIn Learning
14. Moodle
15. 360Learning
16. Docebo
17. Raindrop.io
18. Capacities
19. Notion
20. Linkwarden
21. linkding
22. LearnAwesome
23. Anki

---

# 4. Funktionsanalyse der Vorlagenprogramme

---

## 4.1 Degreed

### Einordnung

Degreed ist eine Enterprise-Learning-Experience- und Upskilling-Plattform. Für den SASD Learning Manager ist Degreed besonders wichtig, weil das Produkt **Lernen nicht ausschließlich als Kurskonsum**, sondern als Verbindung aus Skills, Rollen, Lernbedarf, Lernpfaden, Inhalten und nachweisbarer Entwicklung betrachtet.

### Funktionen

#### Lernökosystem und Inhaltsaggregation

- Zusammenführen von Lerninhalten aus unterschiedlichen internen und externen Quellen.
- Bereitstellung einer zentralen Lernoberfläche statt separater Silos einzelner Content-Provider.
- Unterstützung unterschiedlicher Lernressourcentypen.
- Kuratieren und Teilen von Lerninhalten.
- Erstellen strukturierter Lernpläne und Lernpfade.
- Personalisierte Startseiten.
- Personalisierte Empfehlungen.
- Suche über das verfügbare Lernökosystem.
- KI-gestützte Suche bzw. Chat-Funktionen für Lerninhalte.
- Integration externer Lernanbieter und bestehender Learning-Stacks.

#### Lernpfade und Pläne

- Individuelle Lernpläne.
- Kuratierte Pathways.
- Zusammenstellung mehrerer Ressourcen in einer geplanten Reihenfolge.
- Nutzung von Lernpfaden für Onboarding.
- Nutzung von Lernpfaden für Upskilling.
- Nutzung von Lernpfaden für Reskilling.
- Nutzung von Lernpfaden für rollenbezogene Entwicklung.
- Begleitung eines Lerners über mehrere Lernaktivitäten hinweg.
- Automatisierbare Workflows rund um Lernpläne.
- Benachrichtigungen und „Nudges“, um Lernende an Aktivitäten zu erinnern.

#### Skill-Management

- Zentraler Skill-Bestand.
- Harmonisierung bzw. Normalisierung unterschiedlicher Skill-Bezeichnungen.
- Zuordnung von Skills zu Rollen.
- Skill-Profile von Lernenden.
- Erfassung und Bewertung von Skill-Niveaus.
- Ratings und Assessments.
- Vergleich vorhandener Skills mit benötigten Skills.
- Identifikation von Skill Gaps.
- Verknüpfung von Lernaktivität und Skill-Entwicklung.
- Verwendung von Skill-Daten zur Personalisierung von Lernmaßnahmen.
- Nutzung von Skills für Workforce-/Talent-Readiness.
- Datenbasierte Ermittlung, welche Skills zukünftig benötigt werden.

#### Profile und Leistungsnachweise

- Persönliche Lernprofile.
- Darstellung erworbener bzw. entwickelter Fähigkeiten.
- Badges und andere Nachweise.
- Verwendung von Profilen als Signal für Entwicklungsstand bzw. Bereitschaft.
- Verifizierung von Leistungen.
- Verknüpfung von Lernaktivitäten mit Skill- und Profilinformationen.

#### Automatisierung

- Workflow-Automatisierung.
- Automatische Benachrichtigungen.
- Automatisierte Lernabläufe.
- Automatische bzw. regelbasierte Lernzuweisung.
- Reduktion manueller administrativer Schritte.

#### Analytics und Reporting

- Erfassung von Lernaktivität.
- Messung von Fortschritt.
- Auswertung von Skill-Entwicklung.
- Analyse, welche Lernmaßnahmen Wirkung zeigen.
- Reporting zu Lernnutzung und Skill-Veränderung.
- Nutzung von Lern- und Skilldaten für organisatorische Entscheidungen.

#### Kollaboration und Kuratierung

- Inhalte durch Personen innerhalb einer Organisation kuratieren lassen.
- Inhalte teilen.
- Lernpfade durch Fachverantwortliche zusammenstellen.
- Gemeinsames Lernen im organisatorischen Kontext.

#### Enterprise-Funktionen

- Rollen-/Skill-Mapping auf Organisationsebene.
- Integration bestehender HR-/Talent-/Learning-Systeme.
- Datenharmonisierung.
- Organisationsweite Skill- und Lernanalysen.
- Unterstützung von Onboarding, Performance, Upskilling und Reskilling.

### Besonders interessante Ideen für SASD

- **Skill zuerst, Content danach.**
- Trennung von „Ressource konsumiert“ und „Kompetenz entwickelt“.
- Lernpfade als Mittel zur Schließung konkreter Kompetenzlücken.
- Rollenprofile als Soll-Zustand.
- Persönliche Skills als Ist-Zustand.
- Skill Gap als Grundlage für Priorisierung.
- Badges/Nachweise als zusätzliche Evidenz, nicht als bloße Dekoration.

### Nicht einfach übernehmen

- HR- und Workforce-Komplexität.
- Mitarbeiterbewertung.
- Organisationshierarchien.
- Enterprise-Administration.
- Eine implizite Annahme, dass Skill-Niveaus exakt messbar seien.

### Quellen

- <https://degreed.com/experience/our-platform/>
- <https://degreed.com/experience/de/lxp/>

---

## 4.2 roadmap.sh

### Einordnung

roadmap.sh ist eines der wichtigsten Vorbilder für die **visuelle Strukturierung eines Kompetenzfeldes**. Anders als eine Playlist zeigt das System, welche Themen, Unterthemen, Alternativen und Entwicklungsschritte zu einem Gebiet gehören.

### Funktionen

#### Roadmaps

- Rollenbasierte Roadmaps, z. B. DevOps, Backend, Data Engineer oder Security-nahe Rollen.
- Skillbasierte Roadmaps, z. B. SQL, Python, PostgreSQL.
- Visuelle Baum-/Graphdarstellung von Lerngebieten.
- Hierarchische Strukturierung komplexer Themen.
- Darstellung von Abhängigkeiten und empfohlenen Reihenfolgen.
- Verlinkung einzelner Roadmap-Knoten mit Lernressourcen.
- Interaktive Roadmaps.
- Fortschrittsmarkierung innerhalb einer Roadmap.
- Markierung von Lerninhalten als bearbeitet bzw. Statusverfolgung.
- Community-gepflegte Inhalte.
- Vorschläge und Beiträge aus der Community.
- Review von Änderungen durch Maintainer bzw. Fachexperten.

#### Lernressourcen

- Guides.
- Artikel.
- Videos.
- themenspezifische Ressourcen.
- Projektideen.
- Best-Practice-Inhalte.
- Interviewvorbereitung.
- Quiz-/Wissensprüfungsfunktionen.
- Lesson Packs.
- AI-generierte Kurse, Guides und Roadmaps in aktuellen Premium-Funktionen.

#### Eigene Roadmaps

- Roadmap-Editor.
- Benutzerdefinierte Roadmaps.
- Eigene strukturierte Lernpläne.
- AI-gestützte Roadmap-Erstellung bzw. -Bearbeitung in Premium-Angeboten.
- Mehrere eigene Roadmaps je Benutzer/Plan.

#### Lernplanung

- Learning Plans.
- Fortschrittsverfolgung.
- Persönliche Lernentwicklung.
- Orientierung, was als Nächstes gelernt werden sollte.

#### Teamfunktionen

- Teams anlegen.
- Mitglieder einladen.
- Benutzerdefinierte Team-Roadmaps.
- Roadmaps einzelnen Teammitgliedern zuweisen.
- Gemeinsame und individuelle Growth Plans.
- Onboarding-Pläne.
- Skill-Gap-Analyse.
- Team-Insights.
- Fortschrittsvergleich.
- Rollen und Berechtigungen.
- Granulares Sharing.
- visuelle Teamdokumentation.

#### AI-Funktionen

- AI Chat.
- AI-Kurse.
- AI-Guides.
- AI-Roadmaps.
- AI-Quizzes.
- AI-Lernpläne.
- Chat-Historie.
- AI-gestützter Roadmap-Editor.

### Besonders interessante Ideen für SASD

- **Lernpfad als Landkarte statt Playlist.**
- Knoten repräsentieren Themen/Skills; Ressourcen hängen an Knoten.
- Fortschritt sollte auf Themenebene und nicht nur auf Ressourcenebene sichtbar sein.
- Ein Themenkomplex kann mehrere Wege haben.
- Roadmaps können sowohl rollen- als auch skillorientiert sein.
- Ein Benutzer sollte eigene Roadmaps erstellen und bestehende als Vorlage kopieren können.

### Nicht einfach übernehmen

- Entwicklerfokus als harte Grenze.
- Eine für alle Benutzer identische Roadmap.
- AI-Erzeugung ohne nachvollziehbare fachliche Quellen.

### Quellen

- <https://roadmap.sh/>
- <https://roadmap.sh/about>
- <https://roadmap.sh/teams>
- <https://roadmap.sh/premium>

---

## 4.3 Zotero

### Einordnung

Zotero ist ein Literatur- und Quellenmanager. Für SASD ist es vor allem wegen seines **kanonischen Ressourcenmodells** interessant: Ein Objekt existiert einmal und kann gleichzeitig in mehreren Sammlungen vorkommen, ohne dupliziert zu werden.

### Funktionen

#### Ressourcen-/Item-Modell

- Bibliographische Items für Bücher, Artikel, Reports, Webseiten, Filme, Manuskripte, Audio, Rechtsquellen und viele weitere Typen.
- Typabhängige Metadaten.
- Titel, Autor/Creator, Herausgeber, Datum, Seiten, Publikation usw.
- Anhänge zu Items.
- Notizen zu Items.
- Links und Snapshots.
- Dateien können mit oder ohne übergeordnetes Item gespeichert werden.

#### Web-Erfassung

- Browser Connector.
- Automatische Erkennung bibliographischer Daten auf Webseiten.
- Übernahme von Metadaten.
- Download bzw. Übernahme verfügbarer Anhänge.
- Speichern von Webseitenquellen.
- Automatisches Erkennen wissenschaftlicher Quellen aus Bibliotheken, Datenbanken und Verlagsseiten.

#### Collections

- Hierarchische Collections und Subcollections.
- Ein Item kann mehreren Collections gleichzeitig angehören.
- Keine Duplikation des Items bei Mehrfachzuordnung.
- Verschieben und Zuordnen von Items.
- Projekt-, Kurs-, Themen- oder Quellenorientierte Collections.
- Unfiled Items.
- Trash.
- Duplicate Items.
- My Publications.
- Gespeicherte Suchergebnisse als dynamische Sammlungen.

#### Tags

- Beliebig viele Tags je Item.
- Manuelle Tags.
- Automatisch importierte Tags/Keywords.
- Farbige Tags.
- Tag-Filter.
- Massenzuordnung bzw. Bulk-Tagging.
- Umbenennen und Bereinigen von Tags.
- Kombination mehrerer Tags zur Filterung.

#### Suche

- Schnellsuche über Metadaten, Tags und Volltext.
- Advanced Search.
- Kombination mehrerer Suchkriterien.
- Saved Searches.
- Automatisch aktualisierte gespeicherte Suchergebnisse.

#### PDF Reader und Annotation

- Integrierter PDF-Reader.
- Text markieren.
- Text unterstreichen.
- Annotationen.
- Kommentare an Annotationen.
- Annotationsfarben.
- Annotationen in Notizen übernehmen.
- Links aus Notizen zurück zur PDF-Seite.
- Zitationsbezug der Annotation.
- Erstellen von Notizen aus allen Annotationen eines Dokuments.
- Erstellen von Notizen aus Annotationen mehrerer Items.
- Drag-and-drop von Annotationen in Notizen.
- Vorlagen für die Darstellung von Annotationen in Notizen.
- Export von PDFs mit eingebetteten Annotationen.
- Optional externer PDF-Reader.

#### Notizen

- Child Notes an einem Item.
- Standalone Notes.
- Rich-Text-Notizen.
- Verlinkung/Bezug zu Quellen.
- Automatisches Speichern.
- Synchronisation von Notizen.
- Annotationsbasierte Notizen.

#### Zitation und Bibliographie

- Zitate erzeugen.
- Bibliographien erzeugen.
- Tausende Zitationsstile.
- Word-Integration.
- LibreOffice-Integration.
- Google-Docs-Integration.
- Zitationsdaten direkt aus Quellen.

#### Dateien und Synchronisation

- Dateianhänge.
- Synchronisation von Metadaten.
- Datei-Synchronisation.
- Web Library.
- Nutzung auf mehreren Plattformen.
- Mobile Nutzung.

#### Zusammenarbeit

- Group Libraries.
- Gemeinsame Quellenbibliotheken.
- Teilen und gemeinsames Bearbeiten von Sammlungen.
- Gruppenbasierte Bibliotheken.

#### Erweiterbarkeit

- Plugins.
- Export-/Importformate.
- APIs und Integrationen.
- Offenes Ökosystem.

### Besonders interessante Ideen für SASD

- Eine Lernressource darf **nur einmal** existieren.
- Eine Ressource kann in beliebig vielen Lernpfaden, Themen oder Projekten verwendet werden.
- Tags und Collections erfüllen unterschiedliche Aufgaben.
- Dynamische „Saved Searches“ sind ein starkes Vorbild für Smart Views.
- Quellen- und Dateimetadaten gehören zum Resource Domain Model.
- Annotationen sollten ihren Ursprung behalten.

### Quellen

- <https://www.zotero.org/>
- <https://www.zotero.org/support/quick_start_guide>
- <https://www.zotero.org/support/collections_and_tags/>
- <https://www.zotero.org/support/pdf_reader>
- <https://www.zotero.org/support/notes>

---

## 4.4 Karakeep

### Einordnung

Karakeep, früher Hoarder, ist ein Open-Source-„Bookmark Everything“-System mit Self-hosting-Schwerpunkt. Für SASD ist Karakeep besonders interessant für **Capture, Inbox, Archivierung, Suchbarkeit und automatische Metadatenverarbeitung**.

### Funktionen

#### Erfassen von Inhalten

- Webseiten/Links speichern.
- Notizen speichern.
- Bilder speichern.
- PDFs speichern.
- automatische Ermittlung von Seitentitel.
- automatische Ermittlung von Beschreibung.
- automatische Ermittlung von Vorschaubildern.
- Browser-Erweiterung für Chrome.
- Firefox-Addon.
- Safari-Erweiterung.
- iOS-App.
- Android-App.
- RSS-basierte automatische Übernahme.
- Import aus anderen Bookmark-/Read-it-later-Systemen.
- Browser-Bookmark-Synchronisation über floccus.

#### Organisation

- Listen.
- gemeinsame Listen.
- Tags.
- automatische Tags.
- Bulk Actions.
- Regeln zur automatischen Verwaltung.
- mehrsprachige Inhalte.

#### Suche

- Volltextsuche.
- semantische Suche.
- Suche über gespeicherte Inhalte.
- OCR-Texterkennung für Bilder.
- AI-/LLM-basierte Erschließung.

#### AI-Funktionen

- LLM-basiertes Auto-Tagging.
- automatische Zusammenfassungen.
- Unterstützung lokaler Modelle über Ollama.
- Agentenfreundliche CLI.
- offizielle Skills für Agenten.
- semantische Suche.
- regelbasierte Verarbeitung kann mit AI-Funktionen kombiniert werden.

#### Archivierung

- Full-page archival.
- Archivierung mit Monolith.
- Schutz gegen Link Rot.
- Videoarchivierung mit yt-dlp.
- Speicherung von Content zusätzlich zur Original-URL.

#### Lesen und Markieren

- Mobile Offline Reading.
- Highlights in gespeicherten Inhalten.
- Speicherung von Markierungen.
- spätere Wiederauffindbarkeit.

#### Zusammenarbeit

- Gemeinsame Listen.
- Inhalte gemeinsam sammeln.

#### Integration und Technik

- REST API.
- CLI.
- mehrere Clients.
- SSO-Unterstützung.
- Dark Mode.
- Self-hosting als First-Class-Ziel.
- Importer u. a. für Chrome, Pocket, Linkwarden, Omnivore und weitere Quellen.

### Besonders interessante Ideen für SASD

- **Inbox-First-Capture:** URL speichern muss extrem schnell gehen.
- Metadaten können später vervollständigt werden.
- Linkarchivierung schützt Lernpfade vor Link Rot.
- Volltext- und semantische Suche ergänzen strukturierte Metadaten.
- Automatisches Tagging sollte Vorschläge liefern, aber nicht ungeprüft die fachliche Struktur bestimmen.
- Lokale AI-Modelle sind langfristig attraktiv.

### Quelle

- <https://docs.karakeep.app/>

---

## 4.5 Readwise Reader

### Einordnung

Reader ist ein Read-it-later- und Wissensextraktionssystem. Es eignet sich als Vorbild für den Übergang von **„Ressource gespeichert“ zu „Ressource verarbeitet und Wissen extrahiert“**.

### Funktionen

#### Unterstützte Inhalte

- Webseitenartikel.
- PDFs.
- EPUBs.
- Newsletter.
- RSS-/Feed-Inhalte.
- YouTube-/Video-Inhalte.
- weitere webbasierte Dokumente.
- Import vorhandener Leselisten bzw. Inhalte.

#### Bibliothek und Zustände

- zentrale Library.
- Inbox-/Later-artige Workflows.
- unterschiedliche Dokumentzustände.
- Archivierung.
- Verwaltung ungelesener und gelesener Dokumente.
- Dokumentmetadaten.
- Tags.

#### Lesen

- optimierte Reader-Ansicht.
- Lesen längerer Artikel.
- PDF-Lesen.
- EPUB-Lesen.
- Video-/Transcript-orientiertes Lernen.
- Nutzung auf mehreren Endgeräten.

#### Highlights und Annotation

- Text markieren.
- Highlights kommentieren.
- Tags auf Highlights.
- Dokumentnotizen.
- Markierungen später wiederfinden.
- Highlight-Export in das Readwise-Ökosystem.
- Wiederverwendung der Erkenntnisse in anderen Notizsystemen über Integrationen.

#### YouTube/Video

- Video speichern.
- Transcript anzeigen.
- Transcript synchron zum Video nutzen.
- Textstellen des Transcripts markieren.
- Kommentare/Notizen zu Transcriptstellen.
- Video wie ein lesbares Dokument bearbeiten.

#### Organisation

- Dokument-Tags.
- Highlight-Tags.
- Filtered Views.
- gespeicherte Filteransichten.
- Filter u. a. nach Tags, Quelle, Autor, Speicherdatum, Leselänge und Highlight-Anzahl.
- Queries für dynamische Views.
- Views in der Navigation anheften.

#### Feeds und Newsletter

- RSS-/Feed-Nutzung.
- Verwaltung von Feed-Quellen.
- Newsletter in Reader einliefern.
- Feeds bestimmten Ansichten zuordnen.

#### Suche und Wiederfinden

- Bibliothekssuche.
- Filterabfragen.
- dynamische Gruppierung nach Regeln.
- Autorenbezogene Sichten.
- domain-/quellenbezogene Sichten.

#### Integrationen

- Verbindung zum Readwise-Highlight-System.
- Exporte bzw. Synchronisation zu PKM-/Notizsystemen.
- Workflow zwischen Lesen, Markieren, Wiederholen und langfristigem Wissen.

### Besonders interessante Ideen für SASD

- Eine Lernressource braucht einen **Verarbeitungsstatus**, nicht nur „gespeichert/fertig“.
- Highlights können als Evidence oder Knowledge Artifact aus einer Ressource hervorgehen.
- YouTube-Transkripte machen Videos deutlich besser durchsuchbar und dokumentierbar.
- Smart Views sind als gespeicherte Abfragen interessanter als starre Ordner.
- SASD sollte Reader-Funktionalität eher **integrieren/verlinken** als einen kompletten Reader nachbauen.

### Quellen

- <https://docs.readwise.io/reader/docs/organizing-content>
- <https://docs.readwise.io/reader/docs/faqs/filtered-views>

---

## 4.6 Pluralsight

### Einordnung

Pluralsight ist vor allem wegen der Verbindung **Assessment → Skill Level → Lernpfad → Praxis → erneute Messung** relevant.

### Funktionen

#### Kurse und Inhalte

- On-Demand-Videokurse.
- strukturierte Technologie- und Themenkataloge.
- Kursfortschritt.
- Completion.
- Lernchecks innerhalb von Kursen.
- Zertifikate für Kursabschlüsse.

#### Paths

- kuratierte Lernpfade.
- Kombination von Kursen und anderen Ressourcen.
- Strukturierung nach Skill-Bereichen.
- oft Einteilung in Beginner, Intermediate und Advanced.
- Integration von Skill-IQ-Assessments.
- Integration von Zertifizierungs-Practice-Exams.
- Fortschrittsverfolgung innerhalb eines Paths.
- Synchronisation des Kursfortschritts in Pfaden.

#### Skill IQ

- adaptives Assessment.
- relative Bewertung gegenüber anderen Teilnehmern.
- ungefähr 15-minütige Assessments.
- Zuordnung zu einem passenden Einstiegsniveau.
- Wiederholung des Assessments zur Fortschrittsmessung.
- Verknüpfung mit Paths.

#### Role IQ

- rollenbezogene Skill-Bewertung.
- Vergleich von Fähigkeiten mit Rollenprofilen.
- rollenbezogene Entwicklung.
- in Enterprise-Plänen teilweise anpassbare Rollen.

#### Labs

- Hands-on-Labs.
- temporäre, sichere Browserumgebungen.
- Schritt-für-Schritt-Ziele.
- eigenständige Labs.
- Labs innerhalb von Kursen.
- IT-Ops-Labs.
- Security-Labs.
- Coding-Sandboxes.
- Cloud-/Sandbox-Umgebungen je Plan.
- praktische Anwendung des Gelernten.
- Labs als praktische Prüfung bzw. Assessment nutzbar.

#### Channels

- persönliche oder organisatorische Inhaltszusammenstellungen.
- Kurse/Paths in Channels sammeln.
- Kuratierung für Teams oder Themen.

#### Zertifizierung

- Vorbereitung auf Zertifizierungen.
- Practice Exams.
- Upload externer Zertifizierungen.
- Abschlussnachweise.

#### AI

- AI Assistant („Iris“ in aktuellen Plänen).
- Unterstützung bei Navigation und Lernen.

#### Analytics/Teams

- Priorities.
- Rollen-/Skill-Analytics.
- Fortschrittsmessung.
- teambezogene Auswertungen.
- Lern- und Skilldaten.

### Besonders interessante Ideen für SASD

- Lernpfade sollten optional ein **Einstiegsassessment** haben.
- Skill-Level darf nicht aus „Kurs abgeschlossen“ allein abgeleitet werden.
- Praktische Labs sind eine eigene Evidence-Kategorie.
- Wiederholtes Assessment kann Skill-Fortschritt sichtbar machen.
- Ein Skill Gap kann die Auswahl bzw. Reihenfolge einer Roadmap verändern.

### Quellen

- <https://help.pluralsight.com/hc/en-us/articles/24418811505044-Paths>
- <https://help.pluralsight.com/help/what-is-skill-iq>
- <https://help.pluralsight.com/hc/en-us/articles/24356159003924-Labs-overview>
- <https://help.pluralsight.com/hc/en-us/articles/31499201424020-Skills-subscription-and-plan-comparison>

---

## 4.7 O’Reilly Learning

### Einordnung

O’Reilly ist für das Projekt besonders relevant, weil es bereits heute sehr unterschiedliche Lernmedien innerhalb einer Plattform verbindet und damit ein gutes Vorbild für unser **anbieterunabhängiges Resource Model** ist.

### Inhalte und Medien

- E-Books.
- Early-Release-Bücher.
- Audiobooks.
- Videos.
- strukturierte On-Demand-Kurse.
- Live Events.
- virtuelle Konferenzen.
- Superstreams.
- Cohorts.
- Katas.
- spezielle Events.
- Labs.
- Cloud Labs.
- Sandboxes.
- Cloud Sandboxes.
- Practice Exams.
- Zertifizierungsvorbereitung.
- Playlists.
- kuratierte Collections.

#### Kurse

- On-Demand-Instructor-Led-Video.
- Fortschrittserfassung.
- Completion Tracking.
- teilweise Abschlussquiz.
- Badges bei Erfüllung definierter Abschlusskriterien.
- strukturierte Lerninhalte.

#### Bücher

- digitales Lesen.
- Notizen in Büchern.
- Suche.
- anbieterübergreifender Buch-/Verlagskatalog innerhalb der Plattform.
- frühe Vorabversionen („Early Release“).
- teilweise Audioformate.

#### Playlists

- eigene Playlists.
- geteilte Playlists.
- Organisations-Playlists.
- von O’Reilly kuratierte Playlists.
- Mischung unterschiedlicher Inhalte in Sammlungen.

#### Hands-on Learning

- geführte Labs.
- vorkonfigurierte Entwicklungsumgebungen.
- unguided Sandboxes.
- Cloud Labs mit temporären Cloud-Accounts.
- Cloud Sandboxes.
- Lernen direkt im Browser ohne lokale Einrichtung.

#### Live Learning

- Live Courses.
- Fragen in Echtzeit.
- Konferenzen.
- Superstreams.
- Katas.
- Special Events.
- Cohorts.
- teils Replay registrierter Veranstaltungen.

#### Zertifizierungen

- offizielle Vorbereitungsmaterialien.
- Practice Tests.
- Exam Prep vieler großer Technologieanbieter.
- interaktive Prüfungsfragen.
- unmittelbares Feedback.
- Badges/Completion-Nachweise.

#### Suche und Answers

- plattformweite Suche über Bücher, Kurse, Videos, Events und weitere Formate.
- O’Reilly Answers: Frage stellen und passende Text-/Videostellen finden.
- NLP-/AI-gestützte Informationssuche.

#### Personalisierung

- Empfehlungen.
- individuelle Playlists.
- Notizen.
- personalisierte Inhalte.
- Skill-orientierte Empfehlungen bzw. Skill Plans in aktuellen Angeboten.

#### Skill Plans / Assessments

- diagnostische Skill-Assessments in aktuellen Skill-Plan-Funktionen.
- Feststellung von Kompetenzlücken.
- Einordnung über Leistungsstufen.
- darauf aufbauende Lernplanung.

#### Mobile und andere Geräte

- Mobile Apps.
- Lernen unterwegs.
- Videos bzw. Events auch auf größeren Bildschirmen/TV.

#### Teams und Enterprise

- Learning Insights.
- Reporting.
- Nutzungsanalysen.
- Zertifizierungsstatistiken.
- Integrationen in Identity Provider, LXPs und Reportingsysteme.
- Professional Services.
- private Trainings.
- kuratierte Inhalte für Organisationen.

### Besonders interessante Ideen für SASD

- Eine **Resource** braucht einen klaren Typ, darf aber trotzdem einheitlich verwaltet werden.
- Ein Learning Path muss gemischte Medien unterstützen.
- „Video“ und „Course“ sind fachlich nicht zwingend dasselbe.
- Hands-on-Labs und Sandboxes sollten als eigenständige Lernressourcen gelten.
- Anbieter-Playlist und persönlicher SASD-Lernpfad dürfen nicht verwechselt werden.
- Completion-Badges können als Evidence übernommen werden.

### Quellen

- <https://www.oreilly.com/online-learning/features>
- <https://www.oreilly.com/online-learning/support/features.html>
- <https://www.oreilly.com/online-learning/integration-docs/search.html>
- <https://www.oreilly.com/live/>

---

## 4.8 Heptabase

### Einordnung

Heptabase ist ein Visual-PKM- und Research-System. Es ist ein wichtiges Vorbild dafür, wie Lernende **komplexe Themen räumlich strukturieren und Zusammenhänge zwischen Notizen, Quellen und Konzepten sichtbar machen** können.

### Funktionen

#### Cards und Notizen

- wiederverwendbare Note Cards.
- Markdown-orientierte Notizen.
- langfristige Kartenbibliothek.
- einzelne Karten können in mehreren visuellen Kontexten erscheinen.
- Backlinks und Verknüpfungen.
- Journal/Daily-Log-artige Erfassung.
- Inbox-orientierte Erfassung.
- Tags.
- Tag-Datenbank.

#### Whiteboards

- unendliche bzw. große visuelle Arbeitsflächen.
- Karten frei positionieren.
- Beziehungen räumlich darstellen.
- Text, Bilder, Notizen und Recherchefragmente kombinieren.
- Sections.
- Sub-Whiteboards.
- Verknüpfung visueller Elemente.
- komplexe Themen in Teilbereiche zerlegen.
- vorhandene Karten erneut auf Whiteboards verwenden.

#### Weitere Darstellungen

- Mindmaps.
- Tabellen.
- Kanban-Ansichten.
- Tag-Database-Views.
- Filter.
- Berechnungen in Datenansichten.
- Spaltensteuerung.

#### Research und Quellen

- PDF-Import.
- PDF-Highlighting.
- PDF-Annotation.
- YouTube-Transkripte.
- Audio-/Video-Transkription.
- Podcast-/Video-orientierte Quellenarbeit.
- Highlights.
- Verbindung von Quellen und eigenen Notizen.

#### Suche

- schnelle Volltextsuche.
- große Notizsammlungen durchsuchen.
- aktuelle MCP-Funktionen bieten zusätzlich semantische Suche.
- Suche nach Whiteboards.
- Suche in PDF-Inhalten über MCP.

#### AI

- AI Insights.
- AI-gestütztes Lernen.
- AI Tutor/Agent-Funktionen.
- Bilder in AI-Unterhaltungen.
- Video als AI-Kontext.
- semantische Recherche über eigene Wissensbasis.
- Zusammenfassen und Strukturieren eigener Inhalte.
- Speicherung von AI-Ergebnissen zurück als Notes/Journal-Einträge.

#### MCP

- externe AI-Tools können Heptabase lesen und durchsuchen.
- Notes erzeugen.
- Journal ergänzen.
- semantische Suche.
- Whiteboards finden.
- Whiteboard-Struktur auslesen.
- Objekte lesen.
- PDF-Inhalte durchsuchen.
- PDF-Seiten abrufen.
- Resultate zurück in die Wissensbasis schreiben.

#### Kollaboration

- gemeinsame Whiteboards.
- Echtzeit-Kollaboration.
- unterschiedliche Zugriffsrechte.
- Einladen von Mitwirkenden.
- öffentliches Read-only-Publishing von Whiteboards.
- öffentliche Links aktualisieren sich mit dem Whiteboard.

#### Offline und Synchronisation

- Offline-Zugriff.
- Echtzeit-Synchronisation.
- Desktop.
- Web.
- iOS.
- Android.
- Voice-Note-Unterstützung auf Mobilgeräten.

#### Datenportabilität

- Export von Karten.
- Export von Attachments.
- Import aus anderen Notizsystemen.
- Version History.
- Trash/Restore.
- Cloud Backups.

### Besonders interessante Ideen für SASD

- Eine optionale **Learning Map** könnte Ressourcen, Skills und Erkenntnisse räumlich anzeigen.
- Lernnotizen sollten eigenständige, wiederverwendbare Wissensobjekte sein können.
- Eine Ressource und das daraus gewonnene Wissen sind unterschiedliche Objekte.
- AI sollte auf die **eigene belegte Wissensbasis** zugreifen können.
- MCP/API als langfristige Erweiterung ist interessant.

### Quellen

- <https://heptabase.com/>
- <https://support.heptabase.com/>
- <https://support.heptabase.com/en/articles/12679581-how-to-use-heptabase-mcp>

---

## 4.9 RemNote

### Einordnung

RemNote kombiniert PKM, Quellenarbeit, Flashcards, Spaced Repetition und inzwischen AI-gestütztes Guided Learning. Für SASD ist es ein Vorbild für die Phase **„Wissen behalten“**.

### Notizen und Wissensbasis

- hierarchische Notizen.
- Verknüpfungen.
- strukturierte Dokumente.
- Tags und organisatorische Strukturen.
- Knowledge Base.
- Inhalte aus Notizen direkt in Lernmaterial transformieren.

### Flashcards

- Basic Cards.
- Reverse Cards.
- bidirektionale Karten.
- Concept Cards.
- Descriptor Cards.
- Cloze-/Lückentextkarten.
- Multi-Line Cards.
- List-/Set-Karten.
- Multi-Step Cards.
- Image Occlusion.
- Multiple-Choice-Karten.
- aus Tabellen erzeugte Karten.
- Karten mit Codeblöcken.
- Eingabe von Antworten.
- Extra Card Detail.
- Hints.
- Card Clusters.
- AI-generierte Flashcards.
- Bulk-Änderung von Kartenrichtungen.

### Spaced Repetition

- zeitgesteuerte Wiederholungen.
- Priorisierung fälliger Karten.
- globaler Lern-Queue.
- dokumentbezogene Lern-Queues.
- Practice All.
- Practice All in Order.
- Spaced-Repetition-Modus.
- SM-2-ähnlicher Scheduler.
- FSRS-Unterstützung.
- benutzerdefinierte Scheduler.
- Reset der Scheduling-Historie.
- Prioritäten.
- Deaktivieren von Karten.
- Leech-Handling.

### Lernziele und Statistiken

- Lernziele.
- Streaks.
- Tages-/Wochenfortschritt.
- Flashcard Home.
- Statistiken.
- Dokumentbezogener Lernfortschritt.
- Prüfungstermine.
- Exam Scheduler.
- Exam Study Plan.
- Planung der Wiederholungsmenge bis zum Prüfungstermin.

### Reader

- PDF Reader.
- Annotation.
- Highlights.
- Notes neben dem Dokument.
- Highlights in Flashcards umwandeln.
- Image Occlusion direkt aus Dokumenten.
- weitere Dateien werden teilweise in PDF konvertiert.
- Web Reader.
- YouTube-/Video-Lernen.
- Audio-/Lecture-Workflows.

### Guided Learn

- PDF, Video oder Dokument in Lernplan zerlegen.
- automatische Section-Struktur.
- Zusammenfassungen.
- Flashcards.
- Quizzes.
- personalisierte Reihenfolge abhängig vom Vorwissen.
- AI Tutor.

### AI Tutor

- Fragen zum Dokument.
- Bezug auf hochgeladene Quellen.
- Suche in eigenen Notizen.
- Flashcards aus Antworten erzeugen.
- Study Guides erzeugen.
- Dokumente erzeugen.
- Quizfunktionen.

### Besonders interessante Ideen für SASD

- **Retention ist ein eigener Zustand** neben Completion und Mastery.
- Lernnotizen können optional Wiederholungsobjekte erzeugen.
- Ein Learning Manager muss nicht selbst ein vollständiges SRS enthalten; er kann aber Review-Bedarf und Wiederholungsstatus modellieren.
- Exam Date → dynamischer Lernplan ist eine starke Idee.
- „Kurs zu 100 % gesehen“ darf nicht mit „Wissen verfügbar“ gleichgesetzt werden.

### Quellen

- <https://help.remnote.com/en/collections/3370931-flashcards>
- <https://help.remnote.com/en/articles/6690975-learning-from-pdfs-and-files-with-the-remnote-reader>
- <https://help.remnote.com/en/articles/15724936-guided-learn-mode>

---

## 4.10 Class Central

### Einordnung

Class Central ist ein anbieterübergreifender Katalog und Aggregator für Online-Lernen. Es ist für SASD wichtig, weil der Learning Manager ebenfalls **anbieterneutral** sein soll.

### Funktionen

#### Kursaggregation

- Kurse aus vielen Plattformen aggregieren.
- Educational Videos erfassen.
- große Themenvielfalt.
- Anbieterübergreifende Suche.
- Universitäten, Provider und Themen zusammenführen.

#### Suche und Filter

- Suche nach Stichworten.
- Filter nach Dauer.
- Filter nach Niveau.
- Filter nach kostenlos/kostenpflichtig.
- Filter nach Zertifikat.
- Filter nach kostenlosem Zertifikat.
- Filter nach Sprache.
- thematische Navigation.

#### Ressourcentypen

- Einzelkurse.
- Videoangebote.
- Spezialisierungen.
- Professional Certificates.
- Programme.
- Microcredentials.
- Online Degrees.
- Playlists bzw. Bildungs-Video-Sammlungen.

#### Community und Bewertung

- Kursreviews schreiben.
- Reviews anderer Lernender lesen.
- Bewertungshilfe zur Kursauswahl.

#### Folgen und Updates

- Providern folgen.
- Universitäten folgen.
- Themen folgen.
- Kursen folgen.
- personalisierte Updates erhalten.

#### Planung und Tracking

- Lernaktivitäten planen.
- Lernen verfolgen.
- Listen und Sammlungen für Lernvorhaben.
- selbst zusammengestellte Lernpfade/Listen über verschiedene Anbieter.

### Besonders interessante Ideen für SASD

- Provider ist ein Attribut einer Ressource – nicht die primäre Organisationsstruktur.
- Suche und Filter über mehrere Anbieter müssen einheitlich funktionieren.
- Community-Bewertungen könnten langfristig als externe Qualitätsinformation eingebunden werden.
- Lernpfade dürfen Ressourcen unterschiedlicher Anbieter mischen.

### Quellen

- <https://www.classcentral.com/about>
- <https://www.classcentral.com/help/account-what-class-central>
- <https://www.classcentral.com/help/faq-what-courses-offered>

---

## 4.11 Khan Academy

### Einordnung

Khan Academy ist besonders wegen des **Mastery-Modells** relevant. Der zentrale Gedanke: Bearbeitung, Punkte und tatsächliche Beherrschung sind nicht identisch.

### Funktionen

#### Lerninhalte

- Kurse.
- Units/Lerneinheiten.
- Lektionen.
- Videos.
- Übungen.
- Quizzes.
- Unit Tests.
- Course Challenges.
- skillbezogene Aufgaben.

#### Mastery

- Course Mastery.
- Unit Mastery.
- Mastery-Prozent.
- Punktesystem für Kompetenzen.
- unterschiedliche Kompetenzzustände.
- Wiederholte Übung kann den Mastery-Status verändern.
- Course Challenge prüft Auswahl aus mehreren Skills.
- Mastery über Geräte hinweg verfolgen.

#### Fortschritt

- Lernfortschritt.
- Aufgabenfortschritt.
- Fortschritt innerhalb eines Kurses.
- skillbezogene Entwicklung.
- Leistungsübersichten.

#### Aufgaben und Unterricht

- Aufgaben zuweisen.
- Lehrer-/Coach-Sicht.
- Schülerfortschritt beobachten.
- Inhalte gezielt auswählen.
- Übungen und Tests.

#### Gamification/Motivation

- Punkte.
- Badges bzw. motivierende Elemente.
- Fortschrittsanzeigen.

### Besonders interessante Ideen für SASD

- **Mastery ≠ Completion.**
- Skills können unabhängig vom vollständigen Konsum eines Kurses bewertet werden.
- Wiederholungs- und Prüfungsergebnisse beeinflussen den Kompetenzstatus.
- Ein SASD-Skill sollte mehrere Evidenzquellen haben.

### Quelle

- <https://support.khanacademy.org/hc/de/articles/115002552631-Was-sind-Kurs-und-Lerneinheits-Mastery>

---

## 4.12 Obsidian

### Einordnung

Obsidian ist ein Local-first-PKM-System auf Markdown-Basis. Für SASD ist es ein Vorbild für **offene Daten, Erweiterbarkeit, flexible Views und langlebige Wissensnotizen**.

### Datenmodell und Vault

- lokale Markdown-Dateien.
- Vaults.
- Ordner.
- Properties/Frontmatter.
- interne Links.
- Backlinks.
- unlinked mentions.
- Tags.
- Attachments.
- offene Dateiformate.

### Editor und Notizen

- Markdown.
- Überschriften.
- Listen.
- Codeblöcke.
- Tabellen.
- Links.
- Einbettungen.
- Templates.
- Outline.
- Footnotes.
- Page Preview.
- Note Composer.
- Wort-/Zeichenzählung.

### Suche und Navigation

- Volltextsuche.
- Quick Switcher.
- Command Palette.
- File Explorer.
- Bookmarks.
- Tags View.
- Outgoing Links.
- Backlinks.
- Graph View.
- Workspaces.
- Random Note.

### Bases

- datenbankähnliche Views über Markdown-Dateien.
- Properties anzeigen und bearbeiten.
- filtern.
- sortieren.
- gruppieren.
- Formeln.
- Funktionen.
- Summaries.
- mehrere Views je Base.
- Table View.
- List View.
- Cards View.
- Map View über Plugin.
- Bases als `.base`-Datei.
- Bases in Markdown einbetten.
- YAML-basierte Definition.
- lokale Daten bleiben in Markdown.
- dynamische Ansichten statt Duplikation der Daten.

### Canvas

- visuelle freie Arbeitsfläche.
- Text Cards.
- Notes als Cards.
- Medien.
- PDFs.
- Web Pages.
- Verbindungen zwischen Cards.
- Gruppen.
- offene JSON-Canvas-Dateien.
- Drag-and-drop.

### Web Clipper

- Browser-Erweiterung.
- Webseiten speichern.
- Highlights.
- Reader-Modus.
- Seitenelemente auswählen.
- Templates.
- Variablen.
- Filter.
- Logik/Bedingungen/Schleifen.
- automatische Metadaten.
- Ziel-Vault und Zielordner.
- Natural-Language-Interpreter.
- lokale Speicherung.
- Open-Source-Clipper.

### Sync und Publishing

- Obsidian Sync.
- Gerätesynchronisation.
- Obsidian Publish.
- Website-/Wiki-/Dokumentations-Publishing.
- Public/Private Sites je Konfiguration.
- Nutzung externer Git-/Static-Site-Workflows möglich.

### Plugins und Erweiterbarkeit

- Core Plugins.
- Community Plugins.
- Themes.
- CSS Snippets.
- eigene Plugins mit TypeScript/CSS.
- CLI.
- Importer.
- offene Formate einzelner Komponenten.

### Weitere Core-Funktionen

- Audio Recorder.
- Daily Notes.
- File Recovery.
- Format Converter.
- Slides.
- Unique Note Creator.
- Web Viewer.

### Besonders interessante Ideen für SASD

- Local-first und offene Datenformate.
- Fachliches Datenmodell und Darstellung trennen.
- Smart Views dürfen dieselben Daten unterschiedlich zeigen.
- Erweiterbarkeit über Plugins/API sollte langfristig möglich sein.
- Wissensnotizen sollten exportierbar und nicht im proprietären Format gefangen sein.
- SASD muss kein Obsidian-Klon sein; eine **Obsidian-Verlinkung/Exportmöglichkeit** ist wahrscheinlich sinnvoller.

### Quellen

- <https://obsidian.md/help/>
- <https://obsidian.md/help/plugins>
- <https://obsidian.md/help/bases>
- <https://obsidian.md/help/Plugins/Canvas>
- <https://obsidian.md/help/web-clipper>

---

## 4.13 LinkedIn Learning

### Einordnung

LinkedIn Learning ist als Vorbild für **Career Goal → Role → Skills → Learning Plan** interessant.

### Funktionen

#### Content

- On-Demand-Videokurse.
- Learning Paths.
- fachliche und berufliche Themen.
- Übungsdateien bei vielen Kursen.
- Quiz-/Knowledge-Check-Funktionen je Inhalt.
- Kursabschluss und Zertifikate.

#### Learning Paths

- mehrere Kurse zu einem Ziel bündeln.
- strukturierte Reihenfolge.
- rollen- oder themenbezogene Pfade.
- Fortschrittsverfolgung über den Pfad.

#### Career Goals

- Karriereziel festlegen.
- Lernen mit beruflichen Zielen verbinden.
- Empfehlungen auf das Ziel ausrichten.

#### Career Paths / Role Guides

- Rollen erkunden.
- unternehmensspezifische Role Guides in Career Hub.
- Skills einer Rolle anzeigen.
- Rollenbeschreibung.
- Job Family.
- Level/Seniority.
- nächste mögliche Rolle erkunden.
- Next Role Explorer.
- Visualisierung möglicher Karrierewege.
- Empfehlungen auf Basis von Unternehmensrollen und LinkedIn-Daten.
- Zielrolle mit Learning verknüpfen.

#### Skills

- Skill-orientierte Inhalte.
- Skill-Informationen im Rollenprofil.
- Kompetenzorientierte Empfehlungen.
- Skill-Evaluations/-Assessments je Angebot/Funktion.
- Skill Development in Learning Plans.

#### Personalisierung

- Content Recommendations.
- rollenbezogene Empfehlungen.
- Karrierebezogene Empfehlungen.
- Learning Plans.

#### Enterprise

- organisationseigene Role Guides.
- Anpassung an Rollen-/Job-Frameworks.
- Integration in Career Hub.
- organisatorisches Skills- und Learning-Management.

### Besonders interessante Ideen für SASD

- Zielrollen sollten explizit modellierbar sein.
- Ein Lernplan kann aus der Differenz zwischen aktuellem Profil und Zielrolle entstehen.
- Rolle, Skill und Lernressource sind drei getrennte fachliche Objekte.
- Karriereorientierung sollte optional sein; SASD muss auch reine Interessens-/Projektziele unterstützen.

### Quellen

- <https://www.linkedin.com/help/learning/answer/a1429531>
- <https://www.linkedin.com/help/learning/answer/a10828013>

---

## 4.14 Moodle

### Einordnung

Moodle ist ein sehr großes Open-Source-LMS. Für SASD ist es weniger als UI-Vorbild, aber stark als **Domain-Referenz für Competency Frameworks, Learning Plans, Completion und formale Lernorganisation**.

### Kursverwaltung

- Kurse.
- Kursbereiche.
- Lernaktivitäten.
- Ressourcen.
- Gruppen.
- Einschreibungen.
- Rollen und Berechtigungen.
- Aufgaben.
- Tests.
- Foren.
- Workshops.
- externe Tools.
- Multimedia.
- Kalender.
- Gradebook.

### Activity Completion

- Abschlussbedingungen je Aktivität.
- manuelle Completion.
- automatische Completion abhängig von Bedingungen.
- Grundlage für Kursfortschritt.

### Course Completion

- Kriterien für Kursabschluss.
- Completion Tracking.
- Statusberichte.
- manuelle Bestätigung durch Lernenden oder berechtigte Rollen.
- Completion-Zeitpunkt.
- Reports.
- mehrere Kriterien kombinieren.

### Competencies

- Competency Frameworks.
- hierarchische Kompetenzmodelle.
- Kompetenzen Kursen zuordnen.
- Kompetenzen Aktivitäten zuordnen.
- kompetenzbasierte Bewertung.
- Evidenz/Grade rund um Kompetenzen.
- Kompetenzprofile.

### Learning Plans

- Learning Plan Templates.
- einem Plan mehrere Kompetenzen zuordnen.
- Lernpläne einzelnen Personen zuweisen.
- Lernpläne ganzen Cohorts zuweisen.
- Änderungen am Template auf zugeordnete Pläne übertragen.
- individuelle Lernplanansicht.
- Kompetenzstatus im Lernplan.

### Programme

- mehrere Kurse zu Programmen kombinieren.
- Sets und Subsets.
- Completion „all in order“.
- Completion „all in any order“.
- Completion „at least N“.
- Reihenfolge per Drag-and-drop.
- Program Completion auf Basis der Kursabschlüsse.

### Assessment und Bewertung

- Quizzes.
- Aufgaben.
- Gradebook.
- Advanced Grading.
- Peer Assessment.
- Self Assessment.
- Feedback.
- Workshops.

### Badges

- integrierte Badges.
- Open-Badges-Kompatibilität.
- Auszeichnung von Leistung und Teilnahme.

### Kollaboration

- Foren.
- Wikis.
- Workshops.
- Gruppen.
- kollaborative Publikations-/Lernaktivitäten.

### Integrationen

- LTI/External Tools.
- Plugins.
- externe Repositories.
- APIs/Webservices.
- große Plugin-Landschaft.

### Besonders interessante Ideen für SASD

- Competency Framework und Learning Plan sind getrennte Objekte.
- Completion-Kriterien dürfen aus mehreren Regeln bestehen.
- Lernpfade brauchen optional „in order“, „any order“ und Mindestanzahl-Logik.
- Evidence/Competency-Grade ist als Domain-Idee interessant.
- Die Moodle-Komplexität selbst sollte **nicht** unser Vorbild für V1 sein.

### Quellen

- <https://docs.moodle.org/500/en/Features>
- <https://docs.moodle.org/500/en/admin/tool/lp/learningplans>
- <https://docs.moodle.org/500/en/Programs_Configuration>
- <https://docs.moodle.org/500/en/Course_completion_report>

---

## 4.15 360Learning

### Einordnung

360Learning kombiniert LMS, LXP, Collaborative Learning und Skill Management. Für SASD ist es besonders interessant, weil Lernbedarf, Skill Gap, Lernpfade, Content und Expertise miteinander verbunden werden.

### Skills

- Skills Profiles.
- Assessment History.
- Skills Self-Assessment.
- Manager Validation.
- Assessment Requests.
- geplante Erinnerungen für Skill-Updates.
- Skill-Dashboard.
- Skill-Gap-Visualisierung.
- Skill Portfolio.
- Skills API.
- Import eigener Skill-Ontologien.
- Standard-Skill-Ontologie.
- AI-Skill-Tagging von Content.
- Identifikation interner Experten.
- Skillbasierte Suche.
- Skillbasierte Empfehlungen.

### Karriere und Rollen

- Career Profiles.
- Rollenverlauf.
- Zielrollen.
- Target Job Mapping.
- Job Framework.
- AI-basierte Rollenvorschläge.
- Vergleich Ist-/Soll-Skills.
- persönliche Wachstumsziele.
- Visualisierung der Kompetenzentwicklung.
- personalisierte Upskilling-Pläne.

### Lernpfade

- Paths.
- personalisierte Paths.
- Adaptive Learning Paths.
- Reihenfolge von Lerninhalten.
- Online Courses.
- Instructor-led Training.
- SCORM.
- xAPI.
- externe Content Provider.
- End-of-Course Recommendations.
- Empfehlungen für nächsten Kurs.

### Collaborative Learning

- interne Experten können Inhalte erstellen.
- Kursautoren/SMEs direkt einbinden.
- gemeinsames Erstellen und Verbessern von Kursen.
- In-Course Forums.
- Fragen an Kollegen und Experten.
- Feedback und Reactions.
- Learning Needs.
- Lernbedarf durch Nutzer erfassen.
- Lernbedarf upvoten.
- Experten für einen Lernbedarf vorschlagen.
- Content-Gaps erkennen.

### AI

- AI Content Builder.
- AI Companion.
- AI Coaching.
- AI Content Recommendations.
- Skill Tagging.
- generierte Skill-/Proficiency-Strukturen.
- Adaptive Learning.
- conversational Search/Guidance.
- Forum Translation.

### Suche und Discovery

- Advanced Search.
- Filter.
- Keyword Highlights.
- Search by Skill.
- Recommended Training.
- Carousels.
- Galleries.
- personalisierte Homepage.

### Delivery

- Self-paced Online Learning.
- ILT.
- VILT.
- Blended Learning.
- Cohorts.
- Kalenderintegration.
- Mobile Learning.
- Offline Mobile.
- Push Notifications.

### Integrationen

- Slack.
- Microsoft Teams.
- Salesforce.
- Outlook Calendar.
- APIs.
- HR-/Skills-Datenintegration.

### Analytics

- Course Dashboard.
- Path Statistics.
- Classroom Dashboard.
- Group Dashboard.
- Manager Dashboard.
- Mobile Manager Dashboard.
- Custom Completion Dashboard.
- Custom Reports.
- Export.
- geplante Reports per Mail.
- Skills Developed.
- zentrale Analytics.

### Besonders interessante Ideen für SASD

- **Learning Need** als eigenes Objekt ist sehr interessant.
- Nutzer sollten Lernbedarf erfassen können, bevor eine Ressource existiert.
- Skill Gap kann zu einem personalisierten Path führen.
- Content-Gap: „Für diesen Skill haben wir noch keine gute Ressource.“
- Skill Ontology muss importierbar/erweiterbar sein.
- Expertenhinweise könnten später optional sein.

### Quellen

- <https://360learning.com/product/skills/>
- <https://360learning.com/product/lxp/>
- <https://360learning.com/product/learning-management-system/>
- <https://support.360learning.com/hc/en-us/articles/360041650032-Create-a-learning-need>

---

## 4.16 Docebo

### Einordnung

Docebo ist ein Enterprise-LMS/LXP mit Learning Plans und Skill-Funktionen. Für SASD ist es hauptsächlich als Referenz für **strukturierte Lernpläne, Voraussetzungen, Kurszuweisung und Skill-Kataloge** interessant.

### Learning Plans

- Lernpläne als Sequenz von Kursen.
- E-Learning und ILT kombinieren.
- Titel, Code, Kurzbeschreibung, Beschreibung.
- Additional Fields.
- Published/Under Maintenance.
- Pflichtkurse.
- optionale Kurse.
- Reihenfolgen.
- Voraussetzungen zwischen Kursen.
- freie oder sequenzielle Bearbeitung.
- Zuweisung an Personen.
- Zuweisung an Gruppen/Branches.
- Self-enrollment über Catalogs.
- Bulk Enrollment.
- CSV-Enrollment.
- Learning-Plan-Eigenschaften.
- Filter, Suche und sortierbare Administrationslisten.
- Learning Plans in Channels/Catalogs je Konfiguration.
- Zertifikate/Zertifizierungen in Verbindung mit Plänen.

### Kursverwaltung

- E-Learning Courses.
- ILT.
- VILT.
- Sessions.
- Events.
- Attendance Tracking.
- Training Materials.
- Mehrsprachige Kurse.
- Kalenderinvites/RSVP.
- Content Center.

### Skill Management

- Skill Catalog.
- Skill Sets.
- Skill Profiles.
- Skills Lernenden zuordnen.
- Skills Content zuordnen.
- Skills für formelle und informelle Inhalte.
- personalisierte Content Recommendations.
- personalisierte Lernpfade.
- Skill Gap Tracking.
- externe Skill-/HRIS-Integration.

### Discovery und Suche

- Global Search.
- Suche nach Courses.
- Learning Plans.
- Training Material.
- informal Learning Assets.
- Questions & Answers.
- Visibility-basierte Resultate.

### Reporting

- Team-Dashboards.
- Kursfortschritt.
- Learning-Plan-Fortschritt.
- Completion Metrics.
- Manager-Ansichten.

### Rollen und Rechte

- Superadmin.
- Power Users.
- granulare Berechtigungen.
- Rechte zum View/Create/Edit von Learning Plans.
- getrennte Enrollment-Rechte.

### Besonders interessante Ideen für SASD

- Pflicht/Optional je Ressource im Path.
- Prerequisite-Beziehungen.
- „Published“ und „Draft/Under Maintenance“ für eigene Lernpfade.
- freie vs. sequenzielle Bearbeitung.
- zusätzliche Felder im Datenmodell für Erweiterbarkeit.
- nicht übernehmen: Enterprise-Enrollment- und Rollenkomplexität in V1.

### Quellen

- <https://help.docebo.com/hc/en-us/articles/360020083980-Creating-and-managing-learning-plans>
- <https://help.docebo.com/hc/en-us/sections/360005521799-Skills>
- <https://help.docebo.com/hc/en-us/articles/25135044544146-Best-practices-for-configuring-and-leveraging-skills-in-your-platform>

---

## 4.17 Raindrop.io

### Einordnung

Raindrop.io ist ein moderner Bookmark-Manager. Für SASD ist es ein wichtiges Vorbild für **schnelles Speichern, Sammlungen, Tags, Suche, Import und Datenpflege**.

### Erfassen

- Browser Extension.
- Web App.
- Desktop Apps.
- Mobile Apps.
- Share Sheet.
- URL manuell hinzufügen.
- Dateien hochladen.
- Tabs speichern.
- „Save all tabs“.
- schnelle Ein-Klick-Erfassung.

### Bookmark-Metadaten

- URL.
- Titel.
- Vorschau/Cover.
- Beschreibung/Excerpt.
- Note.
- Tags.
- Collection.
- Erstellzeit.
- automatische Vorschau.
- Cover-Auswahl.

### Collections

- Collections.
- Nested Collections.
- Unsorted.
- Sammlungsspezifische Organisation.
- visuelle Icons/Cover je nach Funktion/Plan.
- verschiedene Layouts.

### Tags

- mehrere Tags.
- Tag Autocomplete.
- Tag-Organisation.
- AI-Vorschläge für Tags in Pro-Funktionen.
- Tags beim Import erhalten.

### Suche

- Volltextsuche.
- AI Semantic Search.
- mehrsprachige semantische Suche.
- Suche nach Titel.
- Excerpt.
- Notes.
- URL-Teilen.
- Datum.
- exakten Phrasen.
- Ausschlussbedingungen.
- OR-Bedingungen.
- Teilwortsuche.
- Suchhervorhebungen.
- Recent Searches.
- Suchergebnisse nach Relevanz.

### Highlights und Annotation

- Highlights auf Webseiten.
- Annotationen/Notizen.
- Import von Highlights aus unterstützten Formaten.
- Markierungen als Teil der gespeicherten Ressource.

### Archivierung und Wartung

- Permanent Copies je Plan/Funktion.
- Prüfung auf Broken Links.
- Duplikaterkennung.
- AI-gestützte bzw. manuelle Bereinigung.
- Stella kann bei Organisation, Duplikaten, Broken Links und Strukturvorschlägen helfen.

### AI / Stella

- Fragen an eigene Bookmark-Sammlung.
- Artikel vergleichen.
- Relevanz beurteilen.
- Organisation vorschlagen.
- Collections vorschlagen.
- Tags vorschlagen.
- Duplikate finden.
- Broken Links finden.
- vergessene Inhalte wiederentdecken.
- thematische Muster finden.
- Leselisten erzeugen.
- Änderungen werden als Vorschlag angezeigt und vom Nutzer bestätigt.

### Import/Export

- Import aus Browsern.
- HTML.
- CSV.
- TXT.
- JSON.
- ENEX.
- Import aus diversen Bookmark-/Read-later-/PKM-Systemen.
- Duplikate beim Import automatisch überspringen.
- Ordnerstruktur übernehmen.
- Tags/Notizen/Highlights je Format übernehmen.
- Export/Backup-Funktionen.

### Integrationen

- verschiedene Apps/Services.
- Browser Extension.
- Mobile Share.
- Cross-Device-Nutzung.

### Besonders interessante Ideen für SASD

- **Capture muss schneller sein als Klassifizieren.**
- Duplicate Detection anhand URL/Identifier gehört früh ins Produkt.
- Unsorted/Inbox ist zwingend sinnvoll.
- Import muss Metadaten möglichst bewahren.
- AI-Organisation als **Vorschlag mit Nutzerfreigabe** ist ein sehr gutes Muster.
- Broken-Link-Erkennung ist für langfristige Lernpfade wichtig.

### Quellen

- <https://help.raindrop.io/>
- <https://help.raindrop.io/bookmarks>
- <https://help.raindrop.io/import>
- <https://help.raindrop.io/stella>

---

## 4.18 Capacities

### Einordnung

Capacities ist ein objektorientiertes PKM-System. Es ist für den SASD Learning Manager besonders interessant, weil es Inhalte nicht primär als Dateien oder Ordner, sondern als **typisierte Objekte mit Eigenschaften, Beziehungen, Collections, Tags und Queries** behandelt.

### Object Types

- Page.
- Tag.
- Image.
- Weblink.
- weitere Built-in Types.
- benutzerdefinierte Content Types.
- je Typ eigene Properties.
- je Typ eigene Dashboards.
- Templates.
- strukturierte Properties.
- Labels.

### Objektorientierte Organisation

- jedes Wissenselement ist ein Objekt.
- Objekte verlinken.
- Backlinks.
- Reuse statt Duplikation.
- Object Dashboards.
- typabhängige Darstellungen.

### Collections

- manuelle Teilmengen eines Object Types.
- ein Objekt kann mehreren Collections angehören.
- keine Verschachtelung wie klassische Ordner.
- Bulk Actions.
- Collections in Seiten einbetten.
- kuratierte Zusammenstellungen.

### Tags

- Tags über mehrere Object Types.
- Tags an ganzen Objekten.
- Tags an einzelnen Blöcken.
- Tag Pages.
- unterschiedliche Views.
- Filter.
- Sorting.
- Related Tags.
- Tags als thematische Querverbindung.
- Tags und Collections kombinierbar.

### Queries

- Object Type Queries.
- Search Queries.
- Tag Queries.
- Variable Queries.
- Filter nach Properties.
- Filter nach Tags.
- Filter nach Collections.
- Filter nach Backlinks/Kontext.
- Sortierung.
- Gruppierung.
- Result-Limit.
- Randomisierung.
- dynamische, automatisch aktualisierte Views.
- Query selbst ist ein Objekt.
- Queries einbetten.
- Queries in Templates verwenden.
- Dashboards aus Queries.

### Suche

- Command Palette.
- Content Search.
- Find in Page.
- Extended Search.
- Filter nach Object Type.
- Filter nach Tags.
- Object- oder Block-Suche.
- exakte Suche.
- semantische Suche.
- Auto-Modus.
- Saved Search als Query.

### Web Capture

- eigene Browser Extension.
- URL erfassen.
- Titel erfassen.
- Cover Image erfassen.
- Ziel-Space auswählen.
- Tags beim Capture.
- Notes beim Capture.
- Markdown-Formatierung.
- Links/Objekte aus Capture-Notiz heraus erzeugen.
- zusätzliche Integrationen mit Web Highlights.
- Readwise-Integration.

### Reading/Highlight Integration

- importierte Highlights.
- spezielles Format zur Trennung fremder Quelle und eigener Gedanken.
- Readwise als Quelle für Multi-Media-Highlight-Workflows.

### Views und Dashboards

- kürzlich geöffnet.
- untagged.
- nicht in Collection.
- ohne Backlinks.
- Collections.
- Queries.
- benutzerdefinierte Dashboard-Bereiche.
- verschiedene Ansichten je Object Type.

### Weitere PKM-Funktionen

- Daily Notes.
- Meeting Notes.
- Tasks.
- Calendar Integration je Plan/Funktion.
- Templates.
- Spaces.

### Besonders interessante Ideen für SASD

- **Resource, Skill, Learning Path, Provider, Evidence, Note und Goal sollten typisierte Domain-Objekte sein.**
- Tags sollten Querschnittsthemen abbilden, nicht Objekttypen ersetzen.
- Collections eignen sich für manuell kuratierte Teilmengen.
- Queries eignen sich für dynamische Smart Views.
- Objektorientierung des UI kann unserem relationalen Domain Model sehr nahekommen.

### Quellen

- <https://docs.capacities.io/reference>
- <https://docs.capacities.io/reference/collections>
- <https://docs.capacities.io/reference/tags>
- <https://docs.capacities.io/reference/queries>
- <https://docs.capacities.io/reference/web-extension>

---

## 4.19 Notion

### Einordnung

Notion ist kein spezieller Learning Manager, aber ein gutes Vorbild für **flexible relationale Datenstrukturen, unterschiedliche Views und schnell anpassbare Workflows**.

### Seiten und Inhalte

- Pages.
- Nested Pages.
- Text.
- Überschriften.
- Listen.
- Tabellen.
- Medien.
- Dateien.
- Embeds.
- Code.
- Callouts.
- Kommentare.
- Links.

### Datenbanken

- jedes Datenbankelement ist eine Seite.
- Properties.
- Text.
- Number.
- Select.
- Multi-Select.
- Status.
- Date.
- Person.
- URL.
- Files.
- Relations.
- Rollups.
- Formulas.
- weitere Property-Typen.
- Database Templates.
- Sub-Items.
- Dependencies.

### Views

- Table.
- Board/Kanban.
- List.
- Calendar.
- Timeline.
- Gallery.
- unterschiedliche Filter/Sortierungen je View.
- dieselben Datensätze in mehreren Views.
- Gruppen.
- versteckte/sichtbare Properties je View.

### Relations

- Beziehungen zwischen Datenbanken.
- bidirektionale Beziehungen.
- Self-Relations.
- eigene Properties je Richtung.
- z. B. Next/Previous.
- Darstellung verknüpfter Properties.

### Rollups

- Werte aus verknüpften Datensätzen aggregieren.
- Originalwerte.
- Unique Values.
- Count.
- Count Unique.
- Empty/Not Empty.
- Percent Empty/Not Empty.
- Sum.
- Average.
- Median.
- Min.
- Max.
- Range.
- Earliest/Latest Date.
- Date Range.

### Formulas

- berechnete Properties.
- Logik auf Datenbankfeldern.
- abgeleitete Status-/Kennzahlen.

### Web Clipper

- Webseiten in Workspace speichern.
- Zielseite/Datenbank wählen.
- neue Linkdatenbank erzeugen.
- Original-URL automatisch speichern.
- geclippte Seite nachträglich um Tags/Properties/Kommentare ergänzen.
- Mobile Share Sheet.
- Bilder/lokale Inhalte je Plattform.

### Zusammenarbeit

- Workspaces.
- Teilen.
- Kommentare.
- Mentions.
- gemeinsames Bearbeiten.
- Rechte.
- Teamspaces je Plan.

### Automatisierung/AI/Integrationen

- Notion AI.
- Datenbankautomationen je Plan.
- API.
- Integrationen mit anderen Diensten.
- Templates.

### Besonders interessante Ideen für SASD

- dieselben Domain-Daten in vielen Views darstellen.
- Self-Relations sind Vorbild für „Prerequisite“, „Next“, „Alternative“, „Deepens“.
- Rollups können Fortschritt eines Learning Paths aus seinen Bestandteilen berechnen.
- flexibel definierbare Properties sind langfristig attraktiv.
- Nicht übernehmen: völlige Schemafreiheit, wenn sie die fachliche Konsistenz gefährdet.

### Quellen

- <https://www.notion.com/help/category/databases>
- <https://www.notion.com/help/relations-and-rollups>
- <https://www.notion.com/help/web-clipper>

---

## 4.20 Linkwarden

### Einordnung

Linkwarden ist ein Open-Source-, Self-hosted- und kollaborativer Bookmark Manager mit starker **Webarchivierung**.

### Capture und Speicherung

- Links speichern.
- Browser Extension.
- Mobile Apps.
- iOS Share/Shortcut.
- Image Upload.
- PDF Upload.
- Import aus SingleFile.
- Browser Sync über floccus.

### Archivierung

- Screenshot automatisch speichern.
- PDF automatisch speichern.
- Single-HTML-Version einer Webseite.
- optionale Wayback-Machine-Sicherung.
- Originalinhalt bleibt verfügbar, selbst wenn die URL verschwindet.

### Lesen und Annotation

- Reader View.
- Highlights.
- Textannotation.
- eigene Gedanken/Notizen.
- archivierte Seite im Lesemodus verwenden.

### Organisation

- Collections.
- Subcollections.
- Name.
- Description.
- mehrere Tags.
- Custom Icons.
- Bulk Actions.

### Suche

- Volltextsuche.
- Filter.
- Sortierung.
- Suche über gespeicherte Links und Inhalte.

### AI

- optionale lokale AI-Tagging-Funktion.
- automatisches Tagging anhand Inhalt.

### Zusammenarbeit

- Collections gemeinsam pflegen.
- Member Permissions.
- öffentliche Freigabe von Links.
- öffentliche Freigabe archivierter Formate.
- Benutzerverwaltung.

### Mobile/Offline

- native iOS-App.
- native Android-App.
- Share Sheet.
- Cached Data für Offline-Lesen.

### Integration und Betrieb

- Self-hosted.
- Cloud-Angebot.
- API Keys.
- SSO.
- Internationalisierung.
- RSS Feed Subscription.
- Dark/Light Mode.

### Besonders interessante Ideen für SASD

- Für wichtige Lernressourcen kann optional ein **Preservation Status** existieren.
- Archivformat und Original-URL sollten getrennt gespeichert werden.
- Link Rot muss langfristig behandelbar sein.
- Annotation und Archivierung sind getrennte Funktionsdimensionen.
- AGPL-Code nicht einfach übernehmen; funktionale Inspiration ist unproblematisch, Code-Lizenz wäre gesondert zu prüfen.

### Quellen

- <https://github.com/linkwarden/linkwarden>
- <https://github.com/linkwarden/linkwarden/blob/main/README.md>

---

## 4.21 linkding

### Einordnung

linkding ist ein bewusst minimalistischer Self-hosted Bookmark Manager. Für SASD ist er ein wichtiges **Anti-Overengineering-Vorbild**.

### Funktionen

- sehr schlanke, lesbare UI.
- Self-hosting.
- Docker-orientierte Installation.
- Bookmark-Verwaltung.
- Tags.
- Markdown-Notes.
- Read-it-later.
- Bulk Editing.
- Teilen mit anderen Benutzern/Gästen.
- automatische Seitentitel.
- automatische Beschreibungen.
- automatische Favicons.
- lokale HTML-Archivierung.
- Internet-Archive-/Wayback-Sicherung.
- Import im Netscape-HTML-Format.
- Export im Netscape-HTML-Format.
- PWA.
- Browser Extensions für Firefox und Chrome.
- Bookmarklet.
- OIDC-SSO.
- Authentication Proxy Support.
- REST API.
- Admin Panel.
- Benutzer-Self-Service.
- Zugriff auf Rohdaten.
- Community Clients und Integrationen.
- SQLite/PostgreSQL-Unterstützung je Deployment.
- optionales Nutzungs-/Most-used-Tracking in aktuellen Entwicklungen.

### Besonders interessante Ideen für SASD

- Kernworkflows müssen ohne AI funktionieren.
- eine gute V1 darf klein sein.
- Tags + Notes + Search + API können bereits sehr wertvoll sein.
- Import/Export sollte früh bedacht werden.
- Self-hosting muss nicht automatisch eine komplexe Infrastruktur erfordern.

### Quelle

- <https://github.com/sissbruecker/linkding>

---

## 4.22 LearnAwesome

### Einordnung

LearnAwesome ist konzeptionell besonders nahe an unserer Idee: **Lernressourcen werden nach Topic, Format und Schwierigkeit organisiert und in Lernpfaden verwendet**. Das untersuchte ältere Repository ist ausdrücklich deprecated, daher dient das Projekt primär als Ideenquelle.

### Funktionen/Konzepte

- Lernressourcen kuratieren.
- Organisation nach Topics.
- Organisation nach Formats.
- Organisation nach Difficulty.
- Learning Map.
- Skills/Topics als Struktur.
- Ressourcen-Metadaten.
- Expert Reviews.
- Metadata Tags.
- optimale bzw. empfohlene Lernpfade als Zielkonzept.
- Community-Kuration.
- Social-Network-Aspekt für lebenslang Lernende.
- Universal Learning Map als konzeptionelles Ziel.
- Benutzerkonten.
- Beiträge/Community.
- GraphQL-API im älteren Projekt.
- Open-Source-Entwicklung.

### Besonders interessante Ideen für SASD

- Das Produkt bestätigt, dass **Resource Library + Topic Graph + Difficulty + Learning Path** eine eigenständige Produktidee ist.
- Difficulty gehört als Metadatum an Resource bzw. Path Node.
- Reviews/Qualitätsurteile können langfristig die Ressourcenauswahl verbessern.
- Ein universeller Lern-Graph ist faszinierend, für V1 aber viel zu groß.

### Risiken/Status

- untersuchtes Repository ist deprecated.
- nicht als technische Basis verwenden.
- Funktionsideen übernehmen, Implementierung unabhängig gestalten.

### Quelle

- <https://github.com/learn-awesome/learn>

---

## 4.23 Anki

### Einordnung

Anki ist ein etabliertes Spaced-Repetition-System. Für SASD ist es weniger ein UI-Vorbild als ein Referenzsystem für **Retention, Scheduling und gezielte Wiederholung**.

### Notes und Cards

- Notes mit Feldern.
- aus einer Note können mehrere Cards entstehen.
- unterschiedliche Note Types.
- Card Templates.
- Front/Back.
- HTML/CSS-basierte Kartendarstellung.
- Bilder.
- Audio.
- Video.
- LaTeX je Setup.
- Felder zur strukturierten Klassifikation.

### Decks

- Decks.
- Subdecks.
- Deck Presets.
- tägliche Limits.
- getrennte Lern-/Review-Queues.

### Tags und Flags

- mehrere Tags je Note.
- hierarchische Tags.
- Tags umbenennen.
- Tags suchen.
- Flags je Card.
- farbige Flags.
- „Marked“-Status.
- Tags/Flags als Such- und Filterkriterien.

### Browser/Suche

- Card Browser.
- Notes Mode.
- Cards Mode.
- komplexe Suchsyntax.
- Suche nach Deck.
- Tags.
- Flags.
- Feldern.
- Review-Status.
- Fälligkeit.
- Lapses.
- Bulk Actions.
- Export.

### Spaced Repetition

- klassische Anki-Scheduling-Logik.
- FSRS.
- Optimierung der Wiederholungsintervalle.
- gewünschte Retention je Konfiguration.
- New/Learning/Review/Relearning-Zustände.
- Again/Hard/Good/Easy.
- Scheduling Presets.
- Tageslimits.
- Lernschritte.
- Relearning.

### Filtered Decks

- temporäre Decks auf Basis von Suchabfragen.
- Cramming.
- Prüfungsvorbereitung.
- bestimmte Tags lernen.
- vergessene Karten erneut lernen.
- Backlog aufarbeiten.
- Review Ahead.
- mehrere Filter.
- Limit und Reihenfolge.

### Statistiken

- Tagesstatistik.
- Review-Zahlen.
- Erfolgsquote.
- Lern-/Review-/Relearning-Verteilung.
- langfristige Graphen.
- Deck-Statistiken.
- Collection-Statistiken.
- Zeitraumfilter.
- Export/Save Statistics as PDF.
- Review-Historie.
- direkter Zugriff auf SQLite-Daten für eigene Analysen.

### Medien

- Bilder.
- Audio.
- Video.
- Media Sync.
- Media Check.
- Dateiverwaltung.

### Synchronisation

- AnkiWeb.
- Geräte-Synchronisation.
- Media Sync.
- Desktop/Mobile/Web-Ökosystem.

### Erweiterbarkeit

- Add-ons.
- große Community.
- eigene Add-ons.
- zusätzliche Scheduler/Statistiken/Importfunktionen über Erweiterungen.

### Besonders interessante Ideen für SASD

- Ein Knowledge Item und seine Review-Schedule sind getrennte Objekte.
- Retention kann ein gewünschtes Zielniveau besitzen.
- dynamische „Review Queues“ sind interessanter als starre Wiederholungslisten.
- SASD sollte Anki wahrscheinlich **integrieren/exportieren**, nicht neu implementieren.
- Der Learning Manager kann dennoch speichern: „Review fällig“, „letzte Wiederholung“, „Retention Confidence“.

### Quellen

- <https://docs.ankiweb.net/deck-options>
- <https://docs.ankiweb.net/filtered-decks.html>
- <https://docs.ankiweb.net/stats.html>
- <https://docs.ankiweb.net/browsing.html>
- <https://docs.ankiweb.net/addons.html>

---

# 5. Funktionslandkarte über alle Produkte

Aus der Analyse lassen sich folgende Funktionsdomänen ableiten.

| Funktionsdomäne | Wichtige Vorbilder |
|---|---|
| Ziele / Career Goals | LinkedIn Learning, Degreed, 360Learning |
| Rollenprofile | LinkedIn Learning, Degreed, Pluralsight, 360Learning |
| Competency Frameworks | Moodle, Degreed, 360Learning, Docebo |
| Skill Profile | Degreed, Pluralsight, 360Learning, Docebo |
| Skill Gap | Degreed, Pluralsight, roadmap.sh Teams, 360Learning |
| Learning Need | 360Learning |
| Learning Paths | roadmap.sh, Degreed, O’Reilly, Pluralsight, Docebo |
| Voraussetzungen | roadmap.sh, Docebo, Notion-Self-Relations |
| gemischte Ressourcen | O’Reilly, Degreed, Class Central |
| Provider-unabhängige Ressourcen | Class Central, SASD-eigener Zielansatz |
| Resource Library | Zotero, Karakeep, Raindrop |
| kanonische Ressource | Zotero, Capacities |
| Collections | Zotero, Raindrop, Capacities |
| Tags | Zotero, Raindrop, Capacities |
| Smart Views / Queries | Zotero Saved Searches, Readwise Filtered Views, Capacities Queries, Obsidian Bases |
| Inbox / Quick Capture | Karakeep, Raindrop, Heptabase |
| Browser Capture | Karakeep, Raindrop, Obsidian, Notion, Capacities, Linkwarden |
| automatische Metadaten | Karakeep, Raindrop, linkding |
| Duplikaterkennung | Raindrop, Zotero |
| Link Rot / Archivierung | Linkwarden, Karakeep, linkding, Raindrop |
| Volltextsuche | Zotero, Karakeep, Linkwarden, Obsidian |
| semantische Suche | Karakeep, Raindrop, Capacities, Heptabase/MCP |
| PDF-Arbeit | Zotero, Readwise, RemNote, Heptabase |
| Video/Transcript | Readwise, Heptabase, RemNote |
| Annotationen | Zotero, Readwise, RemNote, Linkwarden |
| Knowledge Notes | Obsidian, Heptabase, Capacities, RemNote |
| Visual Knowledge Map | roadmap.sh, Heptabase, Obsidian Canvas |
| Fortschritt | O’Reilly, Pluralsight, roadmap.sh, Moodle |
| Completion | Moodle, O’Reilly, Pluralsight, Docebo |
| Mastery | Khan Academy |
| Assessments | Pluralsight, Degreed, Moodle, 360Learning |
| praktische Evidence | Pluralsight Labs, O’Reilly Labs |
| Badges / Credentials | O’Reilly, Moodle, Degreed |
| Retention | RemNote, Anki |
| Spaced Repetition | Anki, RemNote |
| Prüfungstermin-orientierte Planung | RemNote |
| Collaboration | Degreed, 360Learning, Moodle, Heptabase, Linkwarden |
| Reporting | Degreed, Pluralsight, 360Learning, Docebo, Moodle |
| API / Erweiterbarkeit | Zotero, Karakeep, linkding, Linkwarden, Moodle, Capacities |
| AI-Unterstützung | Degreed, roadmap.sh, Karakeep, O’Reilly, Heptabase, RemNote, 360Learning, Raindrop |
| Local-first / offene Daten | Obsidian, Zotero teilweise, Anki lokal, linkding self-hosted |
| Self-hosting | Karakeep, Linkwarden, linkding, Moodle |
| Mobile/Offline | Readwise, Heptabase, RemNote, 360Learning, Linkwarden, O’Reilly |

---

# 6. Gesamtfunktionskatalog, den die Vorbilder nahelegen

Dieser Abschnitt ist **noch kein Lastenheft**. Er zeigt lediglich, welche Produktideen aus der Recherche grundsätzlich für einen Learning Manager relevant sein können.

## 6.1 Ziele

Mögliche Funktionen:

- Lernziel anlegen.
- Karriereziel anlegen.
- Zielrolle anlegen.
- Projektziel anlegen.
- Zertifizierungsziel anlegen.
- Interessensziel anlegen.
- Ziele priorisieren.
- Zieltermine.
- Zielstatus.
- Ziel mit Skills verknüpfen.
- Ziel mit Learning Paths verknüpfen.

## 6.2 Skills und Kompetenzen

- Kompetenzbereiche.
- Skills.
- Skill-Hierarchien.
- Skill-Level.
- Soll-Level.
- Ist-Level.
- Skill Gap.
- Selbstbewertung.
- Fremdbewertung optional.
- Assessment-Ergebnisse.
- Evidence.
- Verlauf der Skillentwicklung.
- Skill-Relationen.
- Required/Recommended Skills.
- Rolle-zu-Skill-Mapping.

## 6.3 Learning Paths

- Roadmap.
- Module.
- Topics.
- Skills.
- Nodes.
- Reihenfolge.
- Abhängigkeiten.
- Voraussetzungen.
- Pflicht/Optional.
- Alternative Ressourcen.
- Parallelpfade.
- „All in order“.
- „All in any order“.
- „At least N“.
- Versionierung von Paths.
- Draft/Published/Archived.
- persönliche Kopie einer Vorlage.
- Progress auf Node-/Module-/Path-Ebene.

## 6.4 Ressourcen

Mögliche Typen:

- Course
- Video Course
- einzelnes Video
- Playlist
- Book
- Audiobook
- Article
- Web Page
- PDF
- Paper
- Documentation
- RFC/Standard
- Podcast
- Lab
- Sandbox
- Practice Exam
- Certification
- Event
- Conference
- Tutorial
- Repository
- eigenes Lab
- eigenes Projekt
- Präsenzkurs

Metadaten:

- Titel.
- Provider.
- Autor/Trainer.
- Publisher.
- URL.
- lokale Datei.
- Original-URL.
- Archiv-URL.
- ISBN/DOI je Typ.
- Sprache.
- Dauer.
- Schwierigkeitsgrad.
- Veröffentlichungsdatum.
- Aktualität.
- Version.
- Beschreibung.
- Tags.
- Qualität.
- Priorität.
- Kosten.
- Zugriffsmodell/Subscription.
- Status.
- geschätzter Aufwand.
- tatsächlicher Aufwand.

## 6.5 Resource Capture

- URL per Paste.
- Browser Extension.
- Share Sheet.
- Inbox.
- automatische Metadaten.
- Provider-Erkennung.
- Ressourcentyp-Erkennung.
- Duplikaterkennung.
- Tags vorschlagen.
- Skill-Zuordnung vorschlagen.
- Learning-Path-Zuordnung vorschlagen.
- später klassifizieren statt Capture zu blockieren.

## 6.6 Learning Activity

- Planned.
- Queued.
- Started.
- Paused.
- Completed.
- Abandoned.
- Superseded.
- Fortschrittsprozent.
- Startdatum.
- Abschlussdatum.
- Lernzeit.
- Sessions.
- Resume Position.
- nächste Aktion.
- Review fällig.
- Notizen während einer Session.

## 6.7 Wissen

- Notizen.
- Highlights.
- Zusammenfassungen.
- Commands/Cheatsheets.
- Code Snippets.
- Fragen.
- Lessons Learned.
- eigene Erklärungen.
- Verknüpfung zurück zur Quelle.
- Verknüpfung zu Skill.
- Wissensobjekte in mehreren Lernpfaden wiederverwenden.
- Export zu Markdown/Obsidian.
- optional Flashcard-Export.

## 6.8 Evidence und Mastery

Evidence-Typen:

- Course Completed.
- Quiz.
- Assessment.
- Practice Exam.
- Certification.
- Lab Completed.
- eigenes Projekt.
- praktische Nutzung.
- Dokumentation erstellt.
- Vortrag gehalten.
- Code/Repository.
- Self Assessment.
- Peer/Manager Assessment.
- Work Experience.

Mastery sollte separat von Completion behandelt werden.

## 6.9 Retention

- letzte Wiederholung.
- nächste Wiederholung.
- Confidence.
- Review Status.
- veraltetes Wissen markieren.
- Skill Refresh.
- Export nach Anki/RemNote.
- optional eigenes leichtgewichtiges Review-System.

## 6.10 Suche und Views

- Volltext.
- strukturierte Filter.
- Tags.
- Provider.
- Ressourcentyp.
- Skill.
- Learning Path.
- Status.
- Priorität.
- Dauer.
- Sprache.
- Schwierigkeit.
- gespeicherte Suche.
- Smart Views.
- semantische Suche optional.
- „Was habe ich zu Thema X?“.
- „Was sollte ich als Nächstes lernen?“.
- „Welche gespeicherten Ressourcen habe ich nie angefangen?“.
- „Welche Skills sind für Zielrolle X noch offen?“.

## 6.11 Wartung der Resource Library

- Duplikate.
- Broken Links.
- veraltete Ressourcen.
- archivierte Ressourcen.
- Nachfolger/neuere Edition.
- Resource superseded by.
- Alternative to.
- Overlaps with.
- Deepens.
- Refreshes.
- Requires.
- Recommended before/after.

---

# 7. Besonders wertvolle Designmuster

## 7.1 Kanonische Ressource – Zotero-Prinzip

Eine Ressource wird **einmal** gespeichert.

Beispiel:

```text
Resource R-00427
  Titel: Linux Performance Optimization
  Provider: O'Reilly
```

Sie kann gleichzeitig vorkommen in:

```text
Linux Performance
Red Hat Administration
Troubleshooting
EX442 Preparation
```

Es entstehen keine vier Kopien.

---

## 7.2 Lernpfad als Skill-Landkarte – roadmap.sh-Prinzip

Nicht:

```text
Video 1
Video 2
Video 3
Video 4
```

sondern:

```text
Linux Performance
├── CPU
│   ├── Scheduler
│   ├── Load Average
│   ├── perf
│   └── Flame Graphs
├── Memory
│   ├── VM
│   ├── Page Cache
│   ├── NUMA
│   └── OOM
├── Storage
└── Network
```

Ressourcen hängen an Skills/Topics und nicht umgekehrt.

---

## 7.3 Skill Gap – Degreed/Pluralsight/360Learning-Prinzip

```text
Zielrolle
      ↓
benötigte Skills
      ↓
aktuelles Skill-Profil
      ↓
Gap
      ↓
Learning Path / Ressourcen
```

---

## 7.4 Completion ≠ Mastery ≠ Retention

Drei getrennte Achsen:

```text
Resource Progress:
  Kurs zu 100 % bearbeitet

Skill Mastery:
  Linux CPU Performance 3/5

Retention:
  seit 8 Monaten nicht wiederholt
```

---

## 7.5 Inbox First – Karakeep/Raindrop-Prinzip

Eine neue Ressource soll in Sekunden erfasst werden können.

```text
URL → Inbox
```

Metadaten und fachliche Einordnung können später folgen.

---

## 7.6 Knowledge Artifact – Readwise/Heptabase/Obsidian-Prinzip

Eine Quelle und die daraus entstandene Erkenntnis sind **nicht dasselbe Objekt**.

```text
Resource
  ↓
Highlight
  ↓
Note
  ↓
Knowledge Artifact
  ↓
Skill Evidence / Review
```

---

## 7.7 Evidence Based Mastery

Ein Skill-Level sollte nicht ausschließlich aus einer Selbsteinschätzung oder einem Kursabschluss entstehen.

Mögliche Evidenz:

```text
Assessment
Lab
Project
Certificate
Course
Work Experience
Presentation
Documentation
Self Assessment
Peer Review
```

---

# 8. Funktionen, die wir bewusst nicht sofort nachbauen sollten

Aus den Vorbildern ergeben sich viele attraktive Funktionen. Für eine erste Version wären einige davon jedoch gefährlich, weil sie den Produktkern verwässern.

Nicht als V1-Kern:

- kompletter PDF-Reader,
- eigener Video-Player,
- komplette YouTube-Transcript-Engine,
- vollwertiges Spaced-Repetition-System,
- LMS mit Lehrern/Klassen/Schülern,
- HR-/Talentmanagement,
- Enterprise-Rollenmodell,
- SCORM-Authoring,
- eigener Kurseditor,
- Videohosting,
- Webarchivierung in der Tiefe von Linkwarden,
- KI-basierte automatische Skill-Bewertung,
- globaler Community-Marktplatz,
- universelle Learning Map des gesamten Wissens,
- Zertifizierungsplattform,
- eigene Prüfungsengine,
- vollständiger Obsidian-/Notion-Ersatz.

Diese Funktionen können später integriert, verlinkt oder über Plugins/API angebunden werden.

---

# 9. Vorläufige Priorisierung der Vorbilder

## Priorität A – intensiv als Referenz verwenden

1. **Degreed** – Skills, Goals, Paths
2. **roadmap.sh** – visuelle Lernlandkarten
3. **Zotero** – kanonisches Resource Model
4. **Karakeep** – Inbox/Capture/Search
5. **Readwise Reader** – Resource → Knowledge
6. **Pluralsight** – Assessment → Gap → Path
7. **O’Reilly Learning** – Multi-Format-Learning
8. **Heptabase** – visuelle Wissensstruktur
9. **RemNote** – Retention
10. **Class Central** – Provider-Unabhängigkeit
11. **Khan Academy** – Mastery
12. **Obsidian** – Local-first/Open Data

## Priorität B – gezielt für einzelne Funktionen

13. LinkedIn Learning – Career Goal / Role Guide
14. Moodle – Competency/Completion Domain Model
15. 360Learning – Learning Needs / Skill Gap / Collaborative Learning
16. Docebo – Learning Plan Rules
17. Raindrop.io – Bookmark UX / Capture / Maintenance
18. Capacities – Object-oriented PKM
19. Notion – Relations / Views / Rollups
20. Linkwarden – Preservation
21. linkding – Minimalismus
22. LearnAwesome – Topic/Resource/Learning-Map-Konzept
23. Anki – Spaced Repetition / Retention

---

# 10. Strategische Schlussfolgerung

Die Recherche bestätigt, dass der SASD Learning Manager **nicht als Bookmark-Manager** gedacht werden sollte.

Eine passendere fachliche Beschreibung wäre:

> **Der SASD Learning Manager ist ein persönliches, anbieterunabhängiges Learning-Portfolio- und Competency-Management-System. Er verbindet Lernziele und Kompetenzen mit strukturierten Lernpfaden und beliebigen externen Lernressourcen, dokumentiert Lernaktivität und Kompetenzentwicklung und unterstützt dabei, aus einer großen Menge verfügbarer Inhalte eine zielgerichtete Weiterbildung zu machen.**

Der interessante Produktkern entsteht gerade aus der Kombination von Funktionen, die heute auf verschiedene Systeme verteilt sind:

```text
Degreed / 360Learning
    Skills & Goals
          │
          ▼
roadmap.sh / Docebo
    Learning Paths
          │
          ▼
Zotero / Karakeep / Raindrop
    Resource Library
          │
          ▼
Readwise / Heptabase / Obsidian
    Knowledge Extraction
          │
          ▼
Pluralsight / Khan Academy
    Assessment & Mastery
          │
          ▼
RemNote / Anki
    Retention
```

Keines der untersuchten Produkte bildet genau diese Kombination als persönlichen, providerunabhängigen Learning Manager ab.

---

# 11. Empfohlener nächster Projektschritt

Auf Basis dieses Research-Dokuments sollte als nächstes **noch nicht programmiert** werden.

Sinnvolle Reihenfolge:

1. Produktfunktionen aus diesem Dokument normalisieren.
2. Doppelte bzw. semantisch gleiche Funktionen zusammenführen.
3. Einen vollständigen SASD-Learning-Manager-Funktionskatalog erstellen.
4. Funktionen klassifizieren:
   - Core
   - Important
   - Useful
   - Later
   - Explicitly Out of Scope
5. V1 anhand eines kleinen, wertstiftenden Workflows schneiden.
6. SASD Project Brief erstellen.
7. Fachliches Domain Model ableiten.
8. Lastenheft erstellen.
9. Pflichtenheft/Architektur erst danach festlegen.
10. Erst dann mit Milestone 0/1 in die Implementierung gehen.

Ein möglicher minimaler erster End-to-End-Workflow wäre:

```text
Lernziel anlegen
    ↓
Skill/Topic anlegen
    ↓
Learning Path strukturieren
    ↓
URL/Resource erfassen
    ↓
Resource einem oder mehreren Path Nodes zuordnen
    ↓
Status/Fortschritt dokumentieren
    ↓
Notiz/Evidence erfassen
```

Wenn dieser Workflow gut funktioniert, besitzt das Produkt bereits einen klaren Eigenwert, ohne dass PDF-Reader, AI, Spaced Repetition oder Enterprise-Funktionen erforderlich sind.

---

# 12. Quellenverzeichnis

## SASD

- SASD Development Standard  
  <https://github.com/Robin-Goerlach/SASD-Development-Standard>

## Degreed

- <https://degreed.com/experience/our-platform/>
- <https://degreed.com/experience/de/lxp/>

## roadmap.sh

- <https://roadmap.sh/>
- <https://roadmap.sh/about>
- <https://roadmap.sh/teams>
- <https://roadmap.sh/premium>

## Zotero

- <https://www.zotero.org/>
- <https://www.zotero.org/support/quick_start_guide>
- <https://www.zotero.org/support/collections_and_tags/>
- <https://www.zotero.org/support/pdf_reader>
- <https://www.zotero.org/support/notes>

## Karakeep

- <https://docs.karakeep.app/>

## Readwise Reader

- <https://docs.readwise.io/reader/docs/organizing-content>
- <https://docs.readwise.io/reader/docs/faqs/filtered-views>

## Pluralsight

- <https://help.pluralsight.com/hc/en-us/articles/24418811505044-Paths>
- <https://help.pluralsight.com/help/what-is-skill-iq>
- <https://help.pluralsight.com/hc/en-us/articles/24356159003924-Labs-overview>
- <https://help.pluralsight.com/hc/en-us/articles/31499201424020-Skills-subscription-and-plan-comparison>

## O’Reilly

- <https://www.oreilly.com/online-learning/features>
- <https://www.oreilly.com/online-learning/support/features.html>
- <https://www.oreilly.com/online-learning/integration-docs/search.html>
- <https://www.oreilly.com/live/>

## Heptabase

- <https://heptabase.com/>
- <https://support.heptabase.com/>
- <https://support.heptabase.com/en/articles/12679581-how-to-use-heptabase-mcp>

## RemNote

- <https://help.remnote.com/en/collections/3370931-flashcards>
- <https://help.remnote.com/en/articles/6690975-learning-from-pdfs-and-files-with-the-remnote-reader>
- <https://help.remnote.com/en/articles/15724936-guided-learn-mode>

## Class Central

- <https://www.classcentral.com/about>
- <https://www.classcentral.com/help/account-what-class-central>
- <https://www.classcentral.com/help/faq-what-courses-offered>

## Khan Academy

- <https://support.khanacademy.org/hc/de/articles/115002552631-Was-sind-Kurs-und-Lerneinheits-Mastery>

## Obsidian

- <https://obsidian.md/help/>
- <https://obsidian.md/help/plugins>
- <https://obsidian.md/help/bases>
- <https://obsidian.md/help/Plugins/Canvas>
- <https://obsidian.md/help/web-clipper>

## LinkedIn Learning

- <https://www.linkedin.com/help/learning/answer/a1429531>
- <https://www.linkedin.com/help/learning/answer/a10828013>

## Moodle

- <https://docs.moodle.org/500/en/Features>
- <https://docs.moodle.org/500/en/admin/tool/lp/learningplans>
- <https://docs.moodle.org/500/en/Programs_Configuration>
- <https://docs.moodle.org/500/en/Course_completion_report>

## 360Learning

- <https://360learning.com/product/skills/>
- <https://360learning.com/product/lxp/>
- <https://360learning.com/product/learning-management-system/>
- <https://support.360learning.com/hc/en-us/articles/360041650032-Create-a-learning-need>

## Docebo

- <https://help.docebo.com/hc/en-us/articles/360020083980-Creating-and-managing-learning-plans>
- <https://help.docebo.com/hc/en-us/sections/360005521799-Skills>
- <https://help.docebo.com/hc/en-us/articles/25135044544146-Best-practices-for-configuring-and-leveraging-skills-in-your-platform>

## Raindrop.io

- <https://help.raindrop.io/>
- <https://help.raindrop.io/bookmarks>
- <https://help.raindrop.io/import>
- <https://help.raindrop.io/stella>

## Capacities

- <https://docs.capacities.io/reference>
- <https://docs.capacities.io/reference/collections>
- <https://docs.capacities.io/reference/tags>
- <https://docs.capacities.io/reference/queries>
- <https://docs.capacities.io/reference/web-extension>

## Notion

- <https://www.notion.com/help/category/databases>
- <https://www.notion.com/help/relations-and-rollups>
- <https://www.notion.com/help/web-clipper>

## Linkwarden

- <https://github.com/linkwarden/linkwarden>

## linkding

- <https://github.com/sissbruecker/linkding>

## LearnAwesome

- <https://github.com/learn-awesome/learn>

## Anki

- <https://docs.ankiweb.net/deck-options>
- <https://docs.ankiweb.net/filtered-decks.html>
- <https://docs.ankiweb.net/stats.html>
- <https://docs.ankiweb.net/browsing.html>
- <https://docs.ankiweb.net/addons.html>

---

**Ende des Dokuments**
