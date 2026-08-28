using Microsoft.EntityFrameworkCore;
using SASD.Bewerbungsmanager.Domain.Entities;
using SASD.Bewerbungsmanager.Domain.Enums;
using SASD.Bewerbungsmanager.Infrastructure.Persistence;

namespace SASD.Bewerbungsmanager.Infrastructure.Tests;

public sealed class AssistantPersistenceTests
{
    [Fact]
    public async Task LatestMigration_AssistantSessionRoundTripsThroughSqlite()
    {
        var path = Path.Combine(Path.GetTempPath(), $"sasd-assistant-{Guid.NewGuid():N}.db");
        try
        {
            var options = new DbContextOptionsBuilder<ApplicationTrackerDbContext>()
                .UseSqlite($"Data Source={path};Pooling=False;Foreign Keys=True")
                .Options;
            var now = new DateTimeOffset(2026, 8, 27, 12, 0, 0, TimeSpan.Zero);
            var id = Guid.NewGuid();

            await using (var context = new ApplicationTrackerDbContext(options))
            {
                await context.Database.MigrateAsync();
                context.AssistantSessions.Add(new AssistantSession
                {
                    Id = id,
                    TaskKind = AssistantTaskKind.InterviewPreparation,
                    Status = AssistantSessionStatus.Prepared,
                    Title = "Synthetic interview preparation",
                    ContextSha256 = new string('b', 64),
                    PromptText = "Synthetic prompt",
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now,
                });
                await context.SaveChangesAsync();
            }

            await using var readContext = new ApplicationTrackerDbContext(options);
            var actual = await readContext.AssistantSessions.AsNoTracking().SingleAsync(item => item.Id == id);
            Assert.Equal(AssistantTaskKind.InterviewPreparation, actual.TaskKind);
            Assert.Equal(AssistantSessionStatus.Prepared, actual.Status);
            Assert.Equal("Synthetic prompt", actual.PromptText);
        }
        finally
        {
            TryDelete(path);
            TryDelete(path + "-shm");
            TryDelete(path + "-wal");
        }
    }

    private static void TryDelete(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
