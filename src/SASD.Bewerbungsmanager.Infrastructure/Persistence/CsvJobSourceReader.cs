using System.Globalization;
using System.Text;
using SASD.Bewerbungsmanager.Application.Abstractions;
using SASD.Bewerbungsmanager.Application.Exceptions;
using SASD.Bewerbungsmanager.Application.Models;

namespace SASD.Bewerbungsmanager.Infrastructure.Persistence;

/// <summary>
/// Reads a semicolon-separated UTF-8 job-source handoff. The parser supports quoted fields,
/// escaped quotes, and embedded line breaks so copied job descriptions remain intact.
/// </summary>
public sealed class CsvJobSourceReader : IJobSourceReader
{
    private static readonly string[] RequiredColumns = ["sourcesystem", "title"];

    /// <inheritdoc />
    public bool CanRead(string path)
        => Path.GetExtension(path).Equals(".csv", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public async Task<JobSourceBatch> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ValidationException("Eine Job-Quellen-Datei ist erforderlich.");
        }

        var fullPath = Path.GetFullPath(path);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("Die Job-Quellen-Datei wurde nicht gefunden.", fullPath);
        }

        var content = await File.ReadAllTextAsync(fullPath, Encoding.UTF8, cancellationToken).ConfigureAwait(false);
        var records = ParseRecords(content);
        if (records.Count < 2)
        {
            throw new ValidationException("Die CSV-Datei muss eine Kopfzeile und mindestens einen Treffer enthalten.");
        }

        var headers = records[0]
            .Select((value, index) => new { Name = NormalizeHeader(value), Index = index })
            .Where(item => item.Name.Length > 0)
            .ToDictionary(item => item.Name, item => item.Index, StringComparer.OrdinalIgnoreCase);
        foreach (var required in RequiredColumns)
        {
            if (!headers.ContainsKey(required))
            {
                throw new ValidationException($"Die CSV-Datei enthält die erforderliche Spalte '{required}' nicht.");
            }
        }

        var items = new List<JobSourceItem>();
        string? sourceSystem = null;
        Guid? searchProfileId = null;
        DateTimeOffset? capturedAtUtc = null;

        foreach (var record in records.Skip(1).Where(row => row.Any(value => !string.IsNullOrWhiteSpace(value))))
        {
            var rowSource = Get(record, headers, "sourcesystem");
            if (string.IsNullOrWhiteSpace(rowSource))
            {
                throw new ValidationException("Jede CSV-Zeile benötigt ein Quellsystem.");
            }

            sourceSystem ??= rowSource.Trim();
            if (!sourceSystem.Equals(rowSource.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                throw new ValidationException("Eine CSV-Datei darf nur Treffer eines Quellsystems enthalten.");
            }

            searchProfileId ??= ParseGuid(Get(record, headers, "searchprofileid"), "searchProfileId");
            capturedAtUtc ??= ParseDate(Get(record, headers, "capturedatutc"), "capturedAtUtc");
            items.Add(new JobSourceItem(
                EmptyToNull(Get(record, headers, "externaljobid")),
                Get(record, headers, "title"),
                EmptyToNull(Get(record, headers, "organizationname")),
                EmptyToNull(Get(record, headers, "location")),
                EmptyToNull(Get(record, headers, "remotetext")),
                EmptyToNull(Get(record, headers, "salarytext")),
                EmptyToNull(Get(record, headers, "url")),
                EmptyToNull(Get(record, headers, "descriptiontext")),
                ParseDate(Get(record, headers, "publishedatutc"), "publishedAtUtc")));
        }

        return new JobSourceBatch(
            SchemaVersion: 1,
            SourceSystem: sourceSystem ?? throw new ValidationException("Das CSV-Paket enthält kein Quellsystem."),
            SearchProfileId: searchProfileId,
            CapturedAtUtc: capturedAtUtc ?? DateTimeOffset.UtcNow,
            Items: items);
    }

    private static string NormalizeHeader(string value)
        => value.Trim().TrimStart('\uFEFF').Replace("_", string.Empty, StringComparison.Ordinal).ToLowerInvariant();

    private static string Get(IReadOnlyList<string> row, IReadOnlyDictionary<string, int> headers, string name)
        => headers.TryGetValue(name, out var index) && index < row.Count ? row[index].Trim() : string.Empty;

    private static string? EmptyToNull(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static Guid? ParseGuid(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (!Guid.TryParse(value, out var parsed))
        {
            throw new ValidationException($"CSV-Feld '{fieldName}' enthält keine gültige GUID.");
        }

        return parsed;
    }

    private static DateTimeOffset? ParseDate(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (!DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed))
        {
            throw new ValidationException($"CSV-Feld '{fieldName}' enthält kein gültiges Datum.");
        }

        return parsed.ToUniversalTime();
    }

    private static List<List<string>> ParseRecords(string text)
    {
        var records = new List<List<string>>();
        var row = new List<string>();
        var field = new StringBuilder();
        var inQuotes = false;

        for (var index = 0; index < text.Length; index++)
        {
            var ch = text[index];
            if (ch == '"')
            {
                if (inQuotes && index + 1 < text.Length && text[index + 1] == '"')
                {
                    field.Append('"');
                    index++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
                continue;
            }

            if (!inQuotes && ch == ';')
            {
                row.Add(field.ToString());
                field.Clear();
                continue;
            }

            if (!inQuotes && (ch == '\r' || ch == '\n'))
            {
                if (ch == '\r' && index + 1 < text.Length && text[index + 1] == '\n')
                {
                    index++;
                }
                row.Add(field.ToString());
                field.Clear();
                records.Add(row);
                row = [];
                continue;
            }

            field.Append(ch);
        }

        if (inQuotes)
        {
            throw new ValidationException("Die CSV-Datei enthält ein nicht geschlossenes Anführungszeichen.");
        }

        if (field.Length > 0 || row.Count > 0)
        {
            row.Add(field.ToString());
            records.Add(row);
        }

        return records;
    }
}
