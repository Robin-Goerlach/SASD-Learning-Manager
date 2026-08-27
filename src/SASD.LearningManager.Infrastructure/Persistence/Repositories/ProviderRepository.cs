using Microsoft.Data.Sqlite;
using SASD.LearningManager.Application.Providers;
using SASD.LearningManager.Domain.Providers;

namespace SASD.LearningManager.Infrastructure.Persistence.Repositories;

/// <summary>SQLite implementation of provider persistence.</summary>
public sealed class ProviderRepository : IProviderRepository
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public ProviderRepository(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Provider?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, Name, WebsiteUrl, Description, ProviderType, Status, CreatedAtUtc, UpdatedAtUtc, ArchivedAtUtc
            FROM Providers WHERE Id = $id;
            """;
        command.Parameters.AddWithValue("$id", id.ToString("D"));
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        return await reader.ReadAsync(cancellationToken).ConfigureAwait(false) ? Map(reader) : null;
    }

    public async Task<IReadOnlyList<ProviderListItemDto>> ListAsync(bool includeArchived, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = includeArchived
            ? "SELECT Id, Name, ProviderType, Status, WebsiteUrl FROM Providers ORDER BY Name COLLATE NOCASE;"
            : "SELECT Id, Name, ProviderType, Status, WebsiteUrl FROM Providers WHERE Status <> 'Archived' ORDER BY Name COLLATE NOCASE;";

        var items = new List<ProviderListItemDto>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            items.Add(new ProviderListItemDto(
                Guid.Parse(reader.GetString(0)),
                reader.GetString(1),
                Enum.Parse<ProviderType>(reader.GetString(2), ignoreCase: false),
                Enum.Parse<ProviderStatus>(reader.GetString(3), ignoreCase: false),
                SqliteValue.NullableString(reader, 4)));
        }

        return items;
    }

    public async Task<bool> NameExistsAsync(string name, Guid? excludingId = null, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = excludingId is null
            ? "SELECT COUNT(1) FROM Providers WHERE Name = $name COLLATE NOCASE;"
            : "SELECT COUNT(1) FROM Providers WHERE Name = $name COLLATE NOCASE AND Id <> $id;";
        command.Parameters.AddWithValue("$name", name);
        if (excludingId is not null)
        {
            command.Parameters.AddWithValue("$id", excludingId.Value.ToString("D"));
        }

        var count = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false), System.Globalization.CultureInfo.InvariantCulture);
        return count > 0;
    }

    public async Task InsertAsync(Provider provider, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.OpenAsync(cancellationToken).ConfigureAwait(false);
        using var transaction = connection.BeginTransaction();
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO Providers (Id, Name, WebsiteUrl, Description, ProviderType, Status, CreatedAtUtc, UpdatedAtUtc, ArchivedAtUtc)
            VALUES ($id, $name, $website, $description, $type, $status, $created, $updated, $archived);
            """;
        AddParameters(command, provider);
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        await WriteActivityAsync(connection, transaction, provider.Id, "ProviderCreated", $"Provider '{provider.Name}' created.", cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAsync(Provider provider, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.OpenAsync(cancellationToken).ConfigureAwait(false);
        using var transaction = connection.BeginTransaction();
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            UPDATE Providers
            SET Name = $name, WebsiteUrl = $website, Description = $description,
                ProviderType = $type, Status = $status, UpdatedAtUtc = $updated, ArchivedAtUtc = $archived
            WHERE Id = $id;
            """;
        AddParameters(command, provider);
        var affected = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        if (affected != 1)
        {
            throw new InvalidOperationException($"Provider '{provider.Id}' could not be updated.");
        }

        await WriteActivityAsync(connection, transaction, provider.Id, "ProviderUpdated", $"Provider '{provider.Name}' updated ({provider.Status}).", cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    private static Provider Map(SqliteDataReader reader)
    {
        return Provider.Rehydrate(
            Guid.Parse(reader.GetString(0)),
            reader.GetString(1),
            SqliteValue.NullableString(reader, 2),
            SqliteValue.NullableString(reader, 3),
            Enum.Parse<ProviderType>(reader.GetString(4)),
            Enum.Parse<ProviderStatus>(reader.GetString(5)),
            SqliteValue.DateTimeOffset(reader, 6),
            SqliteValue.DateTimeOffset(reader, 7),
            SqliteValue.NullableDateTimeOffset(reader, 8));
    }

    private static void AddParameters(SqliteCommand command, Provider provider)
    {
        command.Parameters.AddWithValue("$id", provider.Id.ToString("D"));
        command.Parameters.AddWithValue("$name", provider.Name);
        command.Parameters.AddWithValue("$website", SqliteValue.ToDb(provider.WebsiteUrl));
        command.Parameters.AddWithValue("$description", SqliteValue.ToDb(provider.Description));
        command.Parameters.AddWithValue("$type", provider.Type.ToString());
        command.Parameters.AddWithValue("$status", provider.Status.ToString());
        command.Parameters.AddWithValue("$created", SqliteValue.ToDb(provider.CreatedAtUtc));
        command.Parameters.AddWithValue("$updated", SqliteValue.ToDb(provider.UpdatedAtUtc));
        command.Parameters.AddWithValue("$archived", SqliteValue.ToDb(provider.ArchivedAtUtc));
    }

    private static async Task WriteActivityAsync(SqliteConnection connection, SqliteTransaction transaction, Guid id, string type, string summary, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "INSERT INTO ActivityLog (Id, EntityType, EntityId, ActivityType, OccurredAtUtc, Summary) VALUES ($id, 'Provider', $entityId, $type, $occurred, $summary);";
        command.Parameters.AddWithValue("$id", Guid.NewGuid().ToString("D"));
        command.Parameters.AddWithValue("$entityId", id.ToString("D"));
        command.Parameters.AddWithValue("$type", type);
        command.Parameters.AddWithValue("$occurred", SqliteValue.ToDb(DateTimeOffset.UtcNow));
        command.Parameters.AddWithValue("$summary", summary);
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }
}
