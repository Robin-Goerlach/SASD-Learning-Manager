using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SASD.Bewerbungsmanager.Application.Abstractions;
using SASD.Bewerbungsmanager.Application.Models;
using SASD.Bewerbungsmanager.Domain.Enums;

namespace SASD.Bewerbungsmanager.Infrastructure.Persistence;

/// <summary>
/// Writes evidence reports and application exchange dossiers to files selected by the user. The
/// implementation deliberately uses only .NET platform APIs so the early local-first product does
/// not gain another document-generation dependency merely for a compact evidence PDF.
/// </summary>
public sealed class FileSystemApplicationExportWriter : IApplicationExportWriter
{
    private static readonly CultureInfo GermanCulture = CultureInfo.GetCultureInfo("de-DE");
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    /// <inheritdoc />
    public async Task WriteEvidenceCsvAsync(
        ApplicationEvidenceReport report,
        string path,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(report);
        var fullPath = PrepareTargetPath(path);
        await using var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 16_384, useAsync: true);
        await using var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

        await writer.WriteLineAsync("Versanddatum;Unternehmen;Position;Standort;Kanal;Status;Quelle(n)").ConfigureAwait(false);
        foreach (var item in report.Items)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var fields = new[]
            {
                item.SubmittedAtUtc.ToLocalTime().ToString("dd.MM.yyyy", GermanCulture),
                item.Employer,
                item.Position,
                item.Location ?? string.Empty,
                ChannelText(item.Channel),
                StageText(item.Stage),
                item.Sources,
            };
            await writer.WriteLineAsync(string.Join(";", fields.Select(CsvField))).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public Task WriteEvidencePdfAsync(
        ApplicationEvidenceReport report,
        string path,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(report);
        var lines = BuildEvidencePdfLines(report);
        return SimplePdfWriter.WriteAsync(PrepareTargetPath(path), lines, cancellationToken);
    }

    /// <inheritdoc />
    public async Task WriteDossierJsonAsync(
        ApplicationExchangeDossier dossier,
        string path,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dossier);
        var json = JsonSerializer.Serialize(dossier, JsonOptions);
        await File.WriteAllTextAsync(PrepareTargetPath(path), json, new UTF8Encoding(false), cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task WriteDossierMarkdownAsync(
        ApplicationExchangeDossier dossier,
        string path,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dossier);
        var markdown = BuildDossierMarkdown(dossier);
        await File.WriteAllTextAsync(PrepareTargetPath(path), markdown, new UTF8Encoding(false), cancellationToken).ConfigureAwait(false);
    }

    private static IReadOnlyList<string> BuildEvidencePdfLines(ApplicationEvidenceReport report)
    {
        var generatedText = report.GeneratedAtUtc.ToLocalTime().ToString("dd.MM.yyyy HH:mm", GermanCulture);
        var lines = new List<string>
        {
            "SASD Bewerbungsmanager - Bewerbungsnachweis",
            $"Zeitraum: {report.FromDate:dd.MM.yyyy} bis {report.ToDate:dd.MM.yyyy}",
            $"Erstellt: {generatedText}",
            $"Tatsächlich versendete Bewerbungen: {report.Items.Count}",
            string.Empty,
        };

        if (report.Items.Count == 0)
        {
            lines.Add("Im ausgewählten Zeitraum sind keine versendeten Bewerbungen erfasst.");
            return lines;
        }

        var index = 1;
        foreach (var item in report.Items)
        {
            var submittedDate = item.SubmittedAtUtc.ToLocalTime().ToString("dd.MM.yyyy", GermanCulture);
            var employer = Safe(item.Employer, "(Unternehmen nicht erfasst)");
            lines.Add($"{index}. {submittedDate} - {employer}");
            lines.Add($"   Position: {item.Position}");
            if (!string.IsNullOrWhiteSpace(item.Location))
            {
                lines.Add($"   Standort: {item.Location}");
            }

            lines.Add($"   Kanal: {ChannelText(item.Channel)} | Status: {StageText(item.Stage)}");
            if (!string.IsNullOrWhiteSpace(item.Sources))
            {
                lines.Add($"   Quelle(n): {item.Sources}");
            }

            lines.Add(string.Empty);
            index++;
        }

        return lines;
    }

