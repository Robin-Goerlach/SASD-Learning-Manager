using SASD.Bewerbungsmanager.Infrastructure.Persistence;
using Xunit;

namespace SASD.Bewerbungsmanager.SystemTests;

/// <summary>
/// Verifies that the default application database is stored below the current
/// user's local application-data directory instead of inside the repository.
/// </summary>
public sealed class ApplicationPathsTests
{
    /// <summary>
    /// Ensures that <see cref="AppDataPath.GetDefaultDatabasePath"/> returns
    /// the exact per-user path expected by the application and creates the
    /// containing directory when necessary.
    /// </summary>
    [Fact]
    public void GetDefaultDatabasePath_ReturnsExpectedPerUserDatabasePath()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        Assert.False(string.IsNullOrWhiteSpace(localAppData));

        var expectedDirectory = Path.Combine(localAppData, "SASD GmbH", "SASD Bewerbungsmanager");
        var expectedPath = Path.Combine(expectedDirectory, "application-tracker.db");

        var actualPath = AppDataPath.GetDefaultDatabasePath();

        // An exact path assertion is stronger and produces a more useful failure
        // message than wrapping string.StartsWith/Contains in Assert.True.
        Assert.Equal(expectedPath, actualPath);
        Assert.True(Directory.Exists(expectedDirectory));
    }
}
