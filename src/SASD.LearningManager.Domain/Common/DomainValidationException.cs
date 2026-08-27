namespace SASD.LearningManager.Domain.Common;

/// <summary>
/// Represents a violated business invariant or an invalid value supplied to a domain object.
/// </summary>
public sealed class DomainValidationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DomainValidationException"/> class.
    /// </summary>
    /// <param name="message">Human-readable description of the violated domain rule.</param>
    public DomainValidationException(string message)
        : base(message)
    {
    }
}