    private static string BuildDossierMarkdown(ApplicationExchangeDossier dossier)
    {
        var builder = new StringBuilder(8192);
        builder.AppendLine("# Bewerbungsdossier");
        builder.AppendLine();
        builder.AppendLine($"- **Schema:** {dossier.SchemaVersion}");
        builder.AppendLine($"- **Exportiert:** {FormatDateTime(dossier.ExportedAtUtc)}");
        builder.AppendLine($"- **Position:** {MarkdownInline(dossier.Position)}");
        builder.AppendLine($"- **Unternehmen:** {MarkdownInline(Safe(dossier.Employer, "-"))}");
        builder.AppendLine($"- **Vermittler:** {MarkdownInline(Safe(dossier.Intermediary, "-"))}");
        builder.AppendLine($"- **Status:** {StageText(dossier.Stage)}");
        builder.AppendLine($"- **Kanal:** {ChannelText(dossier.Channel)}");
        builder.AppendLine($"- **Begonnen:** {FormatDateTime(dossier.StartedAtUtc)}");
        var submittedText = dossier.SubmittedAtUtc is null ? "-" : FormatDateTime(dossier.SubmittedAtUtc.Value);
        builder.AppendLine($"- **Versendet:** {submittedText}");
        AppendOptionalBullet(builder, "Standort", dossier.Location);
        AppendOptionalBullet(builder, "Remote/Hybrid", dossier.RemoteText);
        AppendOptionalBullet(builder, "Gehalt Stellenanzeige", dossier.SalaryText);
        AppendOptionalBullet(builder, "Gehaltsvorstellung", dossier.SalaryExpectation);

        builder.AppendLine();
        builder.AppendLine("## Rollenbeschreibung");
        builder.AppendLine();
        builder.AppendLine(string.IsNullOrWhiteSpace(dossier.RoleDescriptionSnapshot) ? "-" : dossier.RoleDescriptionSnapshot.Trim());

        builder.AppendLine();
        builder.AppendLine("## Quellen");
        builder.AppendLine();
        AppendList(builder, dossier.Sources.Select(FormatSource));

        builder.AppendLine();
        builder.AppendLine("## Kontakte");
        builder.AppendLine();
        AppendList(builder, dossier.Contacts.Select(FormatContact));

        builder.AppendLine();
        builder.AppendLine("## Verlauf");
        builder.AppendLine();
        AppendList(builder, dossier.Activities.Select(FormatActivity));

        builder.AppendLine();
        builder.AppendLine("## Aufgaben und Warten auf");
        builder.AppendLine();
        AppendList(builder, dossier.Tasks.Select(FormatTask));

        builder.AppendLine();
        builder.AppendLine("## Verwendete Dokumente");
        builder.AppendLine();
        AppendList(builder, dossier.Documents.Select(item =>
            $"{DocumentTypeText(item.Type)}: {MarkdownInline(item.Label)} / {MarkdownInline(item.Version)} / {MarkdownInline(item.Language)} / SHA-256 {item.Sha256}"));

        builder.AppendLine();
        builder.AppendLine("---");
        builder.AppendLine("Lokale Dateipfade und Dokumentinhalte sind absichtlich nicht Bestandteil dieses Austauschdossiers.");
        return builder.ToString();
    }

