using Microsoft.Data.Sqlite;
using SASD.LearningManager.Application.Common;
using SASD.LearningManager.Application.Goals;
using SASD.LearningManager.Domain.Goals;

namespace SASD.LearningManager.Infrastructure.Persistence.Repositories;

/// <summary>SQLite persistence for goals and their many-to-many skill links.</summary>
public sealed class GoalRepository : IGoalRepository
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public GoalRepository(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Goal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, Title, Description, GoalType, Motivation, Priority, Status, TargetDate,
                   NextActionText, NextActionDueDate, CreatedAtUtc, UpdatedAtUtc, AchievedAtUtc, ArchivedAtUtc
            FROM Goals WHERE Id = $id;
            """;
        command.Parameters.AddWithValue("$id", id.ToString("D"));
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        return await reader.ReadAsync(cancellationToken).ConfigureAwait(false) ? MapGoal(reader) : null;
    }

    public async Task<GoalDetailDto?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var goal = await GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (goal is null) return null;

        await using var connection = await _connectionFactory.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT SkillId FROM GoalSkills WHERE GoalId = $id ORDER BY SkillId;";
        command.Parameters.AddWithValue("$id", id.ToString("D"));
        var skillIds = new List<Guid>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false)) skillIds.Add(Guid.Parse(reader.GetString(0)));

        return new GoalDetailDto(goal.Id, goal.Title, goal.Description, goal.Type, goal.Motivation, goal.Priority,
            goal.Status, goal.TargetDate, goal.NextActionText, goal.NextActionDueDate, goal.CreatedAtUtc,
            goal.UpdatedAtUtc, goal.AchievedAtUtc, goal.ArchivedAtUtc, skillIds);
    }

    public async Task<PagedResult<GoalListItemDto>> SearchAsync(GoalSearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.OpenAsync(cancellationToken).ConfigureAwait(false);
        var filters = new List<string>();
        if (!criteria.IncludeArchived) filters.Add("g.Status <> 'Archived'");
        if (criteria.Status is not null) filters.Add("g.Status = $status");
        if (!string.IsNullOrWhiteSpace(criteria.SearchText))
        {
            filters.Add("(g.Title LIKE $search ESCAPE '\\' COLLATE NOCASE OR g.Description LIKE $search ESCAPE '\\' COLLATE NOCASE OR g.Motivation LIKE $search ESCAPE '\\' COLLATE NOCASE OR g.NextActionText LIKE $search ESCAPE '\\' COLLATE NOCASE)");
        }

        var where = filters.Count == 0 ? string.Empty : "WHERE " + string.Join(" AND ", filters);
        await using var count = connection.CreateCommand();
        count.CommandText = $"SELECT COUNT(1) FROM Goals g {where};";
        AddSearchParameters(count, criteria);
        var total = Convert.ToInt32(await count.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false), System.Globalization.CultureInfo.InvariantCulture);

        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            SELECT g.Id, g.Title, g.GoalType, g.Status, g.Priority, g.TargetDate,
                   (SELECT COUNT(1) FROM GoalSkills gs WHERE gs.GoalId = g.Id) AS SkillCount,
                   g.NextActionText, g.NextActionDueDate, g.UpdatedAtUtc
            FROM Goals g
            {where}
            ORDER BY
                CASE WHEN g.Status = 'Active' THEN 0 WHEN g.Status = 'Planned' THEN 1 WHEN g.Status = 'Paused' THEN 2 ELSE 3 END,
                CASE g.Priority WHEN 'VeryHigh' THEN 0 WHEN 'High' THEN 1 WHEN 'Normal' THEN 2 ELSE 3 END,
                COALESCE(g.TargetDate, '9999-12-31'),
                g.Title COLLATE NOCASE
            LIMIT $limit OFFSET $offset;
            """;
        AddSearchParameters(command, criteria);
        command.Parameters.AddWithValue("$limit", criteria.PageSize);
        command.Parameters.AddWithValue("$offset", (criteria.PageNumber - 1) * criteria.PageSize);

        var items = new List<GoalListItemDto>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            items.Add(new GoalListItemDto(Guid.Parse(reader.GetString(0)), reader.GetString(1),
                Enum.Parse<GoalType>(reader.GetString(2)), Enum.Parse<GoalStatus>(reader.GetString(3)),
                Enum.Parse<GoalPriority>(reader.GetString(4)), SqliteValue.NullableDateOnly(reader, 5), reader.GetInt32(6),
                SqliteValue.NullableString(reader, 7), SqliteValue.NullableDateOnly(reader, 8), SqliteValue.DateTimeOffset(reader, 9)));
        }

        return new PagedResult<GoalListItemDto>(items, criteria.PageNumber, criteria.PageSize, total);
    }

    public async Task<IReadOnlyList<GoalLookupDto>> ListLookupAsync(bool includeArchived, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = includeArchived
            ? "SELECT Id, Title, Status FROM Goals ORDER BY Title COLLATE NOCASE;"
            : "SELECT Id, Title, Status FROM Goals WHERE Status <> 'Archived' ORDER BY Title COLLATE NOCASE;";
        var items = new List<GoalLookupDto>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            items.Add(new GoalLookupDto(Guid.Parse(reader.GetString(0)), reader.GetString(1), Enum.Parse<GoalStatus>(reader.GetString(2))));
        }
        return items;
    }

    public Task InsertAsync(Goal goal, IReadOnlyCollection<Guid> skillIds, CancellationToken cancellationToken = default)
        => SaveAsync(goal, skillIds, insert: true, cancellationToken);

    public Task UpdateAsync(Goal goal, IReadOnlyCollection<Guid> skillIds, CancellationToken cancellationToken = default)
        => SaveAsync(goal, skillIds, insert: false, cancellationToken);

    private async Task SaveAsync(Goal goal, IReadOnlyCollection<Guid> skillIds, bool insert, CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.OpenAsync(cancellationToken).ConfigureAwait(false);
        using var transaction = connection.BeginTransaction();
        try
        {
            await using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = insert
                    ? """
                      INSERT INTO Goals (Id, Title, Description, GoalType, Motivation, Priority, Status, TargetDate,
                                         NextActionText, NextActionDueDate, CreatedAtUtc, UpdatedAtUtc, AchievedAtUtc, ArchivedAtUtc)
                      VALUES ($id, $title, $description, $type, $motivation, $priority, $status, $targetDate,
                              $nextAction, $nextActionDue, $created, $updated, $achieved, $archived);
                      """
                    : """
                      UPDATE Goals SET Title = $title, Description = $description, GoalType = $type,
                          Motivation = $motivation, Priority = $priority, Status = $status, TargetDate = $targetDate,
                          NextActionText = $nextAction, NextActionDueDate = $nextActionDue, UpdatedAtUtc = $updated,
                          AchievedAtUtc = $achieved, ArchivedAtUtc = $archived
                      WHERE Id = $id;
                      """;
                AddGoalParameters(command, goal);
                var affected = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                if (affected != 1)
                {
                    throw new InvalidOperationException($"Goal '{goal.Id}' could not be {(insert ? "created" : "updated")}.");
                }
            }

            await ReplaceSkillsAsync(connection, transaction, goal.Id, skillIds, cancellationToken).ConfigureAwait(false);
            var action = insert ? "created" : $"updated ({goal.Status})";
            await WriteActivityAsync(connection, transaction, goal.Id, insert ? "GoalCreated" : "GoalUpdated",
                $"Goal '{goal.Title}' {action}.", cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None).ConfigureAwait(false);
            throw;
        }
    }

    private static async Task ReplaceSkillsAsync(SqliteConnection connection, SqliteTransaction transaction, Guid goalId,
        IEnumerable<Guid> skillIds, CancellationToken cancellationToken)
    {
        await using (var delete = connection.CreateCommand())
        {
            delete.Transaction = transaction;
            delete.CommandText = "DELETE FROM GoalSkills WHERE GoalId = $goalId;";
            delete.Parameters.AddWithValue("$goalId", goalId.ToString("D"));
            await delete.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        foreach (var skillId in skillIds.Distinct())
        {
            await using var insert = connection.CreateCommand();
            insert.Transaction = transaction;
            insert.CommandText = "INSERT INTO GoalSkills (GoalId, SkillId) VALUES ($goalId, $skillId);";
            insert.Parameters.AddWithValue("$goalId", goalId.ToString("D"));
            insert.Parameters.AddWithValue("$skillId", skillId.ToString("D"));
            await insert.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private static Goal MapGoal(SqliteDataReader reader)
        => Goal.Rehydrate(Guid.Parse(reader.GetString(0)), reader.GetString(1), SqliteValue.NullableString(reader, 2),
            Enum.Parse<GoalType>(reader.GetString(3)), SqliteValue.NullableString(reader, 4),
            Enum.Parse<GoalPriority>(reader.GetString(5)), Enum.Parse<GoalStatus>(reader.GetString(6)),
            SqliteValue.NullableDateOnly(reader, 7), SqliteValue.NullableString(reader, 8), SqliteValue.NullableDateOnly(reader, 9),
            SqliteValue.DateTimeOffset(reader, 10), SqliteValue.DateTimeOffset(reader, 11),
            SqliteValue.NullableDateTimeOffset(reader, 12), SqliteValue.NullableDateTimeOffset(reader, 13));

    private static void AddGoalParameters(SqliteCommand command, Goal goal)
    {
        command.Parameters.AddWithValue("$id", goal.Id.ToString("D"));
        command.Parameters.AddWithValue("$title", goal.Title);
        command.Parameters.AddWithValue("$description", SqliteValue.ToDb(goal.Description));
        command.Parameters.AddWithValue("$type", goal.Type.ToString());
        command.Parameters.AddWithValue("$motivation", SqliteValue.ToDb(goal.Motivation));
        command.Parameters.AddWithValue("$priority", goal.Priority.ToString());
        command.Parameters.AddWithValue("$status", goal.Status.ToString());
        command.Parameters.AddWithValue("$targetDate", SqliteValue.ToDb(goal.TargetDate));
        command.Parameters.AddWithValue("$nextAction", SqliteValue.ToDb(goal.NextActionText));
        command.Parameters.AddWithValue("$nextActionDue", SqliteValue.ToDb(goal.NextActionDueDate));
        command.Parameters.AddWithValue("$created", SqliteValue.ToDb(goal.CreatedAtUtc));
        command.Parameters.AddWithValue("$updated", SqliteValue.ToDb(goal.UpdatedAtUtc));
        command.Parameters.AddWithValue("$achieved", SqliteValue.ToDb(goal.AchievedAtUtc));
        command.Parameters.AddWithValue("$archived", SqliteValue.ToDb(goal.ArchivedAtUtc));
    }

    private static void AddSearchParameters(SqliteCommand command, GoalSearchCriteria criteria)
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
        command.CommandText = "INSERT INTO ActivityLog (Id, EntityType, EntityId, ActivityType, OccurredAtUtc, Summary) VALUES ($id, 'Goal', $entityId, $type, $occurred, $summary);";
        command.Parameters.AddWithValue("$id", Guid.NewGuid().ToString("D"));
        command.Parameters.AddWithValue("$entityId", id.ToString("D"));
        command.Parameters.AddWithValue("$type", type);
        command.Parameters.AddWithValue("$occurred", SqliteValue.ToDb(DateTimeOffset.UtcNow));
        command.Parameters.AddWithValue("$summary", summary);
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }
}
