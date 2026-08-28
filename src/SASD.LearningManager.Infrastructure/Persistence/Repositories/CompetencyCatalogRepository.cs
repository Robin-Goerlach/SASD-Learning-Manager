using Microsoft.Data.Sqlite;
using SASD.LearningManager.Application.Competencies;
using SASD.LearningManager.Domain.Competencies;

namespace SASD.LearningManager.Infrastructure.Persistence.Repositories;

/// <summary>SQLite persistence for competency areas, topics and their many-to-many relation.</summary>
public sealed class CompetencyCatalogRepository : ICompetencyCatalogRepository
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public CompetencyCatalogRepository(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<CompetencyAreaListItemDto>> ListAreasAsync(bool includeArchived, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = includeArchived
            ? "SELECT Id, Name, Description, Status FROM CompetencyAreas ORDER BY Name COLLATE NOCASE;"
            : "SELECT Id, Name, Description, Status FROM CompetencyAreas WHERE Status <> 'Archived' ORDER BY Name COLLATE NOCASE;";

        var items = new List<CompetencyAreaListItemDto>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            items.Add(new CompetencyAreaListItemDto(
                Guid.Parse(reader.GetString(0)), reader.GetString(1), SqliteValue.NullableString(reader, 2),
                Enum.Parse<CatalogStatus>(reader.GetString(3))));
        }

        return items;
    }

    public async Task<CompetencyArea?> GetAreaByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name, Description, Status, CreatedAtUtc, UpdatedAtUtc, ArchivedAtUtc FROM CompetencyAreas WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", id.ToString("D"));
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        return await reader.ReadAsync(cancellationToken).ConfigureAwait(false) ? MapArea(reader) : null;
    }

    public Task<bool> AreaNameExistsAsync(string name, Guid? excludingId = null, CancellationToken cancellationToken = default)
        => NameExistsAsync("CompetencyAreas", name, excludingId, cancellationToken);

    public async Task InsertAreaAsync(CompetencyArea area, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.OpenAsync(cancellationToken).ConfigureAwait(false);
        using var transaction = connection.BeginTransaction();
        try
        {
            await using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = """
                INSERT INTO CompetencyAreas (Id, Name, Description, Status, CreatedAtUtc, UpdatedAtUtc, ArchivedAtUtc)
                VALUES ($id, $name, $description, $status, $created, $updated, $archived);
                """;
            AddAreaParameters(command, area);
            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            await WriteActivityAsync(connection, transaction, "CompetencyArea", area.Id, "CompetencyAreaCreated", $"Competency area '{area.Name}' created.", cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None).ConfigureAwait(false);
            throw;
        }
    }

    public async Task UpdateAreaAsync(CompetencyArea area, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.OpenAsync(cancellationToken).ConfigureAwait(false);
        using var transaction = connection.BeginTransaction();
        try
        {
            await using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = """
                UPDATE CompetencyAreas
                SET Name = $name, Description = $description, Status = $status,
                    UpdatedAtUtc = $updated, ArchivedAtUtc = $archived
                WHERE Id = $id;
                """;
            AddAreaParameters(command, area);
            if (await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false) != 1)
            {
                throw new InvalidOperationException($"Competency area '{area.Id}' could not be updated.");
            }

            await WriteActivityAsync(connection, transaction, "CompetencyArea", area.Id, "CompetencyAreaUpdated", $"Competency area '{area.Name}' updated ({area.Status}).", cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None).ConfigureAwait(false);
            throw;
        }
    }

    public async Task<IReadOnlyList<TopicListItemDto>> ListTopicsAsync(bool includeArchived, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT t.Id, t.Name, t.Description,
                   COALESCE((
                       SELECT GROUP_CONCAT(x.Name, ', ')
                       FROM (
                           SELECT ca.Name AS Name
                           FROM CompetencyAreaTopics cat
                           INNER JOIN CompetencyAreas ca ON ca.Id = cat.CompetencyAreaId
                           WHERE cat.TopicId = t.Id
                           ORDER BY ca.Name COLLATE NOCASE
                       ) x
                   ), '') AS CompetencyAreas,
                   t.Status
            FROM Topics t
            WHERE ($includeArchived = 1 OR t.Status <> 'Archived')
            ORDER BY t.Name COLLATE NOCASE;
            """;
        command.Parameters.AddWithValue("$includeArchived", includeArchived ? 1 : 0);

        var items = new List<TopicListItemDto>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            items.Add(new TopicListItemDto(
                Guid.Parse(reader.GetString(0)), reader.GetString(1), SqliteValue.NullableString(reader, 2),
                reader.GetString(3), Enum.Parse<CatalogStatus>(reader.GetString(4))));
        }

        return items;
    }

    public async Task<Topic?> GetTopicByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name, Description, Status, CreatedAtUtc, UpdatedAtUtc, ArchivedAtUtc FROM Topics WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", id.ToString("D"));
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        return await reader.ReadAsync(cancellationToken).ConfigureAwait(false) ? MapTopic(reader) : null;
    }

    public async Task<TopicDetailDto?> GetTopicDetailAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var topic = await GetTopicByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (topic is null) return null;

        await using var connection = await _connectionFactory.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT CompetencyAreaId FROM CompetencyAreaTopics WHERE TopicId = $id ORDER BY CompetencyAreaId;";
        command.Parameters.AddWithValue("$id", id.ToString("D"));
        var areaIds = new List<Guid>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            areaIds.Add(Guid.Parse(reader.GetString(0)));
        }

        return new TopicDetailDto(topic.Id, topic.Name, topic.Description, topic.Status, areaIds);
    }

    public Task<bool> TopicNameExistsAsync(string name, Guid? excludingId = null, CancellationToken cancellationToken = default)
        => NameExistsAsync("Topics", name, excludingId, cancellationToken);

    public Task InsertTopicAsync(Topic topic, IReadOnlyCollection<Guid> competencyAreaIds, CancellationToken cancellationToken = default)
        => SaveTopicAsync(topic, competencyAreaIds, insert: true, cancellationToken);

    public Task UpdateTopicAsync(Topic topic, IReadOnlyCollection<Guid> competencyAreaIds, CancellationToken cancellationToken = default)
        => SaveTopicAsync(topic, competencyAreaIds, insert: false, cancellationToken);

    private async Task SaveTopicAsync(Topic topic, IReadOnlyCollection<Guid> areaIds, bool insert, CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.OpenAsync(cancellationToken).ConfigureAwait(false);
        using var transaction = connection.BeginTransaction();
        try
        {
            await using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = insert
                    ? "INSERT INTO Topics (Id, Name, Description, Status, CreatedAtUtc, UpdatedAtUtc, ArchivedAtUtc) VALUES ($id, $name, $description, $status, $created, $updated, $archived);"
                    : "UPDATE Topics SET Name = $name, Description = $description, Status = $status, UpdatedAtUtc = $updated, ArchivedAtUtc = $archived WHERE Id = $id;";
                AddTopicParameters(command, topic);
                var affected = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                if (affected != 1)
                {
                    throw new InvalidOperationException($"Topic '{topic.Id}' could not be {(insert ? "created" : "updated")}.");
                }
            }

            await ReplaceTopicAreasAsync(connection, transaction, topic.Id, areaIds, cancellationToken).ConfigureAwait(false);
            var action = insert ? "created" : $"updated ({topic.Status})";
            await WriteActivityAsync(connection, transaction, "Topic", topic.Id,
                insert ? "TopicCreated" : "TopicUpdated", $"Topic '{topic.Name}' {action}.", cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None).ConfigureAwait(false);
            throw;
        }
    }

    private async Task<bool> NameExistsAsync(string tableName, string name, Guid? excludingId, CancellationToken cancellationToken)
    {
        if (tableName != "CompetencyAreas" && tableName != "Topics")
        {
            throw new ArgumentOutOfRangeException(nameof(tableName));
        }

        await using var connection = await _connectionFactory.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        // tableName is selected exclusively from the hard-coded allowlist above, never from user input.
        command.CommandText = excludingId is null
            ? $"SELECT COUNT(1) FROM {tableName} WHERE Name = $name COLLATE NOCASE;"
            : $"SELECT COUNT(1) FROM {tableName} WHERE Name = $name COLLATE NOCASE AND Id <> $id;";
        command.Parameters.AddWithValue("$name", name.Trim());
        if (excludingId is not null) command.Parameters.AddWithValue("$id", excludingId.Value.ToString("D"));
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false), System.Globalization.CultureInfo.InvariantCulture) > 0;
    }

    private static async Task ReplaceTopicAreasAsync(SqliteConnection connection, SqliteTransaction transaction, Guid topicId, IEnumerable<Guid> areaIds, CancellationToken cancellationToken)
    {
        await using (var delete = connection.CreateCommand())
        {
            delete.Transaction = transaction;
            delete.CommandText = "DELETE FROM CompetencyAreaTopics WHERE TopicId = $topicId;";
            delete.Parameters.AddWithValue("$topicId", topicId.ToString("D"));
            await delete.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        foreach (var areaId in areaIds.Distinct())
        {
            await using var insert = connection.CreateCommand();
            insert.Transaction = transaction;
            insert.CommandText = "INSERT INTO CompetencyAreaTopics (CompetencyAreaId, TopicId) VALUES ($areaId, $topicId);";
            insert.Parameters.AddWithValue("$areaId", areaId.ToString("D"));
            insert.Parameters.AddWithValue("$topicId", topicId.ToString("D"));
            await insert.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private static CompetencyArea MapArea(SqliteDataReader reader)
        => CompetencyArea.Rehydrate(Guid.Parse(reader.GetString(0)), reader.GetString(1), SqliteValue.NullableString(reader, 2),
            Enum.Parse<CatalogStatus>(reader.GetString(3)), SqliteValue.DateTimeOffset(reader, 4),
            SqliteValue.DateTimeOffset(reader, 5), SqliteValue.NullableDateTimeOffset(reader, 6));

    private static Topic MapTopic(SqliteDataReader reader)
        => Topic.Rehydrate(Guid.Parse(reader.GetString(0)), reader.GetString(1), SqliteValue.NullableString(reader, 2),
            Enum.Parse<CatalogStatus>(reader.GetString(3)), SqliteValue.DateTimeOffset(reader, 4),
            SqliteValue.DateTimeOffset(reader, 5), SqliteValue.NullableDateTimeOffset(reader, 6));

    private static void AddAreaParameters(SqliteCommand command, CompetencyArea area)
    {
        command.Parameters.AddWithValue("$id", area.Id.ToString("D"));
        command.Parameters.AddWithValue("$name", area.Name);
        command.Parameters.AddWithValue("$description", SqliteValue.ToDb(area.Description));
        command.Parameters.AddWithValue("$status", area.Status.ToString());
        command.Parameters.AddWithValue("$created", SqliteValue.ToDb(area.CreatedAtUtc));
        command.Parameters.AddWithValue("$updated", SqliteValue.ToDb(area.UpdatedAtUtc));
        command.Parameters.AddWithValue("$archived", SqliteValue.ToDb(area.ArchivedAtUtc));
    }

    private static void AddTopicParameters(SqliteCommand command, Topic topic)
    {
        command.Parameters.AddWithValue("$id", topic.Id.ToString("D"));
        command.Parameters.AddWithValue("$name", topic.Name);
        command.Parameters.AddWithValue("$description", SqliteValue.ToDb(topic.Description));
        command.Parameters.AddWithValue("$status", topic.Status.ToString());
        command.Parameters.AddWithValue("$created", SqliteValue.ToDb(topic.CreatedAtUtc));
        command.Parameters.AddWithValue("$updated", SqliteValue.ToDb(topic.UpdatedAtUtc));
        command.Parameters.AddWithValue("$archived", SqliteValue.ToDb(topic.ArchivedAtUtc));
    }

    private static async Task WriteActivityAsync(SqliteConnection connection, SqliteTransaction transaction, string entityType, Guid entityId, string activityType, string summary, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "INSERT INTO ActivityLog (Id, EntityType, EntityId, ActivityType, OccurredAtUtc, Summary) VALUES ($id, $entityType, $entityId, $type, $occurred, $summary);";
        command.Parameters.AddWithValue("$id", Guid.NewGuid().ToString("D"));
        command.Parameters.AddWithValue("$entityType", entityType);
        command.Parameters.AddWithValue("$entityId", entityId.ToString("D"));
        command.Parameters.AddWithValue("$type", activityType);
        command.Parameters.AddWithValue("$occurred", SqliteValue.ToDb(DateTimeOffset.UtcNow));
        command.Parameters.AddWithValue("$summary", summary);
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }
}
