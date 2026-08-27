using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace SASD.LearningManager.Infrastructure.Persistence;

/// <summary>
/// Applies embedded, immutable SQL migrations in ascending order. The schema version and migration
/// checksum are recorded so a modified historical migration is detected instead of silently accepted.
/// </summary>
public sealed class DatabaseInitializer
{
    private readonly SqliteConnectionFactory _connectionFactory;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(SqliteConnectionFactory connectionFactory, ILogger<DatabaseInitializer> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.OpenAsync(cancellationToken).ConfigureAwait(false);
        await EnsureMigrationTableAsync(connection, cancellationToken).ConfigureAwait(false);

        var assembly = typeof(DatabaseInitializer).Assembly;
        var resources = assembly.GetManifestResourceNames()
            .Where(static name => name.Contains(".Persistence.Migrations.", StringComparison.Ordinal) && name.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
            .OrderBy(static name => name, StringComparer.Ordinal)
            .ToArray();

        foreach (var resourceName in resources)
        {
            var fileName = resourceName[(resourceName.LastIndexOf(".Migrations.", StringComparison.Ordinal) + ".Migrations.".Length)..];
            var underscore = fileName.IndexOf('_');
            if (underscore <= 0 || !int.TryParse(fileName[..underscore], out var version))
            {
                throw new InvalidOperationException($"Migration resource '{fileName}' does not start with a numeric version.");
            }

            var sql = await ReadResourceAsync(assembly, resourceName, cancellationToken).ConfigureAwait(false);
            var checksum = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(sql)));
            var appliedChecksum = await GetAppliedChecksumAsync(connection, version, cancellationToken).ConfigureAwait(false);

            if (appliedChecksum is not null)
            {
                if (!string.Equals(appliedChecksum, checksum, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException($"Migration {version} was changed after it had already been applied.");
                }

                continue;
            }

            _logger.LogInformation("Applying database migration {Version}: {MigrationName}", version, fileName);
            using var transaction = connection.BeginTransaction();
            try
            {
                await using (var migrationCommand = connection.CreateCommand())
                {
                    migrationCommand.Transaction = transaction;
                    migrationCommand.CommandText = sql;
                    await migrationCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                }

                await using (var recordCommand = connection.CreateCommand())
                {
                    recordCommand.Transaction = transaction;
                    recordCommand.CommandText = "INSERT INTO SchemaMigrations (Version, Name, AppliedAtUtc, Checksum) VALUES ($version, $name, $applied, $checksum);";
                    recordCommand.Parameters.AddWithValue("$version", version);
                    recordCommand.Parameters.AddWithValue("$name", fileName);
                    recordCommand.Parameters.AddWithValue("$applied", SqliteValue.ToDb(DateTimeOffset.UtcNow));
                    recordCommand.Parameters.AddWithValue("$checksum", checksum);
                    await recordCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                }

                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                await transaction.RollbackAsync(CancellationToken.None).ConfigureAwait(false);
                throw;
            }
        }
    }

    private static async Task EnsureMigrationTableAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS SchemaMigrations (
                Version INTEGER NOT NULL PRIMARY KEY,
                Name TEXT NOT NULL,
                AppliedAtUtc TEXT NOT NULL,
                Checksum TEXT NOT NULL
            );
            """;
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task<string?> GetAppliedChecksumAsync(SqliteConnection connection, int version, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Checksum FROM SchemaMigrations WHERE Version = $version;";
        command.Parameters.AddWithValue("$version", version);
        var value = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return value as string;
    }

    private static async Task<string> ReadResourceAsync(Assembly assembly, string resourceName, CancellationToken cancellationToken)
    {
        await using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded migration '{resourceName}' could not be opened.");
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: false);
        return await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
    }
}
