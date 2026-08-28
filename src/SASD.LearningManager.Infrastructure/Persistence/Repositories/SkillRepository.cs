using Microsoft.Data.Sqlite;
using SASD.LearningManager.Application.Common;
using SASD.LearningManager.Application.Skills;
using SASD.LearningManager.Domain.Skills;

namespace SASD.LearningManager.Infrastructure.Persistence.Repositories;

/// <summary>SQLite persistence for skills, taxonomy links and immutable assessment history.</summary>
public sealed class SkillRepository : ISkillRepository
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public SkillRepository(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Skill?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name, Description, CurrentLevel, TargetLevel, Status, CreatedAtUtc, UpdatedAtUtc, ArchivedAtUtc FROM Skills WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", id.ToString("D"));
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        return await reader.ReadAsync(cancellationToken).ConfigureAwait(false) ? MapSkill(reader) : null;
    }

    public async Task<SkillDetailDto?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var skill = await GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (skill is null) return null;

        await using var connection = await _connectionFactory.OpenAsync(cancellationToken).ConfigureAwait(false);
        var areaIds = await ReadIdsAsync(connection,
            "SELECT CompetencyAreaId FROM CompetencyAreaSkills WHERE SkillId = $id ORDER BY CompetencyAreaId;", id, cancellationToken).ConfigureAwait(false);
        var topicIds = await ReadIdsAsync(connection,
            "SELECT TopicId FROM TopicSkills WHERE SkillId = $id ORDER BY TopicId;", id, cancellationToken).ConfigureAwait(false);

        return new SkillDetailDto(skill.Id, skill.Name, skill.Description, skill.CurrentLevel, skill.TargetLevel,
            skill.Status, skill.CreatedAtUtc, skill.UpdatedAtUtc, skill.ArchivedAtUtc, areaIds, topicIds);
    }

    public async Task<bool> NameExistsAsync(string name, Guid? excludingId = null, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = excludingId is null
            ? "SELECT COUNT(1) FROM Skills WHERE Name = $name COLLATE NOCASE;"
            : "SELECT COUNT(1) FROM Skills WHERE Name = $name COLLATE NOCASE AND Id <> $id;";
        command.Parameters.AddWithValue("$name", name.Trim());
        if (excludingId is not null) command.Parameters.AddWithValue("$id", excludingId.Value.ToString("D"));
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false), System.Globalization.CultureInfo.InvariantCulture) > 0;
    }

    public async Task<PagedResult<SkillListItemDto>> SearchAsync(SkillSearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.OpenAsync(cancellationToken).ConfigureAwait(false);
        var filters = new List<string>();
        if (!criteria.IncludeArchived) filters.Add("s.Status <> 'Archived'");
        if (criteria.Status is not null) filters.Add("s.Status = $status");
        if (!string.IsNullOrWhiteSpace(criteria.SearchText))
        {
            filters.Add("(s.Name LIKE $search ESCAPE '\\' COLLATE NOCASE OR s.Description LIKE $search ESCAPE '\\' COLLATE NOCASE)");
        }

        var where = filters.Count == 0 ? string.Empty : "WHERE " + string.Join(" AND ", filters);
        await using var count = connection.CreateCommand();
        count.CommandText = $"SELECT COUNT(1) FROM Skills s {where};";
        AddSearchParameters(count, criteria);
        var total = Convert.ToInt32(await count.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false), System.Globalization.CultureInfo.InvariantCulture);

        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            SELECT s.Id, s.Name, s.CurrentLevel, s.TargetLevel,
                   CASE WHEN s.CurrentLevel IS NULL OR s.TargetLevel IS NULL THEN NULL ELSE s.TargetLevel - s.CurrentLevel END AS Gap,
                   COALESCE((
                       SELECT GROUP_CONCAT(x.Name, ', ')
                       FROM (
                           SELECT ca.Name AS Name
                           FROM CompetencyAreaSkills cas
                           INNER JOIN CompetencyAreas ca ON ca.Id = cas.CompetencyAreaId
                           WHERE cas.SkillId = s.Id
                           ORDER BY ca.Name COLLATE NOCASE
                       ) x
                   ), '') AS Areas,
                   COALESCE((
                       SELECT GROUP_CONCAT(x.Name, ', ')
                       FROM (
                           SELECT t.Name AS Name
                           FROM TopicSkills ts
                           INNER JOIN Topics t ON t.Id = ts.TopicId
                           WHERE ts.SkillId = s.Id
                           ORDER BY t.Name COLLATE NOCASE
                       ) x
                   ), '') AS Topics,
                   s.Status, s.UpdatedAtUtc
            FROM Skills s
            {where}
            ORDER BY s.Name COLLATE NOCASE
            LIMIT $limit OFFSET $offset;
            """;
        AddSearchParameters(command, criteria);
        command.Parameters.AddWithValue("$limit", criteria.PageSize);
        command.Parameters.AddWithValue("$offset", (criteria.PageNumber - 1) * criteria.PageSize);

        var items = new List<SkillListItemDto>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            items.Add(new SkillListItemDto(
                Guid.Parse(reader.GetString(0)), reader.GetString(1), SqliteValue.NullableInt32(reader, 2),
                SqliteValue.NullableInt32(reader, 3), SqliteValue.NullableInt32(reader, 4), reader.GetString(5),
                reader.GetString(6), Enum.Parse<SkillStatus>(reader.GetString(7)), SqliteValue.DateTimeOffset(reader, 8)));
        }

        return new PagedResult<SkillListItemDto>(items, criteria.PageNumber, criteria.PageSize, total);
    }

    public async Task<IReadOnlyList<SkillLookupDto>> ListLookupAsync(bool includeArchived, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = includeArchived
            ? "SELECT Id, Name, Status, CurrentLevel, TargetLevel FROM Skills ORDER BY Name COLLATE NOCASE;"
            : "SELECT Id, Name, Status, CurrentLevel, TargetLevel FROM Skills WHERE Status <> 'Archived' ORDER BY Name COLLATE NOCASE;";
        var items = new List<SkillLookupDto>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            items.Add(new SkillLookupDto(Guid.Parse(reader.GetString(0)), reader.GetString(1),
                Enum.Parse<SkillStatus>(reader.GetString(2)), SqliteValue.NullableInt32(reader, 3), SqliteValue.NullableInt32(reader, 4)));
        }

        return items;
    }

    public async Task<IReadOnlyList<SkillAssessmentListItemDto>> ListAssessmentsAsync(Guid skillId, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, Level, AssessmentType, Reason, AssessedAtUtc
            FROM SkillAssessments
            WHERE SkillId = $skillId
            ORDER BY AssessedAtUtc DESC, CreatedAtUtc DESC;
            """;
        command.Parameters.AddWithValue("$skillId", skillId.ToString("D"));
        var items = new List<SkillAssessmentListItemDto>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            items.Add(new SkillAssessmentListItemDto(Guid.Parse(reader.GetString(0)), reader.GetInt32(1),
                Enum.Parse<SkillAssessmentType>(reader.GetString(2)), SqliteValue.NullableString(reader, 3),
                SqliteValue.DateTimeOffset(reader, 4)));
        }

        return items;
    }

    public Task InsertAsync(Skill skill, IReadOnlyCollection<Guid> competencyAreaIds, IReadOnlyCollection<Guid> topicIds, CancellationToken cancellationToken = default)
        => SaveAsync(skill, competencyAreaIds, topicIds, insert: true, cancellationToken);

    public Task UpdateAsync(Skill skill, IReadOnlyCollection<Guid> competencyAreaIds, IReadOnlyCollection<Guid> topicIds, CancellationToken cancellationToken = default)
        => SaveAsync(skill, competencyAreaIds, topicIds, insert: false, cancellationToken);

    public async Task AddAssessmentAsync(Skill skill, SkillAssessment assessment, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.OpenAsync(cancellationToken).ConfigureAwait(false);
        using var transaction = connection.BeginTransaction();
        try
        {
            await using (var update = connection.CreateCommand())
            {
                update.Transaction = transaction;
                update.CommandText = "UPDATE Skills SET CurrentLevel = $level, UpdatedAtUtc = $updated WHERE Id = $id;";
                update.Parameters.AddWithValue("$level", assessment.Level);
                update.Parameters.AddWithValue("$updated", SqliteValue.ToDb(skill.UpdatedAtUtc));
                update.Parameters.AddWithValue("$id", skill.Id.ToString("D"));
                if (await update.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false) != 1)
                {
                    throw new InvalidOperationException($"Skill '{skill.Id}' could not be updated for assessment.");
                }
            }

            await using (var insert = connection.CreateCommand())
            {
                insert.Transaction = transaction;
                insert.CommandText = """
                    INSERT INTO SkillAssessments (Id, SkillId, Level, AssessmentType, Reason, AssessedAtUtc, CreatedAtUtc)
                    VALUES ($id, $skillId, $level, $type, $reason, $assessed, $created);
                    """;
                insert.Parameters.AddWithValue("$id", assessment.Id.ToString("D"));
                insert.Parameters.AddWithValue("$skillId", assessment.SkillId.ToString("D"));
                insert.Parameters.AddWithValue("$level", assessment.Level);
                insert.Parameters.AddWithValue("$type", assessment.Type.ToString());
                insert.Parameters.AddWithValue("$reason", SqliteValue.ToDb(assessment.Reason));
                insert.Parameters.AddWithValue("$assessed", SqliteValue.ToDb(assessment.AssessedAtUtc));
                insert.Parameters.AddWithValue("$created", SqliteValue.ToDb(assessment.CreatedAtUtc));
                await insert.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            await WriteActivityAsync(connection, transaction, skill.Id, "SkillAssessed", $"Skill '{skill.Name}' assessed at level {assessment.Level}.", cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None).ConfigureAwait(false);
            throw;
        }
    }

    private async Task SaveAsync(Skill skill, IReadOnlyCollection<Guid> areaIds, IReadOnlyCollection<Guid> topicIds, bool insert, CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.OpenAsync(cancellationToken).ConfigureAwait(false);
        using var transaction = connection.BeginTransaction();
        try
        {
            await using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = insert
                    ? "INSERT INTO Skills (Id, Name, Description, CurrentLevel, TargetLevel, Status, CreatedAtUtc, UpdatedAtUtc, ArchivedAtUtc) VALUES ($id, $name, $description, $current, $target, $status, $created, $updated, $archived);"
                    : "UPDATE Skills SET Name = $name, Description = $description, CurrentLevel = $current, TargetLevel = $target, Status = $status, UpdatedAtUtc = $updated, ArchivedAtUtc = $archived WHERE Id = $id;";
                AddSkillParameters(command, skill);
                var affected = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                if (affected != 1)
                {
                    throw new InvalidOperationException($"Skill '{skill.Id}' could not be {(insert ? "created" : "updated")}.");
                }
            }

            await ReplaceLinksAsync(connection, transaction, "CompetencyAreaSkills", "CompetencyAreaId", skill.Id, areaIds, cancellationToken).ConfigureAwait(false);
            await ReplaceLinksAsync(connection, transaction, "TopicSkills", "TopicId", skill.Id, topicIds, cancellationToken).ConfigureAwait(false);
            var action = insert ? "created" : $"updated ({skill.Status})";
            await WriteActivityAsync(connection, transaction, skill.Id, insert ? "SkillCreated" : "SkillUpdated",
                $"Skill '{skill.Name}' {action}.", cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None).ConfigureAwait(false);
            throw;
        }
    }

    private static async Task ReplaceLinksAsync(SqliteConnection connection, SqliteTransaction transaction, string table, string foreignColumn,
        Guid skillId, IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        if (!((table == "CompetencyAreaSkills" && foreignColumn == "CompetencyAreaId") ||
            (table == "TopicSkills" && foreignColumn == "TopicId")))
        {
            throw new ArgumentOutOfRangeException(nameof(table));
        }

        await using (var delete = connection.CreateCommand())
        {
            delete.Transaction = transaction;
            delete.CommandText = $"DELETE FROM {table} WHERE SkillId = $skillId;";
            delete.Parameters.AddWithValue("$skillId", skillId.ToString("D"));
            await delete.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        foreach (var id in ids.Distinct())
        {
            await using var insert = connection.CreateCommand();
            insert.Transaction = transaction;
            insert.CommandText = $"INSERT INTO {table} ({foreignColumn}, SkillId) VALUES ($foreignId, $skillId);";
            insert.Parameters.AddWithValue("$foreignId", id.ToString("D"));
            insert.Parameters.AddWithValue("$skillId", skillId.ToString("D"));
            await insert.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private static async Task<IReadOnlyList<Guid>> ReadIdsAsync(SqliteConnection connection, string sql, Guid id, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$id", id.ToString("D"));
        var ids = new List<Guid>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false)) ids.Add(Guid.Parse(reader.GetString(0)));
        return ids;
    }

    private static Skill MapSkill(SqliteDataReader reader)
        => Skill.Rehydrate(Guid.Parse(reader.GetString(0)), reader.GetString(1), SqliteValue.NullableString(reader, 2),
            SqliteValue.NullableInt32(reader, 3), SqliteValue.NullableInt32(reader, 4), Enum.Parse<SkillStatus>(reader.GetString(5)),
            SqliteValue.DateTimeOffset(reader, 6), SqliteValue.DateTimeOffset(reader, 7), SqliteValue.NullableDateTimeOffset(reader, 8));

    private static void AddSkillParameters(SqliteCommand command, Skill skill)
    {
        command.Parameters.AddWithValue("$id", skill.Id.ToString("D"));
        command.Parameters.AddWithValue("$name", skill.Name);
        command.Parameters.AddWithValue("$description", SqliteValue.ToDb(skill.Description));
        command.Parameters.AddWithValue("$current", SqliteValue.ToDb(skill.CurrentLevel));
        command.Parameters.AddWithValue("$target", SqliteValue.ToDb(skill.TargetLevel));
        command.Parameters.AddWithValue("$status", skill.Status.ToString());
        command.Parameters.AddWithValue("$created", SqliteValue.ToDb(skill.CreatedAtUtc));
        command.Parameters.AddWithValue("$updated", SqliteValue.ToDb(skill.UpdatedAtUtc));
        command.Parameters.AddWithValue("$archived", SqliteValue.ToDb(skill.ArchivedAtUtc));
    }

    private static void AddSearchParameters(SqliteCommand command, SkillSearchCriteria criteria)
    {
        if (criteria.Status is not null) command.Parameters.AddWithValue("$status", criteria.Status.Value.ToString());
        if (!string.IsNullOrWhiteSpace(criteria.SearchText))
        {
            command.Parameters.AddWithValue("$search", $"%{EscapeLike(criteria.SearchText.Trim())}%");
        }
    }

    private static string EscapeLike(string value)
        => value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("%", "\\%", StringComparison.Ordinal).Replace("_", "\\_", StringComparison.Ordinal);

    private static async Task WriteActivityAsync(SqliteConnection connection, SqliteTransaction transaction, Guid id, string type, string summary, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "INSERT INTO ActivityLog (Id, EntityType, EntityId, ActivityType, OccurredAtUtc, Summary) VALUES ($id, 'Skill', $entityId, $type, $occurred, $summary);";
        command.Parameters.AddWithValue("$id", Guid.NewGuid().ToString("D"));
        command.Parameters.AddWithValue("$entityId", id.ToString("D"));
        command.Parameters.AddWithValue("$type", type);
        command.Parameters.AddWithValue("$occurred", SqliteValue.ToDb(DateTimeOffset.UtcNow));
        command.Parameters.AddWithValue("$summary", summary);
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }
}
