namespace SASD.Bewerbungsmanager.Application.Exceptions;

/// <summary>
/// Represents user-correctable validation failures detected before data reaches persistence.
/// </summary>
public sealed class ValidationException : Exception
{
    /// <summary>Initializes a new validation exception.</summary>
    public ValidationException(string message)
        : base(message)
    {
    }
}
