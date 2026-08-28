using System.Text;
using SASD.Bewerbungsmanager.Application.Models;
using SASD.Bewerbungsmanager.Domain.Enums;
using SASD.Bewerbungsmanager.Infrastructure.Persistence;

namespace SASD.Bewerbungsmanager.Infrastructure.Tests;

/// <summary>Verifies the concrete file formats introduced for v0.2.0.</summary>
public sealed class ApplicationExportWriterTests
{
    [Fact]
    public async Task EvidenceCsv_WritesUtf8BomHeaderAndEscapedFields()
    {
        var directory = CreateTempDirectory();
        try
        {
            var path = Path.Combine(directory, "evidence.csv");
            var writer = new FileSystemApplicationExportWriter();
            await writer.WriteEvidenceCsvAsync(CreateEvidenceReport(), path);

            var bytes = await File.ReadAllBytesAsync(path);
            Assert.InRange(bytes.Length, 4, int.MaxValue);
            Assert.Equal((byte)0xEF, bytes[0]);
            Assert.Equal((byte)0xBB, bytes[1]);
            Assert.Equal((byte)0xBF, bytes[2]);

            var text = Encoding.UTF8.GetString(bytes[3..]);
            Assert.StartsWith("Versanddatum;Unternehmen;Position;Standort;Kanal;Status;Quelle(n)", text);
            Assert.Contains("\"Example; Systems GmbH\"", text);
            Assert.Contains("E-Mail", text);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task EvidencePdf_WritesMinimalValidPdfStructure()
    {
        var directory = CreateTempDirectory();
        try
        {
            var path = Path.Combine(directory, "evidence.pdf");
            var writer = new FileSystemApplicationExportWriter();
            await writer.WriteEvidencePdfAsync(CreateEvidenceReport(), path);

            var bytes = await File.ReadAllBytesAsync(path);
            var ascii = Encoding.ASCII.GetString(bytes);
            Assert.StartsWith("%PDF-1.4", ascii);
            Assert.Contains("/Type /Page", ascii);
            Assert.EndsWith("%%EOF\n", ascii);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task DossierExports_OmitLocalPathsAndRemainReadable()
    {
        var directory = CreateTempDirectory();
        try
        {
            var jsonPath = Path.Combine(directory, "dossier.json");
            var markdownPath = Path.Combine(directory, "dossier.md");
            var writer = new FileSystemApplicationExportWriter();
            var dossier = CreateDossier();

            await writer.WriteDossierJsonAsync(dossier, jsonPath);
            await writer.WriteDossierMarkdownAsync(dossier, markdownPath);

            var json = await File.ReadAllTextAsync(jsonPath);
            var markdown = await File.ReadAllTextAsync(markdownPath);

            Assert.Contains("\"schemaVersion\": 1", json);
            Assert.Contains("System Engineer Linux", json);
            Assert.DoesNotContain("StoredPath", json);
            Assert.DoesNotContain("C:\\\\Private", json);

            Assert.Contains("# Bewerbungsdossier", markdown);
            Assert.Contains("System Engineer Linux", markdown);
            Assert.Contains("Lokale Dateipfade", markdown);
            Assert.DoesNotContain("C:\\Private", markdown);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static ApplicationEvidenceReport CreateEvidenceReport()
        => new(
            new DateOnly(2026, 8, 1),
            new DateOnly(2026, 8, 31),
            new DateTimeOffset(2026, 8, 27, 8, 0, 0, TimeSpan.Zero),
            [
                new ApplicationEvidenceItem(
                    Guid.NewGuid(),
                    new DateTimeOffset(2026, 8, 25, 9, 0, 0, TimeSpan.Zero),
                    "Linux Platform Engineer",
                    "Example; Systems GmbH",
                    "Example City",
                    ApplicationChannel.Email,
                    ApplicationStage.Screening,
                    "Example Portal"),
            ]);

    private static ApplicationExchangeDossier CreateDossier()
        => new(
            SchemaVersion: 1,
            ExportedAtUtc: new DateTimeOffset(2026, 8, 27, 8, 0, 0, TimeSpan.Zero),
            ApplicationId: Guid.NewGuid(),
            OpportunityId: Guid.NewGuid(),
            Position: "System Engineer Linux",
            Employer: "Example Health IT GmbH",
            Intermediary: string.Empty,
            Stage: ApplicationStage.Interview,
            Channel: ApplicationChannel.Email,
            StartedAtUtc: new DateTimeOffset(2026, 8, 20, 8, 0, 0, TimeSpan.Zero),
            SubmittedAtUtc: new DateTimeOffset(2026, 8, 21, 8, 0, 0, TimeSpan.Zero),
            SalaryExpectation: null,
            Location: "Example City",
            RemoteText: "Hybrid",
            SalaryText: null,
            RoleDescriptionSnapshot: "Synthetic role description.",
            Sources: [new ApplicationDossierSource("Company", "https://example.invalid/jobs/1", null)],
            Contacts: [new ApplicationDossierContact("Erika Beispiel", "Recruiting", "erika@example.invalid", null, null)],
            Activities: [new ApplicationDossierActivity(ActivityKind.Interview, ActivityStatus.Planned, "Technical interview", null, null, new DateTimeOffset(2026, 8, 28, 9, 0, 0, TimeSpan.Zero))],
            Tasks: [new ApplicationDossierTask(WorkItemKind.Action, WorkItemStatus.Open, "Prepare interview", null, new DateTimeOffset(2026, 8, 27, 18, 0, 0, TimeSpan.Zero))],
            Documents: [new ApplicationDossierDocument(DocumentType.Cv, "Linux CV", "2026-08", "DE", new string('A', 64), new DateTimeOffset(2026, 8, 21, 8, 0, 0, TimeSpan.Zero))]);

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), $"sasd-export-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }
}