    private static void AppendOptionalBullet(StringBuilder builder, string label, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            builder.AppendLine($"- **{label}:** {MarkdownInline(value)}");
        }
    }

    private static void AppendList(StringBuilder builder, IEnumerable<string> values)
    {
        var any = false;
        foreach (var value in values)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            builder.AppendLine($"- {value}");
            any = true;
        }

        if (!any)
        {
            builder.AppendLine("-");
        }
    }

    private static string FormatSource(ApplicationDossierSource source)
    {
        var externalId = string.IsNullOrWhiteSpace(source.ExternalId)
            ? string.Empty
            : $" — ID {MarkdownInline(source.ExternalId)}";
        return $"{MarkdownInline(source.Source)} — {MarkdownInline(source.Url)}{externalId}";
    }

    private static string FormatContact(ApplicationDossierContact contact)
    {
        var parts = new List<string> { MarkdownInline(contact.FullName) };
        if (!string.IsNullOrWhiteSpace(contact.Role))
        {
            parts.Add(MarkdownInline(contact.Role));
        }

        if (!string.IsNullOrWhiteSpace(contact.Email))
        {
            parts.Add(MarkdownInline(contact.Email));
        }

        if (!string.IsNullOrWhiteSpace(contact.Phone))
        {
            parts.Add(MarkdownInline(contact.Phone));
        }

        if (!string.IsNullOrWhiteSpace(contact.LinkedInUrl))
        {
            parts.Add(MarkdownInline(contact.LinkedInUrl));
        }

        return string.Join(" — ", parts);
    }

    private static string FormatActivity(ApplicationDossierActivity activity)
    {
        var timestamp = activity.OccurredAtUtc ?? activity.ScheduledAtUtc;
        var when = timestamp is null ? string.Empty : $"{FormatDateTime(timestamp.Value)} — ";
        var notes = string.IsNullOrWhiteSpace(activity.Notes) ? string.Empty : $" — {MarkdownInline(activity.Notes)}";
        return $"{when}{ActivityKindText(activity.Kind)} / {ActivityStatusText(activity.Status)}: {MarkdownInline(activity.Subject)}{notes}";
    }

    private static string FormatTask(ApplicationDossierTask task)
    {
        var due = task.DueAtUtc is null ? string.Empty : $" — fällig {FormatDateTime(task.DueAtUtc.Value)}";
        var notes = string.IsNullOrWhiteSpace(task.Notes) ? string.Empty : $" — {MarkdownInline(task.Notes)}";
        return $"{WorkItemKindText(task.Kind)} / {WorkItemStatusText(task.Status)}: {MarkdownInline(task.Title)}{due}{notes}";
    }

    private static string CsvField(string value)
    {
        var normalized = value.Replace("\r\n", " ", StringComparison.Ordinal)
            .Replace('\r', ' ')
            .Replace('\n', ' ');
        if (!normalized.Contains(';') && !normalized.Contains('\"'))
        {
            return normalized;
        }

        var escaped = normalized.Replace("\"", "\"\"", StringComparison.Ordinal);
        return $"\"{escaped}\"";
    }

    private static string PrepareTargetPath(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var fullPath = Path.GetFullPath(path);
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        return fullPath;
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    private static string FormatDateTime(DateTimeOffset value)
        => value.ToLocalTime().ToString("dd.MM.yyyy HH:mm", GermanCulture);

    private static string Safe(string? value, string fallback)
        => string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

    private static string MarkdownInline(string value)
        => value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("|", "\\|", StringComparison.Ordinal)
            .Replace("\r\n", " ", StringComparison.Ordinal)
            .Replace('\r', ' ')
            .Replace('\n', ' ')
            .Trim();

    private static string ChannelText(ApplicationChannel value) => value switch
    {
        ApplicationChannel.Unknown => "Unbekannt",
        ApplicationChannel.Portal => "Portal",
        ApplicationChannel.Email => "E-Mail",
        ApplicationChannel.LinkedIn => "LinkedIn",
        ApplicationChannel.Recruiter => "Recruiter",
        ApplicationChannel.Other => "Sonstiges",
        _ => value.ToString(),
    };

    private static string StageText(ApplicationStage value) => value switch
    {
        ApplicationStage.Draft => "Entwurf",
        ApplicationStage.Submitted => "Versendet",
        ApplicationStage.Screening => "Prüfung",
        ApplicationStage.Interview => "Interview",
        ApplicationStage.Offer => "Angebot",
        ApplicationStage.Rejected => "Absage",
        ApplicationStage.Withdrawn => "Zurückgezogen",
        ApplicationStage.Hired => "Eingestellt",
        ApplicationStage.Closed => "Abgeschlossen",
        _ => value.ToString(),
    };

    private static string ActivityKindText(ActivityKind value) => value switch
    {
        ActivityKind.Email => "E-Mail",
        ActivityKind.PhoneCall => "Telefonat",
        ActivityKind.LinkedIn => "LinkedIn",
        ActivityKind.ApplicationSubmitted => "Bewerbung versendet",
        ActivityKind.Interview => "Interview",
        ActivityKind.Meeting => "Meeting",
        ActivityKind.AuthorityAppointment => "Behördentermin",
        ActivityKind.Note => "Notiz",
        ActivityKind.Other => "Sonstiges",
        _ => value.ToString(),
    };

    private static string ActivityStatusText(ActivityStatus value) => value switch
    {
        ActivityStatus.Recorded => "Stattgefunden",
        ActivityStatus.Planned => "Geplant",
        ActivityStatus.Completed => "Erledigt",
        ActivityStatus.Cancelled => "Abgesagt",
        _ => value.ToString(),
    };

    private static string WorkItemKindText(WorkItemKind value) => value switch
    {
        WorkItemKind.Action => "ACTION",
        WorkItemKind.WaitingFor => "WAITING_FOR",
        _ => value.ToString(),
    };

    private static string WorkItemStatusText(WorkItemStatus value) => value switch
    {
        WorkItemStatus.Open => "Offen",
        WorkItemStatus.Completed => "Erledigt",
        WorkItemStatus.Cancelled => "Abgebrochen",
        _ => value.ToString(),
    };

    private static string DocumentTypeText(DocumentType value) => value switch
    {
        DocumentType.Cv => "Lebenslauf",
        DocumentType.CoverLetter => "Anschreiben",
        DocumentType.Certificate => "Zeugnis",
        DocumentType.JobAdvertisement => "Stellenanzeige",
        DocumentType.Other => "Sonstiges",
        _ => value.ToString(),
    };
}

