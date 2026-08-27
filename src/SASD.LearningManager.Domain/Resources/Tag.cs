using SASD.LearningManager.Domain.Common;

namespace SASD.LearningManager.Domain.Resources;

/// <summary>Represents a lightweight cross-cutting label attached to resources.</summary>
public sealed record Tag
{
    /// <summary>Initializes a normalized tag.</summary>
    public Tag(string name)
    {
        Name = Guard.RequiredText(name, "Tag", 100);
    }

    public string Name { get; }
}
