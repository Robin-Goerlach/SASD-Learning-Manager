using SASD.Bewerbungsmanager.Domain.Entities;
using SASD.Bewerbungsmanager.Domain.Enums;

namespace SASD.Bewerbungsmanager.Domain.Tests;

/// <summary>Verifies the small lifecycle rules of imported communication records.</summary>
public sealed class CommunicationDomainTests
{
    [Fact]
    public void LinkAndIgnore_UpdateProcessingStateAndTimestamp()
    {
        var importedAt = new DateTimeOffset(2026, 8, 27, 8, 0, 0, TimeSpan.Zero);
        var linkedAt = importedAt.AddMinutes(5);
        var ignoredAt = importedAt.AddMinutes(10);
        var opportunityId = Guid.NewGuid();
        var message = new CommunicationMessage
        {
            Id = Guid.NewGuid(),
            SourceSystem = "Test",
            FingerprintSha256 = new string('a', 64),
            Subject = "Synthetic message",
            BodyText = "Synthetic body",
            Kind = CommunicationKind.General,
            Status = CommunicationStatus.Imported,
            ImportedAtUtc = importedAt,
            UpdatedAtUtc = importedAt,
        };

        message.LinkContext(opportunityId, null, null, null, linkedAt);

        Assert.Equal(CommunicationStatus.Linked, message.Status);
        Assert.Equal(opportunityId, message.OpportunityId);
        Assert.Equal(linkedAt, message.UpdatedAtUtc);

        message.Ignore(ignoredAt);

        Assert.Equal(CommunicationStatus.Ignored, message.Status);
        Assert.Equal(ignoredAt, message.UpdatedAtUtc);
    }
}