/// <summary>
/// Very small PDF 1.4 writer for line-oriented evidence documents. It deliberately supports only
/// the features needed by the report: A4 pages, a built-in Helvetica font and WinAnsi text. Keeping
/// this implementation narrow avoids introducing a large PDF framework into the operational MVP.
/// </summary>
internal static class SimplePdfWriter
{
    private const int LinesPerPage = 55;
    private const int WrapColumn = 96;

    public static async Task WriteAsync(string path, IReadOnlyList<string> sourceLines, CancellationToken cancellationToken)
    {
        var wrapped = sourceLines.SelectMany(WrapLine).ToList();
        if (wrapped.Count == 0)
        {
            wrapped.Add(string.Empty);
        }

        var pages = wrapped.Chunk(LinesPerPage).Select(chunk => chunk.ToList()).ToList();
        var bytes = BuildDocument(pages);
        await File.WriteAllBytesAsync(path, bytes, cancellationToken).ConfigureAwait(false);
    }

    private static byte[] BuildDocument(IReadOnlyList<List<string>> pages)
    {
        using var stream = new MemoryStream();
        var offsets = new List<long> { 0 };
        WriteAscii(stream, "%PDF-1.4\n%SASD\n");

        WriteObject(stream, offsets, 1, "<< /Type /Catalog /Pages 2 0 R >>");
        var kids = string.Join(" ", Enumerable.Range(0, pages.Count).Select(index => $"{4 + (index * 2)} 0 R"));
        WriteObject(stream, offsets, 2, $"<< /Type /Pages /Kids [{kids}] /Count {pages.Count} >>");
        WriteObject(stream, offsets, 3, "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>");

        for (var index = 0; index < pages.Count; index++)
        {
            var pageObject = 4 + (index * 2);
            var contentObject = pageObject + 1;
            WriteObject(stream, offsets, pageObject,
                $"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 3 0 R >> >> /Contents {contentObject} 0 R >>");

            var content = BuildPageContent(pages[index], index + 1, pages.Count);
            WriteStreamObject(stream, offsets, contentObject, content);
        }

        var xrefOffset = stream.Position;
        WriteAscii(stream, $"xref\n0 {offsets.Count}\n");
        WriteAscii(stream, "0000000000 65535 f \n");
        for (var index = 1; index < offsets.Count; index++)
        {
            var offsetText = offsets[index].ToString("D10", CultureInfo.InvariantCulture);
            WriteAscii(stream, $"{offsetText} 00000 n \n");
        }

        var xrefText = xrefOffset.ToString(CultureInfo.InvariantCulture);
        WriteAscii(stream, $"trailer\n<< /Size {offsets.Count} /Root 1 0 R >>\nstartxref\n{xrefText}\n%%EOF\n");
        return stream.ToArray();
    }

