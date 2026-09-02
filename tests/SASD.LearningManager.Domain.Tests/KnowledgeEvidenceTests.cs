using SASD.LearningManager.Domain.Common;
using SASD.LearningManager.Domain.Evidence;
using SASD.LearningManager.Domain.Knowledge;

namespace SASD.LearningManager.Domain.Tests;

public sealed class KnowledgeEvidenceTests
{
    [Fact]
    public void Knowledge_artifact_preserves_markdown_and_can_be_archived()
    {
        var now = DateTimeOffset.Parse("2026-09-02T08:00:00Z");
        var item = KnowledgeArtifact.Create("LINQ notes", "# Select\n\n`Select` projects values.", KnowledgeArtifactType.Note, now);
        item.Archive(now.AddMinutes(1));
        Assert.Equal(KnowledgeArtifactStatus.Archived, item.Status);
        Assert.Contains("`Select`", item.Markdown, StringComparison.Ordinal);
    }

    [Fact]
    public void Archived_knowledge_cannot_be_edited_until_restored()
    {
        var now = DateTimeOffset.Parse("2026-09-02T08:00:00Z");
        var item = KnowledgeArtifact.Create("SQL notes", "Initial", KnowledgeArtifactType.Note, now);
        item.Archive(now.AddMinutes(1));
        Assert.Throws<DomainValidationException>(() => item.Update("Changed", "Changed", KnowledgeArtifactType.Summary, now.AddMinutes(2)));
    }

    [Fact]
    public void Evidence_rejects_future_dates()
    {
        var now = DateTimeOffset.Parse("2026-09-02T08:00:00Z");
        Assert.Throws<DomainValidationException>(() => EvidenceItem.Create("Synthetic lab", null, EvidenceType.Lab,
            now.AddHours(1), null, null, null, now));
    }

    [Fact]
    public void Evidence_does_not_model_a_skill_level()
    {
        var now = DateTimeOffset.Parse("2026-09-02T08:00:00Z");
        var item = EvidenceItem.Create("Synthetic API lab", "Implemented a sample endpoint", EvidenceType.Lab,
            now, "https://example.test/lab", null, "Reviewed locally", now);
        Assert.Equal(EvidenceType.Lab, item.Type);
        Assert.DoesNotContain(typeof(EvidenceItem).GetProperties(), property => property.Name.Contains("Level", StringComparison.Ordinal));
    }
}
