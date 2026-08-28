using System.Text;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SASD.Bewerbungsmanager.Domain.Entities;
using SASD.Bewerbungsmanager.Domain.Enums;
using SASD.Bewerbungsmanager.Infrastructure.Persistence;

namespace SASD.Bewerbungsmanager.Infrastructure.Tests;

/// <summary>Integration tests for the v0.3.0 communication persistence and local JSON handoff adapter.</summary>
public sealed class CommunicationInfrastructureTests
{
    [Fact]
    public async Task CommunicationMessage_RoundTripsThroughLatestMigration()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<ApplicationTrackerDbContext>()
            .UseSqlite(connection)
            .Options;

        await using (var setup = new ApplicationTrackerDbContext(options))
        {
            await setup.Database.MigrateAsync();
        }

        var now = new DateTimeOffset(2026, 8, 27, 8, 0, 0, TimeSpan.Zero);
        var message = new CommunicationMessage
        {
            Id = Guid.NewGuid(),
            SourceSystem = "SASD Mail Workbench",
            ExternalMessageId = "test-message-1",
            FingerprintSha256 = new string('b', 64),
            Direction = CommunicationDirection.Incoming,
            Kind = CommunicationKind.Recruiter,
            Status = CommunicationStatus.Imported,
            FromName = "Erika Beispiel",
            FromAddress = "erika@example.invalid",
            Subject = "Synthetic recruiter mail",
            BodyText = "Synthetic body.",
            MessageAtUtc = now,
            ImportedAtUtc = now,
            UpdatedAtUtc = now,
        };

        await using (var write = new ApplicationTrackerDbContext(options))
        {
            write.CommunicationMessages.Add(message);
            await write.SaveChangesAsync();
        }

        var store = new TrackerDataStore(new TestDbContextFactory(options));
        var loaded = await store.FindCommunicationMessageByExternalIdentityAsync("SASD Mail Workbench", "test-message-1");

        Assert.NotNull(loaded);
        Assert.Equal(message.Id, loaded.Id);
        Assert.Equal(CommunicationKind.Recruiter, loaded.Kind);
        Assert.Single(await store.ListCommunicationMessagesAsync());
    }

    [Fact]
    public async Task JsonHandoffReader_ReadsVersionedStringEnums()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"sasd-communication-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "handoff.json");
        try
        {
            var json = """
                {
                  "schemaVersion": 1,
                  "sourceSystem": "SASD Mail Workbench",
                  "messages": [
                    {
                      "externalMessageId": "abc-123",
                      "direction": "Incoming",
                      "kind": "Recruiter",
                      "fromName": "Erika Beispiel",
                      "fromAddress": "erika@example.invalid",
                      "toAddresses": "user@example.invalid",
                      "subject": "Synthetic message",
                      "bodyText": "Synthetic body",
                      "messageAtUtc": "2026-08-27T08:00:00+00:00",
                      "sourceReference": "Inbox/Test"
                    }
                  ]
                }
                """;
            await File.WriteAllTextAsync(path, json, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            var batch = await new JsonCommunicationHandoffReader().ReadAsync(path);

            Assert.Equal(1, batch.SchemaVersion);
            Assert.Equal("SASD Mail Workbench", batch.SourceSystem);
            var message = Assert.Single(batch.Messages);
            Assert.Equal(CommunicationDirection.Incoming, message.Direction);
            Assert.Equal(CommunicationKind.Recruiter, message.Kind);
            Assert.Equal("abc-123", message.ExternalMessageId);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private sealed class TestDbContextFactory(DbContextOptions<ApplicationTrackerDbContext> options)
        : IDbContextFactory<ApplicationTrackerDbContext>
    {
        public ApplicationTrackerDbContext CreateDbContext() => new(options);

        public Task<ApplicationTrackerDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(CreateDbContext());
    }
}
