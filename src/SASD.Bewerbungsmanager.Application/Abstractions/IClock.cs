namespace SASD.Bewerbungsmanager.Application.Abstractions;

/// <summary>
/// Supplies the current time to application services so timestamp-sensitive behavior remains testable.
/// </summary>
public interface IClock
{
    /// <summary>Gets the current UTC time.</summary>
    DateTimeOffset UtcNow { get; }
}