    private static string BuildPageContent(IReadOnlyList<string> lines, int pageNumber, int pageCount)
    {
        var builder = new StringBuilder();
        builder.AppendLine("BT");
        builder.AppendLine("/F1 9 Tf");
        builder.AppendLine("12 TL");
        builder.AppendLine("42 795 Td");
        foreach (var line in lines)
        {
            builder.Append('<').Append(EncodeWinAnsiHex(line)).AppendLine("> Tj");
            builder.AppendLine("T*");
        }

        builder.AppendLine("ET");
        builder.AppendLine("BT");
        builder.AppendLine("/F1 8 Tf");
        builder.AppendLine("42 28 Td");
        builder.Append('<').Append(EncodeWinAnsiHex($"Seite {pageNumber} / {pageCount}")).AppendLine("> Tj");
        builder.AppendLine("ET");
        return builder.ToString();
    }

    private static IEnumerable<string> WrapLine(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            yield return string.Empty;
            yield break;
        }

        var remaining = value.TrimEnd();
        while (remaining.Length > WrapColumn)
        {
            var splitAt = remaining.LastIndexOf(' ', WrapColumn);
            if (splitAt < 20)
            {
                splitAt = WrapColumn;
            }

            yield return remaining[..splitAt].TrimEnd();
            remaining = remaining[splitAt..].TrimStart();
        }

        yield return remaining;
    }

    private static string EncodeWinAnsiHex(string value)
    {
        var builder = new StringBuilder(value.Length * 2);
        foreach (var character in value)
        {
            var current = ToWinAnsiByte(character);
            builder.Append(current.ToString("X2", CultureInfo.InvariantCulture));
        }

        return builder.ToString();
    }

    private static byte ToWinAnsiByte(char value)
    {
        if (value is >= ' ' and <= '~')
        {
            return (byte)value;
        }

        if (value is >= '\u00A0' and <= '\u00FF')
        {
            return (byte)value;
        }

        return value switch
        {
            '\u20AC' => 0x80,
            '\u201A' => 0x82,
            '\u201E' => 0x84,
            '\u2026' => 0x85,
            '\u2018' => 0x91,
            '\u2019' => 0x92,
            '\u201C' => 0x93,
            '\u201D' => 0x94,
            '\u2013' => 0x96,
            '\u2014' => 0x97,
            _ => (byte)'?',
        };
    }

    private static void WriteObject(Stream stream, List<long> offsets, int objectNumber, string body)
    {
        EnsureOffsetSlot(offsets, objectNumber);
        offsets[objectNumber] = stream.Position;
        WriteAscii(stream, $"{objectNumber} 0 obj\n{body}\nendobj\n");
    }

    private static void WriteStreamObject(Stream stream, List<long> offsets, int objectNumber, string content)
    {
        var contentBytes = Encoding.ASCII.GetBytes(content);
        EnsureOffsetSlot(offsets, objectNumber);
        offsets[objectNumber] = stream.Position;
        WriteAscii(stream, $"{objectNumber} 0 obj\n<< /Length {contentBytes.Length} >>\nstream\n");
        stream.Write(contentBytes, 0, contentBytes.Length);
        WriteAscii(stream, "endstream\nendobj\n");
    }

    private static void EnsureOffsetSlot(List<long> offsets, int objectNumber)
    {
        while (offsets.Count <= objectNumber)
        {
            offsets.Add(0);
        }
    }

    private static void WriteAscii(Stream stream, string value)
    {
        var bytes = Encoding.ASCII.GetBytes(value);
        stream.Write(bytes, 0, bytes.Length);
    }
}
