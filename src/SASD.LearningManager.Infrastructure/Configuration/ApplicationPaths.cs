namespace SASD.LearningManager.Infrastructure.Configuration;

/// <summary>Centralizes per-user application paths so database and log locations are not scattered through the UI.</summary>
public sealed class ApplicationPaths
{
    private ApplicationPaths(string rootDirectory)
    {
        RootDirectory = rootDirectory;
        DataDirectory = Path.Combine(rootDirectory, "data");
        LogDirectory = Path.Combine(rootDirectory, "logs");
        BackupDirectory = Path.Combine(rootDirectory, "backups");
        DatabasePath = Path.Combine(DataDirectory, "learning-manager.db");
        SettingsPath = Path.Combine(rootDirectory, "settings.json");
    }

    public string RootDirectory { get; }
    public string DataDirectory { get; }
    public string LogDirectory { get; }
    public string BackupDirectory { get; }
    public string DatabasePath { get; }
    public string SettingsPath { get; }

    /// <summary>Creates the standard local application-data layout and ensures its directories exist.</summary>
    public static ApplicationPaths CreateDefault()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var root = Path.Combine(localAppData, "SASD", "LearningManager");
        var paths = new ApplicationPaths(root);
        Directory.CreateDirectory(paths.DataDirectory);
        Directory.CreateDirectory(paths.LogDirectory);
        Directory.CreateDirectory(paths.BackupDirectory);
        return paths;
    }
}
