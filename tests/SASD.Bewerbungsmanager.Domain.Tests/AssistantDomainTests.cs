using SASD.Bewerbungsmanager.Domain.Entities;
using SASD.Bewerbungsmanager.Domain.Enums;

namespace SASD.Bewerbungsmanager.Domain.Tests;

public sealed class AssistantDomainTests
{
    [Fact]
    public void Complete_StoresResponseAndCompletionMetadata()
    {
        var completedAt = new DateTimeOffset(2026, 8, 27, 12, 0, 0, TimeSpan.Zero);
        var session = new AssistantSession
        {
            Id = Guid.NewGuid(),
            TaskKind = AssistantTaskKind.FitAnalysis,
            Status = AssistantSessionStatus.Prepared,
            Title = "Synthetic assistant session",
            ContextSha256 = new string('a', 64),
            PromptText = "Synthetic prompt",
            CreatedAtUtc = completedAt.AddMinutes(-5),
            UpdatedAtUtc = completedAt.AddMinutes(-5),
        };

        session.Complete(" Synthetic response ", " Local model ", completedAt);

        Assert.Equal(AssistantSessionStatus.Completed, session.Status);
        Assert.Equal("Synthetic response", session.ResponseText);
        Assert.Equal("Local model", session.ProviderLabel);
        Assert.Equal(completedAt, session.CompletedAtUtc);
        Assert.Equal(completedAt, session.UpdatedAtUtc);
    }

    [Fact]
    public void DiscardedSession_CannotBeCompleted()
    {
        var session = new AssistantSession { Status = AssistantSessionStatus.Prepared };
        session.Discard(DateTimeOffset.UtcNow);

        Assert.Throws<InvalidOperationException>(() =>
            session.Complete("Response", null, DateTimeOffset.UtcNow));
    }
}
