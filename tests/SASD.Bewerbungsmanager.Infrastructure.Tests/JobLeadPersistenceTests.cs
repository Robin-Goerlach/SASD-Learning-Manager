using Microsoft.EntityFrameworkCore;
using SASD.Bewerbungsmanager.Domain.Entities;
using SASD.Bewerbungsmanager.Domain.Enums;
using SASD.Bewerbungsmanager.Infrastructure.Persistence;

namespace SASD.Bewerbungsmanager.Infrastructure.Tests;

public sealed class JobLeadPersistenceTests
{
    [Fact]
    public async Task MigrationAndJobLead_RoundTripThroughSqlite()
    {
        var path = Path.Combine(Path.GetTempPath(), $"sasd-joblead-{Guid.NewGuid():N}.db");
        try
        {
            var options = new DbContextOptionsBuilder<ApplicationTrackerDbContext>()
                .UseSqlite($"Data Source={path};Pooling=False;Foreign Keys=True")
                .Options;
            await using (var context = new ApplicationTrackerDbContext(options))
            {
                await context.Database.MigrateAsync();
                context.JobLeads.Add(new JobLead
                {
                    Id = Guid.NewGuid(),
                    SourceSystem = "Example Portal",
                    ExternalJobId = "job-1",
                    FingerprintSha256 = new string('a', 64),
                    Title = "Linux Engineer",
                    SourceUrl = "https://jobs.example.invalid/1",
                    FoundAtUtc = DateTimeOffset.UtcNow,
                    Status = JobLeadStatus.New,
                    CreatedAtUtc = DateTimeOffset.UtcNow,
                    UpdatedAtUtc = DateTimeOffset.UtcNow,
                });
                await context.SaveChangesAsync();
            }

            await using var readContext = new ApplicationTrackerDbContext(options);
            var actual = await readContext.JobLeads.AsNoTracking().SingleAsync();
            Assert.Equal("Linux Engineer", actual.Title);
            Assert.Equal(JobLeadStatus.New, actual.Status);
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
