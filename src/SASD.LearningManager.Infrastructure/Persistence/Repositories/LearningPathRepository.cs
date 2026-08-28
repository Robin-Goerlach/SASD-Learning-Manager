using Microsoft.Data.Sqlite;
using SASD.LearningManager.Application.Common;
using SASD.LearningManager.Application.LearningPaths;
using SASD.LearningManager.Domain.LearningPaths;

namespace SASD.LearningManager.Infrastructure.Persistence.Repositories;

/// <summary>
/// SQLite persistence for learning paths, their hierarchical nodes, assignments and graph-like
/// node relations. Multi-record writes are transactional so tree metadata and assignments cannot
/// drift apart after a partial failure.
/// </summary>
public sealed class LearningPathRepository : ILearningPathRepository
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public LearningPathRepository(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<LearningPath?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, Title, Description, Status, Priority, PlannedStartDate, TargetDate,
                   NextActionText, NextActionDueDate, CreatedAtUtc, UpdatedAtUtc, StartedAtUtc,
                   CompletedAtUtc, ArchivedAtUtc
            FROM LearningPaths WHERE Id = $id;
            """;
        command.Parameters.AddWithValue("$id", id.ToString("D"));
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        return await reader.ReadAsync(cancellationToken).ConfigureAwait(false) ? MapPath(reader) : null;
    }

    public async Task<LearningPathDetailDto?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var path = await GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (path is null)
        {
            return null;
        }

        await using var connection = await _connectionFactory.OpenAsync(cancellationToken).ConfigureAwait(false);
        var goalIds = await ReadIdsAsync(connection,
            "SELECT GoalId FROM GoalLearningPaths WHERE LearningPathId = $id ORDER BY GoalId;", id, cancellationToken).ConfigureAwait(false);
        var nodes = await ListNodesUsingConnectionAsync(connection, id, includeArchived: false, cancellationToken).ConfigureAwait(false);
        var progress = LearningPathProgress.Calculate(nodes);
        return new LearningPathDetailDto(path.Id, path.Title, path.Description, path.Status, path.Priority,
            path.PlannedStartDate, path.TargetDate, path.NextActionText, path.NextActionDueDate, path.CreatedAtUtc,
            path.UpdatedAtUtc, path.StartedAtUtc, path.CompletedAtUtc, path.ArchivedAtUtc, goalIds,
            progress.RequiredCompleted, progress.RequiredTotal, progress.OptionalCompleted, progress.OptionalTotal,
            progress.CoreCompletionPercent);
    }

    public async Task<PagedResult<LearningPathListItemDto>> SearchAsync(LearningPathSearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.OpenAsync(cancellationToken).ConfigureAwait(false);
        var filters = new List<string>();
        if (!criteria.IncludeArchived) filters.Add("lp.Status <> 'Archived'");
        if (criteria.Status is not null) filters.Add("lp.Status = $status");
        if (!string.IsNullOrWhiteSpace(criteria.SearchText))
        {
            filters.Add("(lp.Title LIKE $search ESCAPE '\\' COLLATE NOCASE OR lp.Description LIKE $search ESCAPE '\\' COLLATE NOCASE OR lp.NextActionText LIKE $search ESCAPE '\\' COLLATE NOCASE)");
        }
        var where = filters.Count == 0 ? string.Empty : "WHERE " + string.Join(" AND ", filters);

        await using var count = connection.CreateCommand();
        count.CommandText = $"SELECT COUNT(1) FROM LearningPaths lp {where};";
        AddSearchParameters(count, criteria);
        var total = Convert.ToInt32(await count.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false), System.Globalization.CultureInfo.InvariantCulture);

        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            SELECT lp.Id, lp.Title, lp.Status, lp.Priority, lp.TargetDate,
                   (SELECT COUNT(1) FROM LearningPathNodes n WHERE n.LearningPathId = lp.Id AND n.Status <> 'Archived') AS NodeCount,
                   (SELECT COUNT(1) FROM LearningPathNodes n WHERE n.LearningPathId = lp.Id AND n.Status = 'Completed' AND n.IsRequired = 1) AS RequiredCompleted,
                   (SELECT COUNT(1) FROM LearningPathNodes n WHERE n.LearningPathId = lp.Id AND n.Status <> 'Archived' AND n.IsRequired = 1) AS RequiredTotal,
                   CASE
                       WHEN (SELECT COUNT(1) FROM LearningPathNodes n WHERE n.LearningPathId = lp.Id AND n.Status <> 'Archived' AND n.IsRequired = 1) > 0
                       THEN ROUND(100.0 *
                           (SELECT COUNT(1) FROM LearningPathNodes n WHERE n.LearningPathId = lp.Id AND n.Status = 'Completed' AND n.IsRequired = 1) /
                           (SELECT COUNT(1) FROM LearningPathNodes n WHERE n.LearningPathId = lp.Id AND n.Status <> 'Archived' AND n.IsRequired = 1), 1)
                       WHEN (SELECT COUNT(1) FROM LearningPathNodes n WHERE n.LearningPathId = lp.Id AND n.Status <> 'Archived') > 0
                       THEN ROUND(100.0 *
                           (SELECT COUNT(1) FROM LearningPathNodes n WHERE n.LearningPathId = lp.Id AND n.Status = 'Completed') /
                           (SELECT COUNT(1) FROM LearningPathNodes n WHERE n.LearningPathId = lp.Id AND n.Status <> 'Archived'), 1)
                       ELSE NULL
                   END AS CoreCompletion,
                   lp.NextActionText, lp.UpdatedAtUtc
            FROM LearningPaths lp
            {where}
            ORDER BY
                CASE lp.Status WHEN 'Active' THEN 0 WHEN 'Planned' THEN 1 WHEN 'Paused' THEN 2 WHEN 'Completed' THEN 3 ELSE 4 END,
                CASE lp.Priority WHEN 'VeryHigh' THEN 0 WHEN 'High' THEN 1 WHEN 'Normal' THEN 2 ELSE 3 END,
                COALESCE(lp.TargetDate, '9999-12-31'),
                lp.Title COLLATE NOCASE
            LIMIT $limit OFFSET $offset;
            """;
        AddSearchParameters(command, criteria);
        command.Parameters.AddWithValue("$limit", criteria.PageSize);
        command.Parameters.AddWithValue("$offset", (criteria.PageNumber - 1) * criteria.PageSize);

        var items = new List<LearningPathListItemDto>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            items.Add(new LearningPathListItemDto(
                Guid.Parse(reader.GetString(0)), reader.GetString(1), Enum.Parse<LearningPathStatus>(reader.GetString(2)),
                Enum.Parse<LearningPathPriority>(reader.GetString(3)), SqliteValue.NullableDateOnly(reader, 4), reader.GetInt32(5),
                reader.GetInt32(6), reader.GetInt32(7), reader.IsDBNull(8) ? null : Convert.ToDecimal(reader.GetDouble(8)),
                SqliteValue.NullableString(reader, 9), SqliteValue.DateTimeOffset(reader, 10)));
        }

        return new PagedResult<LearningPathListItemDto>(items, criteria.PageNumber, criteria.PageSize, total);
    }

    public Task InsertAsync(LearningPath path, IReadOnlyCollection<Guid> goalIds, CancellationToken cancellationToken = default)
        => SavePathAsync(path, goalIds, insert: true, cancellationToken);

    public Task UpdateAsync(LearningPath path, IReadOnlyCollection<Guid> goalIds, CancellationToken cancellationToken = default)
        => SavePathAsync(path, goalIds, insert: false, cancellationToken);

    public async Task<IReadOnlyList<LearningPathNode>> ListNodesAsync(Guid learningPathId, bool includeArchived, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.OpenAsync(cancellationToken).ConfigureAwait(false);
        return await ListNodesUsingConnectionAsync(connection, learningPathId, includeArchived, cancellationToken).ConfigureAwait(false);
    }

    public async Task<LearningPathNode?> GetNodeByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, LearningPathId, ParentNodeId, Title, Description, NodeType, SortOrder,
                   IsRequired, Status, CreatedAtUtc, UpdatedAtUtc, ArchivedAtUtc
            FROM LearningPathNodes WHERE Id = $id;
            """;
        command.Parameters.AddWithValue("$id", id.ToString("D"));
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        return await reader.ReadAsync(cancellationToken).ConfigureAwait(false) ? MapNode(reader) : null;
    }

    public async Task<LearningPathNodeDetailDto?> GetNodeDetailAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var node = await GetNodeByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (node is null)
        {
            return null;
        }

        await using var connection = await _connectionFactory.OpenAsync(cancellationToken).ConfigureAwait(false);
        var skillIds = await ReadIdsAsync(connection,
            "SELECT SkillId FROM LearningPathNodeSkills WHERE LearningPathNodeId = $id ORDER BY SkillId;", id, cancellationToken).ConfigureAwait(false);
        var resourceIds = await ReadIdsAsync(connection,
            "SELECT ResourceId FROM LearningPathNodeResources WHERE LearningPathNodeId = $id ORDER BY ResourceId;", id, cancellationToken).ConfigureAwait(false);
        return new LearningPathNodeDetailDto(node.Id, node.LearningPathId, node.ParentNodeId, node.Title, node.Description,
            node.Type, node.SortOrder, node.IsRequired, node.Status, node.CreatedAtUtc, node.UpdatedAtUtc,
            node.ArchivedAtUtc, skillIds, resourceIds);
    }

    public Task InsertNodeAsync(LearningPathNode node, IReadOnlyCollection<Guid> skillIds, IReadOnlyCollection<Guid> resourceIds, CancellationToken cancellationToken = default)
        => SaveNodeAsync(node, skillIds, resourceIds, insert: true, cancellationToken);

    public Task UpdateNodeAsync(LearningPathNode node, IReadOnlyCollection<Guid> skillIds, IReadOnlyCollection<Guid> resourceIds, CancellationToken cancellationToken = default)
        => SaveNodeAsync(node, skillIds, resourceIds, insert: false, cancellationToken);

    public async Task UpdateNodeOrdersAsync(IReadOnlyCollection<LearningPathNodeOrderUpdate> updates, CancellationToken cancellationToken = default)
    {
        if (updates.Count == 0)
        {
            return;
        }

        await using var connection = await _connectionFactory.OpenAsync(cancellationToken).ConfigureAwait(false);
        using var transaction = connection.BeginTransaction();
        try
        {
            foreach (var update in updates)
            {
                await using var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = "UPDATE LearningPathNodes SET ParentNodeId = $parent, SortOrder = $sort, UpdatedAtUtc = $updated WHERE Id = $id;";
                command.Parameters.AddWithValue("$parent", SqliteValue.ToDb(update.ParentNodeId));
                command.Parameters.AddWithValue("$sort", update.SortOrder);
                command.Parameters.AddWithValue("$updated", SqliteValue.ToDb(update.UpdatedAtUtc));
                command.Parameters.AddWithValue("$id", update.NodeId.ToString("D"));
                if (await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false) != 1)
                {
                    throw new InvalidOperationException($"Learning path node '{update.NodeId}' could not be reordered.");
                }
            }
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None).ConfigureAwait(false);
            throw;
        }
    }

    public async Task ArchiveNodesAsync(IReadOnlyCollection<Guid> nodeIds, DateTimeOffset nowUtc, CancellationToken cancellationToken = default)
    {
        if (nodeIds.Count == 0)
        {
            return;
        }

        await using var connection = await _connectionFactory.OpenAsync(cancellationToken).ConfigureAwait(false);
        using var transaction = connection.BeginTransaction();
        try
        {
            foreach (var id in nodeIds.Distinct())
            {
                await using var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = "UPDATE LearningPathNodes SET Status = 'Archived', ArchivedAtUtc = $archived, UpdatedAtUtc = $updated WHERE Id = $id;";
                command.Parameters.AddWithValue("$archived", SqliteValue.ToDb(nowUtc));
                command.Parameters.AddWithValue("$updated", SqliteValue.ToDb(nowUtc));
                command.Parameters.AddWithValue("$id", id.ToString("D"));
                if (await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false) != 1)
                {
                    throw new InvalidOperationException($"Learning path node '{id}' could not be archived.");
                }
            }
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None).ConfigureAwait(false);
            throw;
        }
    }

    public async Task<IReadOnlyList<LearningPathNodeRelationDto>> ListRelationsAsync(Guid learningPathId, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT r.Id, r.SourceNodeId, s.Title, r.TargetNodeId, t.Title, r.RelationType, r.Note, r.CreatedAtUtc
            FROM LearningPathNodeRelations r
            INNER JOIN LearningPathNodes s ON s.Id = r.SourceNodeId
            INNER JOIN LearningPathNodes t ON t.Id = r.TargetNodeId
            WHERE s.LearningPathId = $pathId AND t.LearningPathId = $pathId
            ORDER BY s.Title COLLATE NOCASE, r.RelationType, t.Title COLLATE NOCASE;
            """;
        command.Parameters.AddWithValue("$pathId", learningPathId.ToString("D"));
        var items = new List<LearningPathNodeRelationDto>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            items.Add(new LearningPathNodeRelationDto(Guid.Parse(reader.GetString(0)), Guid.Parse(reader.GetString(1)), reader.GetString(2),
                Guid.Parse(reader.GetString(3)), reader.GetString(4), Enum.Parse<LearningPathNodeRelationType>(reader.GetString(5)),
                SqliteValue.NullableString(reader, 6), SqliteValue.DateTimeOffset(reader, 7)));
        }
        return items;
    }

    public async Task<bool> RelationExistsAsync(Guid sourceNodeId, Guid targetNodeId, LearningPathNodeRelationType type, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(1) FROM LearningPathNodeRelations WHERE SourceNodeId = $source AND TargetNodeId = $target AND RelationType = $type;";
        command.Parameters.AddWithValue("$source", sourceNodeId.ToString("D"));
        command.Parameters.AddWithValue("$target", targetNodeId.ToString("D"));
        command.Parameters.AddWithValue("$type", type.ToString());
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false), System.Globalization.CultureInfo.InvariantCulture) > 0;
    }

    public async Task InsertRelationAsync(LearningPathNodeRelation relation, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO LearningPathNodeRelations (Id, SourceNodeId, TargetNodeId, RelationType, Note, CreatedAtUtc)
            VALUES ($id, $source, $target, $type, $note, $created);
            """;
        command.Parameters.AddWithValue("$id", relation.Id.ToString("D"));
        command.Parameters.AddWithValue("$source", relation.SourceNodeId.ToString("D"));
        command.Parameters.AddWithValue("$target", relation.TargetNodeId.ToString("D"));
        command.Parameters.AddWithValue("$type", relation.Type.ToString());
        command.Parameters.AddWithValue("$note", SqliteValue.ToDb(relation.Note));
        command.Parameters.AddWithValue("$created", SqliteValue.ToDb(relation.CreatedAtUtc));
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteRelationAsync(Guid relationId, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM LearningPathNodeRelations WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", relationId.ToString("D"));
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task SavePathAsync(LearningPath path, IReadOnlyCollection<Guid> goalIds, bool insert, CancellationToken cancellationToken)
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
                      INSERT INTO LearningPaths (Id, Title, Description, Status, Priority, PlannedStartDate, TargetDate,
                          NextActionText, NextActionDueDate, CreatedAtUtc, UpdatedAtUtc, StartedAtUtc, CompletedAtUtc, ArchivedAtUtc)
                      VALUES ($id, $title, $description, $status, $priority, $plannedStart, $targetDate,
                          $nextAction, $nextActionDue, $created, $updated, $started, $completed, $archived);
                      """
                    : """
                      UPDATE LearningPaths SET Title = $title, Description = $description, Status = $status,
                          Priority = $priority, PlannedStartDate = $plannedStart, TargetDate = $targetDate,
                          NextActionText = $nextAction, NextActionDueDate = $nextActionDue, UpdatedAtUtc = $updated,
                          StartedAtUtc = $started, CompletedAtUtc = $completed, ArchivedAtUtc = $archived
                      WHERE Id = $id;
                      """;
                AddPathParameters(command, path);
                if (await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false) != 1)
                {
                    throw new InvalidOperationException($"Learning path '{path.Id}' could not be {(insert ? "created" : "updated")}.");
                }
            }

            await ReplaceLinksAsync(connection, transaction, "GoalLearningPaths", "GoalId", "LearningPathId", path.Id, goalIds, cancellationToken).ConfigureAwait(false);
            await WriteActivityAsync(connection, transaction, "LearningPath", path.Id, insert ? "LearningPathCreated" : "LearningPathUpdated",
                $"Learning path '{path.Title}' {(insert ? "created" : $"updated ({path.Status})")}.", cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None).ConfigureAwait(false);
            throw;
        }
    }

    private async Task SaveNodeAsync(LearningPathNode node, IReadOnlyCollection<Guid> skillIds, IReadOnlyCollection<Guid> resourceIds,
        bool insert, CancellationToken cancellationToken)
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
                      INSERT INTO LearningPathNodes (Id, LearningPathId, ParentNodeId, Title, Description, NodeType,
                          SortOrder, IsRequired, Status, CreatedAtUtc, UpdatedAtUtc, ArchivedAtUtc)
                      VALUES ($id, $pathId, $parentId, $title, $description, $type, $sort, $required, $status,
                          $created, $updated, $archived);
                      """
                    : """
                      UPDATE LearningPathNodes SET ParentNodeId = $parentId, Title = $title, Description = $description,
                          NodeType = $type, SortOrder = $sort, IsRequired = $required, Status = $status,
                          UpdatedAtUtc = $updated, ArchivedAtUtc = $archived
                      WHERE Id = $id;
                      """;
                AddNodeParameters(command, node);
                if (await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false) != 1)
                {
                    throw new InvalidOperationException($"Learning path node '{node.Id}' could not be {(insert ? "created" : "updated")}.");
                }
            }

            await ReplaceLinksAsync(connection, transaction, "LearningPathNodeSkills", "SkillId", "LearningPathNodeId", node.Id, skillIds, cancellationToken).ConfigureAwait(false);
            await ReplaceLinksAsync(connection, transaction, "LearningPathNodeResources", "ResourceId", "LearningPathNodeId", node.Id, resourceIds, cancellationToken).ConfigureAwait(false);
            await WriteActivityAsync(connection, transaction, "LearningPathNode", node.Id, insert ? "LearningPathNodeCreated" : "LearningPathNodeUpdated",
                $"Learning path node '{node.Title}' {(insert ? "created" : $"updated ({node.Status})")}.", cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None).ConfigureAwait(false);
            throw;
        }
    }

    private static async Task ReplaceLinksAsync(SqliteConnection connection, SqliteTransaction transaction,
        string table, string foreignColumn, string ownerColumn, Guid ownerId, IEnumerable<Guid> foreignIds, CancellationToken cancellationToken)
    {
        var valid =
            (table == "GoalLearningPaths" && foreignColumn == "GoalId" && ownerColumn == "LearningPathId") ||
            (table == "LearningPathNodeSkills" && foreignColumn == "SkillId" && ownerColumn == "LearningPathNodeId") ||
            (table == "LearningPathNodeResources" && foreignColumn == "ResourceId" && ownerColumn == "LearningPathNodeId");
        if (!valid)
        {
            throw new ArgumentOutOfRangeException(nameof(table));
        }

        await using (var delete = connection.CreateCommand())
        {
            delete.Transaction = transaction;
            delete.CommandText = $"DELETE FROM {table} WHERE {ownerColumn} = $ownerId;";
            delete.Parameters.AddWithValue("$ownerId", ownerId.ToString("D"));
            await delete.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        foreach (var foreignId in foreignIds.Distinct())
        {
            await using var insert = connection.CreateCommand();
            insert.Transaction = transaction;
            insert.CommandText = $"INSERT INTO {table} ({foreignColumn}, {ownerColumn}) VALUES ($foreignId, $ownerId);";
            insert.Parameters.AddWithValue("$foreignId", foreignId.ToString("D"));
            insert.Parameters.AddWithValue("$ownerId", ownerId.ToString("D"));
            await insert.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task<IReadOnlyList<LearningPathNode>> ListNodesUsingConnectionAsync(SqliteConnection connection, Guid learningPathId,
        bool includeArchived, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = includeArchived
            ? """
              SELECT Id, LearningPathId, ParentNodeId, Title, Description, NodeType, SortOrder, IsRequired,
                     Status, CreatedAtUtc, UpdatedAtUtc, ArchivedAtUtc
              FROM LearningPathNodes WHERE LearningPathId = $pathId
              ORDER BY ParentNodeId, SortOrder, Title COLLATE NOCASE;
              """
            : """
              SELECT Id, LearningPathId, ParentNodeId, Title, Description, NodeType, SortOrder, IsRequired,
                     Status, CreatedAtUtc, UpdatedAtUtc, ArchivedAtUtc
              FROM LearningPathNodes WHERE LearningPathId = $pathId AND Status <> 'Archived'
              ORDER BY ParentNodeId, SortOrder, Title COLLATE NOCASE;
              """;
        command.Parameters.AddWithValue("$pathId", learningPathId.ToString("D"));
        var nodes = new List<LearningPathNode>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            nodes.Add(MapNode(reader));
        }
        return nodes;
    }

    private static LearningPath MapPath(SqliteDataReader reader)
        => LearningPath.Rehydrate(Guid.Parse(reader.GetString(0)), reader.GetString(1), SqliteValue.NullableString(reader, 2),
            Enum.Parse<LearningPathStatus>(reader.GetString(3)), Enum.Parse<LearningPathPriority>(reader.GetString(4)),
            SqliteValue.NullableDateOnly(reader, 5), SqliteValue.NullableDateOnly(reader, 6), SqliteValue.NullableString(reader, 7),
            SqliteValue.NullableDateOnly(reader, 8), SqliteValue.DateTimeOffset(reader, 9), SqliteValue.DateTimeOffset(reader, 10),
            SqliteValue.NullableDateTimeOffset(reader, 11), SqliteValue.NullableDateTimeOffset(reader, 12), SqliteValue.NullableDateTimeOffset(reader, 13));

    private static LearningPathNode MapNode(SqliteDataReader reader)
        => LearningPathNode.Rehydrate(Guid.Parse(reader.GetString(0)), Guid.Parse(reader.GetString(1)), SqliteValue.NullableGuid(reader, 2),
            reader.GetString(3), SqliteValue.NullableString(reader, 4), Enum.Parse<LearningPathNodeType>(reader.GetString(5)),
            reader.GetInt32(6), reader.GetInt32(7) != 0, Enum.Parse<LearningPathNodeStatus>(reader.GetString(8)),
            SqliteValue.DateTimeOffset(reader, 9), SqliteValue.DateTimeOffset(reader, 10), SqliteValue.NullableDateTimeOffset(reader, 11));

    private static void AddPathParameters(SqliteCommand command, LearningPath path)
    {
        command.Parameters.AddWithValue("$id", path.Id.ToString("D"));
        command.Parameters.AddWithValue("$title", path.Title);
        command.Parameters.AddWithValue("$description", SqliteValue.ToDb(path.Description));
        command.Parameters.AddWithValue("$status", path.Status.ToString());
        command.Parameters.AddWithValue("$priority", path.Priority.ToString());
        command.Parameters.AddWithValue("$plannedStart", SqliteValue.ToDb(path.PlannedStartDate));
        command.Parameters.AddWithValue("$targetDate", SqliteValue.ToDb(path.TargetDate));
        command.Parameters.AddWithValue("$nextAction", SqliteValue.ToDb(path.NextActionText));
        command.Parameters.AddWithValue("$nextActionDue", SqliteValue.ToDb(path.NextActionDueDate));
        command.Parameters.AddWithValue("$created", SqliteValue.ToDb(path.CreatedAtUtc));
        command.Parameters.AddWithValue("$updated", SqliteValue.ToDb(path.UpdatedAtUtc));
        command.Parameters.AddWithValue("$started", SqliteValue.ToDb(path.StartedAtUtc));
        command.Parameters.AddWithValue("$completed", SqliteValue.ToDb(path.CompletedAtUtc));
        command.Parameters.AddWithValue("$archived", SqliteValue.ToDb(path.ArchivedAtUtc));
    }

    private static void AddNodeParameters(SqliteCommand command, LearningPathNode node)
    {
        command.Parameters.AddWithValue("$id", node.Id.ToString("D"));
        command.Parameters.AddWithValue("$pathId", node.LearningPathId.ToString("D"));
        command.Parameters.AddWithValue("$parentId", SqliteValue.ToDb(node.ParentNodeId));
        command.Parameters.AddWithValue("$title", node.Title);
        command.Parameters.AddWithValue("$description", SqliteValue.ToDb(node.Description));
        command.Parameters.AddWithValue("$type", node.Type.ToString());
        command.Parameters.AddWithValue("$sort", node.SortOrder);
        command.Parameters.AddWithValue("$required", node.IsRequired ? 1 : 0);
        command.Parameters.AddWithValue("$status", node.Status.ToString());
        command.Parameters.AddWithValue("$created", SqliteValue.ToDb(node.CreatedAtUtc));
        command.Parameters.AddWithValue("$updated", SqliteValue.ToDb(node.UpdatedAtUtc));
        command.Parameters.AddWithValue("$archived", SqliteValue.ToDb(node.ArchivedAtUtc));
    }

    private static async Task<IReadOnlyList<Guid>> ReadIdsAsync(SqliteConnection connection, string sql, Guid id, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$id", id.ToString("D"));
        var ids = new List<Guid>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            ids.Add(Guid.Parse(reader.GetString(0)));
        }
        return ids;
    }

    private static void AddSearchParameters(SqliteCommand command, LearningPathSearchCriteria criteria)
    {
        if (criteria.Status is not null) command.Parameters.AddWithValue("$status", criteria.Status.Value.ToString());
        if (!string.IsNullOrWhiteSpace(criteria.SearchText)) command.Parameters.AddWithValue("$search", $"%{EscapeLike(criteria.SearchText.Trim())}%");
    }

    private static string EscapeLike(string value)
        => value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("%", "\\%", StringComparison.Ordinal).Replace("_", "\\_", StringComparison.Ordinal);

    private static async Task WriteActivityAsync(SqliteConnection connection, SqliteTransaction transaction, string entityType,
        Guid entityId, string activityType, string summary, CancellationToken cancellationToken)
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
