using System.Data.Common;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SASD.Bewerbungsmanager.Infrastructure.Persistence;

namespace SASD.Bewerbungsmanager.Infrastructure.Operations;

/// <summary>
/// Produces a privacy-conscious technical health report. It reports integrity state and counts only;
/// business free text, message bodies, document contents, secrets and absolute data paths are excluded.
/// </summary>
public sealed class TrackerDiagnosticsService(IDbContextFactory<ApplicationTrackerDbContext> contextFactory)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    /// <summary>Collects the current database health state without exposing application content.</summary>
    public async Task<TrackerDiagnosticReport> CreateReportAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await context.Database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var quickCheck = await ReadFirstScalarAsync(context.Database.GetDbConnection(), "PRAGMA quick_check;", cancellationToken)
                .ConfigureAwait(false);
            var foreignKeyViolations = await CountRowsAsync(context.Database.GetDbConnection(), "PRAGMA foreign_key_check;", cancellationToken)
                .ConfigureAwait(false);

            var counts = new SortedDictionary<string, long>(StringComparer.Ordinal)
            {
                ["organizations"] = await context.Organizations.LongCountAsync(cancellationToken).ConfigureAwait(false),
                ["contacts"] = await context.Contacts.LongCountAsync(cancellationToken).ConfigureAwait(false),
                ["opportunities"] = await context.Opportunities.LongCountAsync(cancellationToken).ConfigureAwait(false),
                ["applications"] = await context.Applications.LongCountAsync(cancellationToken).ConfigureAwait(false),
                ["activities"] = await context.Activities.LongCountAsync(cancellationToken).ConfigureAwait(false),
                ["work_items"] = await context.Tasks.LongCountAsync(cancellationToken).ConfigureAwait(false),
                ["search_profiles"] = await context.SearchProfiles.LongCountAsync(cancellationToken).ConfigureAwait(false),
                ["documents"] = await context.Documents.LongCountAsync(cancellationToken).ConfigureAwait(false),
                ["application_document_snapshots"] = await context.ApplicationDocumentSnapshots.LongCountAsync(cancellationToken).ConfigureAwait(false),
                ["communication_messages"] = await context.CommunicationMessages.LongCountAsync(cancellationToken).ConfigureAwait(false),
                ["job_leads"] = await context.JobLeads.LongCountAsync(cancellationToken).ConfigureAwait(false),
                ["assistant_sessions"] = await context.AssistantSessions.LongCountAsync(cancellationToken).ConfigureAwait(false),
            };

            var applied = (await context.Database.GetAppliedMigrationsAsync(cancellationToken).ConfigureAwait(false)).ToArray();
            var pending = (await context.Database.GetPendingMigrationsAsync(cancellationToken).ConfigureAwait(false)).ToArray();
            return new TrackerDiagnosticReport(1, DateTimeOffset.UtcNow, quickCheck, foreignKeyViolations, applied, pending, counts);
        }
        finally
        {
            await context.Database.CloseConnectionAsync().ConfigureAwait(false);
        }
    }

    /// <summary>Writes a diagnostic report as UTF-8 JSON.</summary>
    public async Task WriteReportAsync(string targetPath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(targetPath))
        {
            throw new ArgumentException("Ein Zielpfad ist erforderlich.", nameof(targetPath));
        }

        var report = await CreateReportAsync(cancellationToken).ConfigureAwait(false);
        var path = Path.GetFullPath(targetPath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(report, JsonOptions), cancellationToken).ConfigureAwait(false);
    }

    private static async Task<string> ReadFirstScalarAsync(DbConnection connection, string commandText, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = commandText;
        var value = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
    }

    private static async Task<int> CountRowsAsync(DbConnection connection, string commandText, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = commandText;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        var count = 0;
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            count++;
        }

        return count;
    }
}
