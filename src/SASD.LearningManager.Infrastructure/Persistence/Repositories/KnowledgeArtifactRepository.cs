using Microsoft.Data.Sqlite;
using SASD.LearningManager.Application.Common;
using SASD.LearningManager.Application.Knowledge;
using SASD.LearningManager.Domain.Knowledge;

namespace SASD.LearningManager.Infrastructure.Persistence.Repositories;

/// <summary>SQLite persistence for Markdown knowledge and its explicit source/context links.</summary>
public sealed class KnowledgeArtifactRepository : IKnowledgeArtifactRepository
{
    private readonly SqliteConnectionFactory _connections;
    public KnowledgeArtifactRepository(SqliteConnectionFactory connections) => _connections = connections;

    public async Task<KnowledgeArtifact?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await using var c = await _connections.OpenAsync(ct); await using var q = c.CreateCommand();
        q.CommandText = "SELECT Id,Title,Markdown,Type,Status,CreatedAtUtc,UpdatedAtUtc,ArchivedAtUtc FROM KnowledgeArtifacts WHERE Id=$id;";
        q.Parameters.AddWithValue("$id", id.ToString("D")); await using var r = await q.ExecuteReaderAsync(ct);
        return await r.ReadAsync(ct) ? Map(r) : null;
    }

    public async Task<KnowledgeArtifactDetailDto?> GetDetailAsync(Guid id, CancellationToken ct = default)
    {
        var item = await GetByIdAsync(id, ct); if (item is null) return null;
        await using var c = await _connections.OpenAsync(ct);
        return new(item.Id, item.Title, item.Markdown, item.Type, item.Status, item.CreatedAtUtc, item.UpdatedAtUtc,
            await ReadLinks(c,"KnowledgeArtifactResources","ResourceId",id,ct), await ReadLinks(c,"KnowledgeArtifactSkills","SkillId",id,ct),
            await ReadLinks(c,"KnowledgeArtifactTopics","TopicId",id,ct), await ReadLinks(c,"KnowledgeArtifactGoals","GoalId",id,ct),
            await ReadLinks(c,"KnowledgeArtifactLearningPaths","LearningPathId",id,ct));
    }

    public async Task<PagedResult<KnowledgeArtifactListItemDto>> SearchAsync(KnowledgeSearchCriteria x, CancellationToken ct = default)
    {
        await using var c = await _connections.OpenAsync(ct);
        var where = x.IncludeArchived ? "1=1" : "Status <> 'Archived'";
        if (!string.IsNullOrWhiteSpace(x.SearchText)) where += " AND (Title LIKE $search ESCAPE '\\' COLLATE NOCASE OR Markdown LIKE $search ESCAPE '\\' COLLATE NOCASE)";
        await using var count = c.CreateCommand(); count.CommandText = $"SELECT COUNT(1) FROM KnowledgeArtifacts WHERE {where};"; AddSearch(count,x.SearchText);
        var total = Convert.ToInt32(await count.ExecuteScalarAsync(ct), System.Globalization.CultureInfo.InvariantCulture);
        await using var q = c.CreateCommand(); q.CommandText = $"SELECT Id,Title,Type,Status,UpdatedAtUtc FROM KnowledgeArtifacts WHERE {where} ORDER BY UpdatedAtUtc DESC LIMIT $limit OFFSET $offset;";
        AddSearch(q,x.SearchText); q.Parameters.AddWithValue("$limit",x.PageSize); q.Parameters.AddWithValue("$offset",(x.PageNumber-1)*x.PageSize);
        var items = new List<KnowledgeArtifactListItemDto>(); await using var r = await q.ExecuteReaderAsync(ct);
        while(await r.ReadAsync(ct)) items.Add(new(Guid.Parse(r.GetString(0)),r.GetString(1),Enum.Parse<KnowledgeArtifactType>(r.GetString(2)),Enum.Parse<KnowledgeArtifactStatus>(r.GetString(3)),SqliteValue.DateTimeOffset(r,4)));
        return new(items,x.PageNumber,x.PageSize,total);
    }

    public async Task SaveAsync(KnowledgeArtifact item, KnowledgeArtifactEditModel model, bool insert, CancellationToken ct = default)
    {
        await using var c = await _connections.OpenAsync(ct); using var tx = c.BeginTransaction();
        try {
            await using(var q=c.CreateCommand()) { q.Transaction=tx; q.CommandText=insert
                ? "INSERT INTO KnowledgeArtifacts(Id,Title,Markdown,Type,Status,CreatedAtUtc,UpdatedAtUtc,ArchivedAtUtc) VALUES($id,$title,$markdown,$type,$status,$created,$updated,$archived);"
                : "UPDATE KnowledgeArtifacts SET Title=$title,Markdown=$markdown,Type=$type,Status=$status,UpdatedAtUtc=$updated,ArchivedAtUtc=$archived WHERE Id=$id;";
                q.Parameters.AddWithValue("$id",item.Id.ToString("D")); q.Parameters.AddWithValue("$title",item.Title); q.Parameters.AddWithValue("$markdown",item.Markdown);
                q.Parameters.AddWithValue("$type",item.Type.ToString()); q.Parameters.AddWithValue("$status",item.Status.ToString()); q.Parameters.AddWithValue("$created",SqliteValue.ToDb(item.CreatedAtUtc));
                q.Parameters.AddWithValue("$updated",SqliteValue.ToDb(item.UpdatedAtUtc)); q.Parameters.AddWithValue("$archived",SqliteValue.ToDb(item.ArchivedAtUtc)); if(await q.ExecuteNonQueryAsync(ct)!=1) throw new InvalidOperationException("Knowledge artifact could not be saved."); }
            await Replace(c,tx,"KnowledgeArtifactResources","ResourceId",item.Id,model.ResourceIds,ct); await Replace(c,tx,"KnowledgeArtifactSkills","SkillId",item.Id,model.SkillIds,ct);
            await Replace(c,tx,"KnowledgeArtifactTopics","TopicId",item.Id,model.TopicIds,ct); await Replace(c,tx,"KnowledgeArtifactGoals","GoalId",item.Id,model.GoalIds,ct);
            await Replace(c,tx,"KnowledgeArtifactLearningPaths","LearningPathId",item.Id,model.LearningPathIds,ct); await tx.CommitAsync(ct);
        } catch { await tx.RollbackAsync(CancellationToken.None); throw; }
    }

    private static KnowledgeArtifact Map(SqliteDataReader r) => KnowledgeArtifact.Rehydrate(Guid.Parse(r.GetString(0)),r.GetString(1),r.GetString(2),Enum.Parse<KnowledgeArtifactType>(r.GetString(3)),Enum.Parse<KnowledgeArtifactStatus>(r.GetString(4)),SqliteValue.DateTimeOffset(r,5),SqliteValue.DateTimeOffset(r,6),SqliteValue.NullableDateTimeOffset(r,7));
    private static void AddSearch(SqliteCommand q,string? value) { if(!string.IsNullOrWhiteSpace(value)) q.Parameters.AddWithValue("$search",$"%{value.Trim().Replace("\\","\\\\",StringComparison.Ordinal).Replace("%","\\%",StringComparison.Ordinal).Replace("_","\\_",StringComparison.Ordinal)}%"); }
    private static async Task<IReadOnlyList<Guid>> ReadLinks(SqliteConnection c,string table,string column,Guid id,CancellationToken ct) { await using var q=c.CreateCommand(); q.CommandText=$"SELECT {column} FROM {table} WHERE KnowledgeArtifactId=$id ORDER BY {column};"; q.Parameters.AddWithValue("$id",id.ToString("D")); var list=new List<Guid>(); await using var r=await q.ExecuteReaderAsync(ct); while(await r.ReadAsync(ct)) list.Add(Guid.Parse(r.GetString(0))); return list; }
    private static async Task Replace(SqliteConnection c,SqliteTransaction tx,string table,string column,Guid id,IEnumerable<Guid> ids,CancellationToken ct) { await using(var d=c.CreateCommand()){d.Transaction=tx;d.CommandText=$"DELETE FROM {table} WHERE KnowledgeArtifactId=$id;";d.Parameters.AddWithValue("$id",id.ToString("D"));await d.ExecuteNonQueryAsync(ct);} foreach(var value in ids.Distinct()){await using var q=c.CreateCommand();q.Transaction=tx;q.CommandText=$"INSERT INTO {table}(KnowledgeArtifactId,{column}) VALUES($id,$value);";q.Parameters.AddWithValue("$id",id.ToString("D"));q.Parameters.AddWithValue("$value",value.ToString("D"));await q.ExecuteNonQueryAsync(ct);} }
}
