using System.Globalization;
using System.Text;
using SASD.LearningManager.Application.Providers;
using SASD.LearningManager.Application.Resources;
using SASD.LearningManager.Domain.Providers;
using SASD.LearningManager.Domain.Resources;

namespace SASD.LearningManager.Application.ImportExport;

/// <summary>
/// Imports and exports the canonical resource library as an RFC-4180-compatible CSV document.
/// The service intentionally uses the existing provider and resource application services so CSV
/// transfer cannot bypass domain validation, URL duplicate checks, lifecycle rules or tag cleanup.
/// </summary>
public sealed class ResourceCsvTransferService
{
    private static readonly string[] Header =
    [
        "Title", "Type", "Provider", "Url", "LocalPath", "Description", "WhySaved", "Creator",
        "LanguageCode", "VersionText", "EstimatedMinutes", "Difficulty", "Priority", "Status",
        "ProgressPercent", "Tags"
    ];

    private readonly ResourceService _resourceService;
    private readonly ProviderService _providerService;

    /// <summary>Initializes the CSV transfer use case.</summary>
    public ResourceCsvTransferService(ResourceService resourceService, ProviderService providerService)
    {
        _resourceService = resourceService;
        _providerService = providerService;
    }

    /// <summary>
    /// Exports every resource, including archived records, to a portable UTF-8 CSV document.
    /// Tags are stored in one semicolon-separated field because semicolons remain ordinary data
    /// inside the RFC-4180 quoted CSV field.
    /// </summary>
    public async Task ExportAsync(string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var providers = await _providerService.ListAsync(includeArchived: true, cancellationToken).ConfigureAwait(false);
        var providerNames = providers.ToDictionary(static provider => provider.Id, static provider => provider.Name);

        var rows = new List<IReadOnlyList<string?>>();
        var pageNumber = 1;

        while (true)
        {
            var page = await _resourceService.SearchAsync(
                new ResourceSearchCriteria(null, null, null, null, null, IncludeArchived: true, pageNumber, 500),
                cancellationToken).ConfigureAwait(false);

            foreach (var item in page.Items)
            {
                var detail = await _resourceService.GetDetailAsync(item.Id, cancellationToken).ConfigureAwait(false)
                    ?? throw new InvalidOperationException($"Resource '{item.Id}' disappeared during export.");

                rows.Add(
                [
                    detail.Title,
                    detail.Type.ToString(),
                    detail.ProviderId is Guid providerId && providerNames.TryGetValue(providerId, out var providerName) ? providerName : null,
                    detail.Url,
                    detail.LocalPath,
                    detail.Description,
                    detail.WhySaved,
                    detail.Creator,
                    detail.LanguageCode,
                    detail.VersionText,
                    detail.EstimatedMinutes?.ToString(CultureInfo.InvariantCulture),
                    detail.Difficulty.ToString(),
                    detail.Priority.ToString(),
                    detail.Status.ToString(),
                    detail.ProgressPercent?.ToString(CultureInfo.InvariantCulture),
                    string.Join(';', detail.Tags)
                ]);
            }

            if (page.Items.Count < 500)
            {
                break;
            }

            pageNumber++;
        }

        await File.WriteAllTextAsync(filePath, CsvDocument.Write(Header, rows), new UTF8Encoding(encoderShouldEmitUTF8Identifier: true), cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Imports resources from the portable CSV format. One bad row does not invalidate the entire
    /// file; instead, the returned report contains row-specific diagnostics for user review.
    /// Existing URL duplicates are skipped through the canonical-resource rule rather than being
    /// silently inserted a second time.
    /// </summary>
    public async Task<ResourceCsvImportReport> ImportAsync(string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        var csv = await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);
        var document = CsvDocument.Read(csv);
        ValidateHeader(document.Header);

        var providers = (await _providerService.ListAsync(includeArchived: false, cancellationToken).ConfigureAwait(false))
            .ToDictionary(static provider => provider.Name, StringComparer.OrdinalIgnoreCase);

        var created = 0;
        var skippedDuplicates = 0;
        var errors = new List<ResourceCsvImportError>();

        for (var index = 0; index < document.Rows.Count; index++)
        {
            var rowNumber = index + 2; // Header occupies physical/logical row one.
            var row = document.Rows[index];

            try
            {
                var providerId = await ResolveProviderAsync(row[2], providers, cancellationToken).ConfigureAwait(false);
                var requestedStatus = ParseEnum<ResourceStatus>(row[13], "Status", rowNumber);
                var createStatus = requestedStatus == ResourceStatus.Archived ? ResourceStatus.Planned : requestedStatus;

                var model = new ResourceEditModel(
                    Required(row[0], "Title", rowNumber),
                    ParseEnum<ResourceType>(row[1], "Type", rowNumber),
                    providerId,
                    NullIfWhiteSpace(row[3]),
                    NullIfWhiteSpace(row[4]),
                    NullIfWhiteSpace(row[5]),
                    NullIfWhiteSpace(row[6]),
                    NullIfWhiteSpace(row[7]),
                    NullIfWhiteSpace(row[8]),
                    NullIfWhiteSpace(row[9]),
                    ParseNullableInt(row[10], "EstimatedMinutes", rowNumber),
                    ParseEnum<ResourceDifficulty>(row[11], "Difficulty", rowNumber),
                    ParseEnum<ResourcePriority>(row[12], "Priority", rowNumber),
                    createStatus,
                    ParseNullableInt(row[14], "ProgressPercent", rowNumber),
                    ParseTags(row[15]));

                var id = await _resourceService.CreateAsync(model, allowDuplicateUrl: false, cancellationToken).ConfigureAwait(false);
                if (requestedStatus == ResourceStatus.Archived)
                {
                    await _resourceService.ArchiveAsync(id, cancellationToken).ConfigureAwait(false);
                }

                created++;
            }
            catch (DuplicateResourceException exception)
            {
                skippedDuplicates++;
                errors.Add(new ResourceCsvImportError(rowNumber, $"Duplicate URL skipped: {exception.Message}"));
            }
            catch (Exception exception) when (exception is ArgumentException or InvalidOperationException or FormatException)
            {
                errors.Add(new ResourceCsvImportError(rowNumber, exception.Message));
            }
        }

        return new ResourceCsvImportReport(document.Rows.Count, created, skippedDuplicates, errors);
    }

    private async Task<Guid?> ResolveProviderAsync(
        string? providerName,
        IDictionary<string, ProviderListItemDto> providers,
        CancellationToken cancellationToken)
    {
        var normalized = NullIfWhiteSpace(providerName);
        if (normalized is null)
        {
            return null;
        }

        if (providers.TryGetValue(normalized, out var existing))
        {
            return existing.Id;
        }

        // CSV files are intentionally provider-neutral. If an imported resource references a
        // provider unknown to this installation, create a minimal provider rather than discarding
        // useful source information. The user can enrich its metadata later in provider management.
        var id = await _providerService.CreateAsync(
            new ProviderEditModel(normalized, null, "Automatically created during CSV resource import.", ProviderType.Other),
            cancellationToken).ConfigureAwait(false);

        providers[normalized] = new ProviderListItemDto(id, normalized, ProviderType.Other, ProviderStatus.Active, null);
        return id;
    }

    private static void ValidateHeader(IReadOnlyList<string> actual)
    {
        if (actual.Count != Header.Length || !actual.SequenceEqual(Header, StringComparer.OrdinalIgnoreCase))
        {
            throw new FormatException($"Unsupported resource CSV header. Expected: {string.Join(',', Header)}");
        }
    }

    private static TEnum ParseEnum<TEnum>(string? value, string field, int rowNumber)
        where TEnum : struct, Enum
    {
        if (Enum.TryParse<TEnum>(value, ignoreCase: true, out var parsed) && Enum.IsDefined(parsed))
        {
            return parsed;
        }

        throw new FormatException($"Row {rowNumber}: '{value}' is not a valid {field} value.");
    }

    private static int? ParseNullableInt(string? value, string field, int rowNumber)
    {
        var normalized = NullIfWhiteSpace(value);
        if (normalized is null)
        {
            return null;
        }

        if (int.TryParse(normalized, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        throw new FormatException($"Row {rowNumber}: '{value}' is not a valid integer for {field}.");
    }

    private static string Required(string? value, string field, int rowNumber)
        => NullIfWhiteSpace(value) ?? throw new FormatException($"Row {rowNumber}: {field} is required.");

    private static string? NullIfWhiteSpace(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static IReadOnlyCollection<string> ParseTags(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? []
            : value.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}

/// <summary>Summarizes one completed resource CSV import.</summary>
public sealed record ResourceCsvImportReport(
    int TotalRows,
    int Created,
    int SkippedDuplicates,
    IReadOnlyList<ResourceCsvImportError> Errors);

/// <summary>Describes an import problem associated with one CSV row.</summary>
public sealed record ResourceCsvImportError(int RowNumber, string Message);

/// <summary>
/// Small dependency-free CSV codec used by import/export. It supports quoted fields, escaped
/// quotes, commas and embedded line breaks, avoiding a third-party package for this narrow V1 need.
/// </summary>
public static class CsvDocument
{
    /// <summary>Serializes a header and rows using RFC-4180 quoting rules.</summary>
    public static string Write(IReadOnlyList<string> header, IEnumerable<IReadOnlyList<string?>> rows)
    {
        var builder = new StringBuilder();
        AppendRow(builder, header.Select(static value => (string?)value));
        foreach (var row in rows)
        {
            AppendRow(builder, row);
        }

        return builder.ToString();
    }

    /// <summary>Parses an RFC-4180-style CSV string into header and data rows.</summary>
    public static CsvData Read(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        var rows = new List<IReadOnlyList<string>>();
        var row = new List<string>();
        var field = new StringBuilder();
        var quoted = false;

        for (var index = 0; index < text.Length; index++)
        {
            var character = text[index];
            if (quoted)
            {
                if (character == '"')
                {
                    if (index + 1 < text.Length && text[index + 1] == '"')
                    {
                        field.Append('"');
                        index++;
                    }
                    else
                    {
                        quoted = false;
                    }
                }
                else
                {
                    field.Append(character);
                }

                continue;
            }

            switch (character)
            {
                case '"' when field.Length == 0:
                    quoted = true;
                    break;
                case ',':
                    row.Add(field.ToString());
                    field.Clear();
                    break;
                case '\r':
                    if (index + 1 < text.Length && text[index + 1] == '\n')
                    {
                        index++;
                    }
                    CompleteRow(rows, row, field);
                    break;
                case '\n':
                    CompleteRow(rows, row, field);
                    break;
                default:
                    field.Append(character);
                    break;
            }
        }

        if (quoted)
        {
            throw new FormatException("CSV ends inside a quoted field.");
        }

        if (field.Length > 0 || row.Count > 0)
        {
            CompleteRow(rows, row, field);
        }

        if (rows.Count == 0)
        {
            throw new FormatException("CSV file is empty.");
        }

        var header = rows[0].Select(static value => value.TrimStart('\uFEFF')).ToArray();
        var dataRows = rows.Skip(1).Where(static values => values.Any(static value => value.Length > 0)).ToArray();
        if (dataRows.Any(values => values.Count != header.Length))
        {
            throw new FormatException("CSV contains a row with a different number of columns than the header.");
        }

        return new CsvData(header, dataRows);
    }

    private static void CompleteRow(List<IReadOnlyList<string>> rows, List<string> row, StringBuilder field)
    {
        row.Add(field.ToString());
        field.Clear();
        rows.Add(row.ToArray());
        row.Clear();
    }

    private static void AppendRow(StringBuilder builder, IEnumerable<string?> values)
    {
        builder.AppendLine(string.Join(',', values.Select(Escape)));
    }

    private static string Escape(string? value)
    {
        var text = value ?? string.Empty;
        return text.IndexOfAny([',', '"', '\r', '\n']) >= 0
            ? $"\"{text.Replace("\"", "\"\"")}\""
            : text;
    }
}

/// <summary>In-memory representation of a parsed CSV document.</summary>
public sealed record CsvData(IReadOnlyList<string> Header, IReadOnlyList<IReadOnlyList<string>> Rows);
