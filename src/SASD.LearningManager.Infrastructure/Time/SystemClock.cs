using SASD.LearningManager.Application.Abstractions;

namespace SASD.LearningManager.Infrastructure.Time;

/// <summary>Production clock backed by <see cref="DateTimeOffset.UtcNow"/>.</summary>
public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
