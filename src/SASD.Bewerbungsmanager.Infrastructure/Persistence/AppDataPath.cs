namespace SASD.Bewerbungsmanager.Infrastructure.Persistence;

/// <summary>Resolves writable per-user paths without placing personal data inside the repository.</summary>
public static class AppDataPath
{
    private const string CompanyDirectory = "SASD GmbH";
    private const string ProductDirectory = "SASD Bewerbungsmanager";

    /// <summary>Returns the default local SQLite database path and ensures its parent directory exists.</summary>
    public static string GetDefaultDatabasePath()
        => Path.Combine(GetApplicationDirectory(), "application-tracker.db");

    /// <summary>Returns the local application-data root used by database, logs, and private snapshots.</summary>
    public static string GetApplicationDirectory()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(localAppData))
        {
            throw new InvalidOperationException("Das lokale Anwendungsdatenverzeichnis konnte nicht ermittelt werden.");
        }

        var directory = Path.Combine(localAppData, CompanyDirectory, ProductDirectory);
        Directory.CreateDirectory(directory);
        return directory;
    }

    /// <summary>Returns and creates the root directory containing private document snapshots.</summary>
    public static string GetDocumentsDirectory()
    {
        var directory = Path.Combine(GetApplicationDirectory(), "Documents");
        Directory.CreateDirectory(directory);
        return directory;
    }

    /// <summary>Returns and creates the private snapshot directory for one concrete application.</summary>
    public static string GetApplicationDocumentDirectory(Guid applicationId)
    {
        var directory = Path.Combine(GetDocumentsDirectory(), applicationId.ToString("N"));
        Directory.CreateDirectory(directory);
        return directory;
    }
}
