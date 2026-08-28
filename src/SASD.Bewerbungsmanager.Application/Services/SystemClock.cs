using SASD.Bewerbungsmanager.Application.Abstractions;

namespace SASD.Bewerbungsmanager.Application.Services;

/// <summary>Production clock based on <see cref="DateTimeOffset.UtcNow"/>.</summary>
public sealed class SystemClock : IClock
{
    /// <inheritdoc />
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
