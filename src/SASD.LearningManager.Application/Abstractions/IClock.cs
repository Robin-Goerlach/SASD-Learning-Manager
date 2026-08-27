namespace SASD.LearningManager.Application.Abstractions;

/// <summary>Provides the current time to application use cases in a testable form.</summary>
public interface IClock
{
    /// <summary>Gets the current UTC timestamp.</summary>
    DateTimeOffset UtcNow { get; }
}
