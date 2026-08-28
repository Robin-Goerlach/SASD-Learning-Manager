using Microsoft.Data.Sqlite;
using SASD.LearningManager.Application.Common;
using SASD.LearningManager.Application.Resources;
using SASD.LearningManager.Domain.Resources;

namespace SASD.LearningManager.Infrastructure.Persistence.Repositories;

/// <summary>
/// SQLite persistence for canonical learning resources. Resource metadata and tag assignments are
/// saved in one transaction so the UI never observes a partially updated resource aggregate.
/// </summary>
public sealed class ResourceRepository : IResourceRepository
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public ResourceRepository(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Resource?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = BaseResourceSelect + " WHERE r.Id = $id;";
        command.Parameters.AddWithValue("$id", id.ToString("D"));
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        return await reader.ReadAsync(cancellationToken).ConfigureAwait(false) ? MapResource(reader) : null;
    }

    public async Task<ResourceDetailDto?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT r.Id, r.Title, r.ResourceType, r.ProviderId, p.Name, r.Url, r.LocalPath,
                   r.Description, r.WhySaved, r.Creator, r.LanguageCode, r.VersionText,
                   r.EstimatedMinutes, r.Difficulty, r.Priority, r.Status, r.ProgressPercent,
                   r.StartedAtUtc, r.CompletedAtUtc, r.CreatedAtUtc, r.UpdatedAtUtc, r.ArchivedAtUtc
            FROM Resources r
            LEFT JOIN Providers p ON p.Id = r.ProviderId
            WHERE r.Id = $id;
            """;
        command.Parameters.AddWithValue("$id", id.ToString("D"));
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        // Copy the scalar values while the reader is active. The reader is then disposed before
        // issuing the tag query on the same SQLite connection. This avoids multiple active readers
        // while keeping the read model strongly typed.
        var resourceId = Guid.Parse(reader.GetString(0));
        var title = reader.GetString(1);
        var type = Enum.Parse<ResourceType>(reader.GetString(2));
        var providerId = SqliteValue.NullableGuid(reader, 3);
        var providerName = SqliteValue.NullableString(reader, 4);
        var url = SqliteValue.NullableString(reader, 5);
        var localPath = SqliteValue.NullableString(reader, 6);
        var description = SqliteValue.NullableString(reader, 7);
        var whySaved = SqliteValue.NullableString(reader, 8);
        var creator = SqliteValue.NullableString(reader, 9);
        var languageCode = SqliteValue.NullableString(reader, 10);
        var versionText = SqliteValue.NullableString(reader, 11);
        var estimatedMinutes = SqliteValue.NullableInt32(reader, 12);
        var difficulty = Enum.Parse<ResourceDifficulty>(reader.GetString(13));
        var priority = Enum.Parse<ResourcePriority>(reader.GetString(14));
        var status = Enum.Parse<ResourceStatus>(reader.GetString(15));
        var progressPercent = SqliteValue.NullableInt32(reader, 16);
        var startedAtUtc = SqliteValue.NullableDateTimeOffset(reader, 17);
        var completedAtUtc = SqliteValue.NullableDateTimeOffset(reader, 18);
        var createdAtUtc = SqliteValue.DateTimeOffset(reader, 19);
        var updatedAtUtc = SqliteValue.DateTimeOffset(reader, 20);
        var archivedAtUtc = SqliteValue.NullableDateTimeOffset(reader, 21);

        await reader.DisposeAsync().ConfigureAwait(false);
        var tags = await GetTagsOnConnectionAsync(connection, id, cancellationToken).ConfigureAwait(false);

        return new ResourceDetailDto(
            resourceId, title, type, providerId, providerName, url, localPath, description, whySaved,
            creator, languageCode, versionText, estimatedMinutes, difficulty, priority, status,
            progressPercent, startedAtUtc, completedAtUtc, createdAtUtc, updatedAtUtc, archivedAtUtc, tags);
    }

    public async Task<Resource?> FindByNormalizedUrlAsync(string normalizedUrl, Guid? excludingId = null, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = BaseResourceSelect + " WHERE r.NormalizedUrl = $url AND ($excludingId IS NULL OR r.Id <> $excludingId) ORDER BY r.CreatedAtUtc LIMIT 1;";
        command.Parameters.AddWithValue("$url", normalizedUrl);
        command.Parameters.AddWithValue("$excludingId", excludingId is null ? DBNull.Value : excludingId.Value.ToString("D"));
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        return await reader.ReadAsync(cancellationToken).ConfigureAwait(false) ? MapResource(reader) : null;
    }

    public async Task<PagedResult<ResourceListItemDto>> SearchAsync(ResourceSearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.OpenAsync(cancellationToken).ConfigureAwait(false);
        var where = new List<string>();
        var parameters = new List<(string Name, object Value)>();

        if (!criteria.IncludeArchived)
        {
            where.Add("r.Status <> 'Archived'");
        }

        if (!string.IsNullOrWhiteSpace(criteria.SearchText))
        {
            where.Add("(r.Title LIKE $search ESCAPE '\\' OR COALESCE(r.Description, '') LIKE $search ESCAPE '\\' OR COALESCE(r.Url, '') LIKE $search ESCAPE '\\' OR COALESCE(p.Name, '') LIKE $search ESCAPE '\\')");
            parameters.Add(("$search", $"%{EscapeLike(criteria.SearchText.Trim())}%"));
        }

        if (criteria.ProviderId is not null)
        {
            where.Add("r.ProviderId = $providerId");
            parameters.Add(("$providerId", criteria.ProviderId.Value.ToString("D")));
        }

        if (criteria.Type is not null)
        {
            where.Add("r.ResourceType = $type");
            parameters.Add(("$type", criteria.Type.Value.ToString()));
        }

        if (criteria.Status is not null)
        {
            where.Add("r.Status = $status");
            parameters.Add(("$status", criteria.Status.Value.ToString()));
        }

        if (criteria.Priority is not null)
        {
            where.Add("r.Priority = $priority");
            parameters.Add(("$priority", criteria.Priority.Value.ToString()));
        }

        var whereClause = where.Count == 0 ? string.Empty : " WHERE " + string.Join(" AND ", where);

        await using var countCommand = connection.CreateCommand();
        countCommand.CommandText = "SELECT COUNT(1) FROM Resources r LEFT JOIN Providers p ON p.Id = r.ProviderId" + whereClause + ";";
        AddParameters(countCommand, parameters);
        var totalCount = Convert.ToInt32(await countCommand.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false), System.Globalization.CultureInfo.InvariantCulture);

        await using var dataCommand = connection.CreateCommand();
        dataCommand.CommandText = """
            SELECT r.Id, r.Title, p.Name, r.ResourceType, r.Status, r.ProgressPercent,
                   r.Priority, r.Difficulty, r.UpdatedAtUtc
            FROM Resources r
            LEFT JOIN Providers p ON p.Id = r.ProviderId
            """ + whereClause + " ORDER BY r.UpdatedAtUtc DESC, r.Title COLLATE NOCASE LIMIT $limit OFFSET $offset;";
        AddParameters(dataCommand, parameters);
        dataCommand.Parameters.AddWithValue("$limit", criteria.PageSize);
        dataCommand.Parameters.AddWithValue("$offset", (criteria.PageNumber - 1) * criteria.PageSize);

        var items = new List<ResourceListItemDto>();
        await using var reader = await dataCommand.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            items.Add(new ResourceListItemDto(
                Guid.Parse(reader.GetString(0)),
                reader.GetString(1),
                SqliteValue.NullableString(reader, 2),
                Enum.Parse<ResourceType>(reader.GetString(3)),
                Enum.Parse<ResourceStatus>(reader.GetString(4)),
                SqliteValue.NullableInt32(reader, 5),
                Enum.Parse<ResourcePriority>(reader.GetString(6)),
                Enum.Parse<ResourceDifficulty>(reader.GetString(7)),
                SqliteValue.DateTimeOffset(reader, 8)));
        }

        return new PagedResult<ResourceListItemDto>(items, criteria.PageNumber, criteria.PageSize, totalCount);
    }

    /// <summary>Returns Inbox resources ordered by capture time, using a projection optimized for classification.</summary>
    public async Task<PagedResult<InboxListItemDto>> SearchInboxAsync(InboxSearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.OpenAsync(cancellationToken).ConfigureAwait(false);
        var where = new List<string> { "r.Status = 'Inbox'" };
        var parameters = new List<(string Name, object Value)>();

        if (!string.IsNullOrWhiteSpace(criteria.SearchText))
        {
            where.Add("(r.Title LIKE $search ESCAPE '\\' OR COALESCE(r.Url, '') LIKE $search ESCAPE '\\' OR COALESCE(r.WhySaved, '') LIKE $search ESCAPE '\\')");
            parameters.Add(("$search", $"%{EscapeLike(criteria.SearchText.Trim())}%"));
        }

        var whereClause = " WHERE " + string.Join(" AND ", where);

        await using var countCommand = connection.CreateCommand();
        countCommand.CommandText = "SELECT COUNT(1) FROM Resources r" + whereClause + ";";
        AddParameters(countCommand, parameters);
        var totalCount = Convert.ToInt32(
            await countCommand.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false),
            System.Globalization.CultureInfo.InvariantCulture);

        await using var dataCommand = connection.CreateCommand();
        dataCommand.CommandText = """
            SELECT r.Id, r.Title, r.Url, p.Name, r.ResourceType, r.WhySaved, r.CreatedAtUtc
            FROM Resources r
            LEFT JOIN Providers p ON p.Id = r.ProviderId
            """ + whereClause + " ORDER BY r.CreatedAtUtc DESC, r.Title COLLATE NOCASE LIMIT $limit OFFSET $offset;";
        AddParameters(dataCommand, parameters);
        dataCommand.Parameters.AddWithValue("$limit", criteria.PageSize);
        dataCommand.Parameters.AddWithValue("$offset", (criteria.PageNumber - 1) * criteria.PageSize);

        var items = new List<InboxListItemDto>();
        await using var reader = await dataCommand.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            items.Add(new InboxListItemDto(
                Guid.Parse(reader.GetString(0)),
                reader.GetString(1),
                SqliteValue.NullableString(reader, 2),
                SqliteValue.NullableString(reader, 3),
                Enum.Parse<ResourceType>(reader.GetString(4)),
                SqliteValue.NullableString(reader, 5),
                SqliteValue.DateTimeOffset(reader, 6)));
        }

        return new PagedResult<InboxListItemDto>(items, criteria.PageNumber, criteria.PageSize, totalCount);
    }

    public async Task<IReadOnlyList<ResourceLookupDto>> ListLookupAsync(bool includeArchived, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = includeArchived
            ? "SELECT r.Id, r.Title, r.ResourceType, r.Status, p.Name FROM Resources r LEFT JOIN Providers p ON p.Id = r.ProviderId ORDER BY r.Title COLLATE NOCASE;"
            : "SELECT r.Id, r.Title, r.ResourceType, r.Status, p.Name FROM Resources r LEFT JOIN Providers p ON p.Id = r.ProviderId WHERE r.Status <> 'Archived' ORDER BY r.Title COLLATE NOCASE;";
        var items = new List<ResourceLookupDto>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            items.Add(new ResourceLookupDto(Guid.Parse(reader.GetString(0)), reader.GetString(1),
                Enum.Parse<ResourceType>(reader.GetString(2)), Enum.Parse<ResourceStatus>(reader.GetString(3)),
                SqliteValue.NullableString(reader, 4)));
        }
        return items;
    }

    public async Task InsertAsync(Resource resource, IReadOnlyCollection<string> tags, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.OpenAsync(cancellationToken).ConfigureAwait(false);
        using var transaction = connection.BeginTransaction();
        try
        {
            await using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = """
                    INSERT INTO Resources (
                        Id, Title, ResourceType, ProviderId, Url, NormalizedUrl, LocalPath, Description, WhySaved,
                        Creator, LanguageCode, VersionText, EstimatedMinutes, Difficulty, Priority, Status,
                        ProgressPercent, StartedAtUtc, CompletedAtUtc, CreatedAtUtc, UpdatedAtUtc, ArchivedAtUtc)
                    VALUES (
                        $id, $title, $type, $providerId, $url, $normalizedUrl, $localPath, $description, $whySaved,
                        $creator, $language, $version, $estimatedMinutes, $difficulty, $priority, $status,
                        $progress, $started, $completed, $created, $updated, $archived);
                    """;
                AddResourceParameters(command, resource);
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            await ReplaceTagsAsync(connection, transaction, resource.Id, tags, cancellationToken).ConfigureAwait(false);
            await WriteActivityAsync(connection, transaction, resource.Id, "ResourceCreated", $"Resource '{resource.Title}' created.", cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None).ConfigureAwait(false);
            throw;
        }
    }

    public async Task UpdateAsync(Resource resource, IReadOnlyCollection<string> tags, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.OpenAsync(cancellationToken).ConfigureAwait(false);
        using var transaction = connection.BeginTransaction();
        try
        {
            await using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = """
                    UPDATE Resources SET
                        Title = $title, ResourceType = $type, ProviderId = $providerId, Url = $url,
                        NormalizedUrl = $normalizedUrl, LocalPath = $localPath, Description = $description,
                        WhySaved = $whySaved, Creator = $creator, LanguageCode = $language, VersionText = $version,
                        EstimatedMinutes = $estimatedMinutes, Difficulty = $difficulty, Priority = $priority,
                        Status = $status, ProgressPercent = $progress, StartedAtUtc = $started,
                        CompletedAtUtc = $completed, UpdatedAtUtc = $updated, ArchivedAtUtc = $archived
                    WHERE Id = $id;
                    """;
                AddResourceParameters(command, resource);
                var affected = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                if (affected != 1)
                {
                    throw new InvalidOperationException($"Resource '{resource.Id}' could not be updated.");
                }
            }

            await ReplaceTagsAsync(connection, transaction, resource.Id, tags, cancellationToken).ConfigureAwait(false);
            await WriteActivityAsync(connection, transaction, resource.Id, "ResourceUpdated", $"Resource '{resource.Title}' updated ({resource.Status}).", cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None).ConfigureAwait(false);
            throw;
        }
    }

    public async Task<IReadOnlyList<string>> GetTagsAsync(Guid resourceId, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.OpenAsync(cancellationToken).ConfigureAwait(false);
        return await GetTagsOnConnectionAsync(connection, resourceId, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<IReadOnlyList<string>> GetTagsOnConnectionAsync(SqliteConnection connection, Guid resourceId, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT t.Name
            FROM Tags t
            INNER JOIN ResourceTags rt ON rt.TagId = t.Id
            WHERE rt.ResourceId = $resourceId
            ORDER BY t.Name COLLATE NOCASE;
            """;
        command.Parameters.AddWithValue("$resourceId", resourceId.ToString("D"));
        var tags = new List<string>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            tags.Add(reader.GetString(0));
        }

        return tags;
    }

    private static async Task ReplaceTagsAsync(SqliteConnection connection, SqliteTransaction transaction, Guid resourceId, IReadOnlyCollection<string> tags, CancellationToken cancellationToken)
    {
        await using (var deleteCommand = connection.CreateCommand())
        {
            deleteCommand.Transaction = transaction;
            deleteCommand.CommandText = "DELETE FROM ResourceTags WHERE ResourceId = $resourceId;";
            deleteCommand.Parameters.AddWithValue("$resourceId", resourceId.ToString("D"));
            await deleteCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        foreach (var tag in tags)
        {
            await using (var insertTag = connection.CreateCommand())
            {
                insertTag.Transaction = transaction;
                insertTag.CommandText = "INSERT OR IGNORE INTO Tags (Id, Name, CreatedAtUtc) VALUES ($id, $name, $created);";
                insertTag.Parameters.AddWithValue("$id", Guid.NewGuid().ToString("D"));
                insertTag.Parameters.AddWithValue("$name", tag);
                insertTag.Parameters.AddWithValue("$created", SqliteValue.ToDb(DateTimeOffset.UtcNow));
                await insertTag.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            await using var linkCommand = connection.CreateCommand();
            linkCommand.Transaction = transaction;
            linkCommand.CommandText = """
                INSERT INTO ResourceTags (ResourceId, TagId)
                SELECT $resourceId, Id FROM Tags WHERE Name = $name COLLATE NOCASE;
                """;
            linkCommand.Parameters.AddWithValue("$resourceId", resourceId.ToString("D"));
            linkCommand.Parameters.AddWithValue("$name", tag);
            await linkCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private static Resource MapResource(SqliteDataReader reader)
    {
        return Resource.Rehydrate(
            Guid.Parse(reader.GetString(0)),
            reader.GetString(1),
            Enum.Parse<ResourceType>(reader.GetString(2)),
            SqliteValue.NullableGuid(reader, 3),
            SqliteValue.NullableString(reader, 4),
            SqliteValue.NullableString(reader, 5),
            SqliteValue.NullableString(reader, 6),
            SqliteValue.NullableString(reader, 7),
            SqliteValue.NullableString(reader, 8),
            SqliteValue.NullableString(reader, 9),
            SqliteValue.NullableString(reader, 10),
            SqliteValue.NullableString(reader, 11),
            SqliteValue.NullableInt32(reader, 12),
            Enum.Parse<ResourceDifficulty>(reader.GetString(13)),
            Enum.Parse<ResourcePriority>(reader.GetString(14)),
            Enum.Parse<ResourceStatus>(reader.GetString(15)),
            SqliteValue.NullableInt32(reader, 16),
            SqliteValue.NullableDateTimeOffset(reader, 17),
            SqliteValue.NullableDateTimeOffset(reader, 18),
            SqliteValue.DateTimeOffset(reader, 19),
            SqliteValue.DateTimeOffset(reader, 20),
            SqliteValue.NullableDateTimeOffset(reader, 21));
    }

    private static void AddResourceParameters(SqliteCommand command, Resource resource)
    {
        command.Parameters.AddWithValue("$id", resource.Id.ToString("D"));
        command.Parameters.AddWithValue("$title", resource.Title);
        command.Parameters.AddWithValue("$type", resource.Type.ToString());
        command.Parameters.AddWithValue("$providerId", SqliteValue.ToDb(resource.ProviderId));
        command.Parameters.AddWithValue("$url", SqliteValue.ToDb(resource.Url));
        command.Parameters.AddWithValue("$normalizedUrl", SqliteValue.ToDb(resource.NormalizedUrl));
        command.Parameters.AddWithValue("$localPath", SqliteValue.ToDb(resource.LocalPath));
        command.Parameters.AddWithValue("$description", SqliteValue.ToDb(resource.Description));
        command.Parameters.AddWithValue("$whySaved", SqliteValue.ToDb(resource.WhySaved));
        command.Parameters.AddWithValue("$creator", SqliteValue.ToDb(resource.Creator));
        command.Parameters.AddWithValue("$language", SqliteValue.ToDb(resource.LanguageCode));
        command.Parameters.AddWithValue("$version", SqliteValue.ToDb(resource.VersionText));
        command.Parameters.AddWithValue("$estimatedMinutes", SqliteValue.ToDb(resource.EstimatedMinutes));
        command.Parameters.AddWithValue("$difficulty", resource.Difficulty.ToString());
        command.Parameters.AddWithValue("$priority", resource.Priority.ToString());
        command.Parameters.AddWithValue("$status", resource.Status.ToString());
        command.Parameters.AddWithValue("$progress", SqliteValue.ToDb(resource.ProgressPercent));
        command.Parameters.AddWithValue("$started", SqliteValue.ToDb(resource.StartedAtUtc));
        command.Parameters.AddWithValue("$completed", SqliteValue.ToDb(resource.CompletedAtUtc));
        command.Parameters.AddWithValue("$created", SqliteValue.ToDb(resource.CreatedAtUtc));
        command.Parameters.AddWithValue("$updated", SqliteValue.ToDb(resource.UpdatedAtUtc));
        command.Parameters.AddWithValue("$archived", SqliteValue.ToDb(resource.ArchivedAtUtc));
    }

    private static void AddParameters(SqliteCommand command, IEnumerable<(string Name, object Value)> parameters)
    {
        foreach (var parameter in parameters)
        {
            command.Parameters.AddWithValue(parameter.Name, parameter.Value);
        }
    }

    private static string EscapeLike(string value) => value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("%", "\\%", StringComparison.Ordinal).Replace("_", "\\_", StringComparison.Ordinal);

    private static async Task WriteActivityAsync(SqliteConnection connection, SqliteTransaction transaction, Guid id, string type, string summary, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "INSERT INTO ActivityLog (Id, EntityType, EntityId, ActivityType, OccurredAtUtc, Summary) VALUES ($id, 'Resource', $entityId, $type, $occurred, $summary);";
        command.Parameters.AddWithValue("$id", Guid.NewGuid().ToString("D"));
        command.Parameters.AddWithValue("$entityId", id.ToString("D"));
        command.Parameters.AddWithValue("$type", type);
        command.Parameters.AddWithValue("$occurred", SqliteValue.ToDb(DateTimeOffset.UtcNow));
        command.Parameters.AddWithValue("$summary", summary);
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private const string BaseResourceSelect = """
        SELECT r.Id, r.Title, r.ResourceType, r.ProviderId, r.Url, r.NormalizedUrl, r.LocalPath,
               r.Description, r.WhySaved, r.Creator, r.LanguageCode, r.VersionText,
               r.EstimatedMinutes, r.Difficulty, r.Priority, r.Status, r.ProgressPercent,
               r.StartedAtUtc, r.CompletedAtUtc, r.CreatedAtUtc, r.UpdatedAtUtc, r.ArchivedAtUtc
        FROM Resources r
        """;
}
