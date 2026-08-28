using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SASD.Bewerbungsmanager.Application.Abstractions;
using SASD.Bewerbungsmanager.Infrastructure.Operations;
using SASD.Bewerbungsmanager.Infrastructure.Persistence;

namespace SASD.Bewerbungsmanager.Infrastructure;

/// <summary>Registers SQLite and infrastructure adapters.</summary>
public static class InfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Adds the local SQLite persistence and technical local-data services. A configured Database:Path
    /// overrides the default LocalApplicationData database and is primarily intended for tests.
    /// </summary>
    public static IServiceCollection AddTrackerInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var configuredPath = configuration["Database:Path"];
        var databasePath = string.IsNullOrWhiteSpace(configuredPath)
            ? AppDataPath.GetDefaultDatabasePath()
            : Path.GetFullPath(Environment.ExpandEnvironmentVariables(configuredPath));

        var directory = Path.GetDirectoryName(databasePath)
            ?? throw new InvalidOperationException("Das Datenbankverzeichnis konnte nicht ermittelt werden.");
        Directory.CreateDirectory(directory);

        // For configured/test databases keep all technical companion files beside the test database
        // instead of touching the user's real LocalApplicationData tree.
        var applicationDirectory = string.IsNullOrWhiteSpace(configuredPath)
            ? AppDataPath.GetApplicationDirectory()
            : Path.Combine(directory, Path.GetFileNameWithoutExtension(databasePath) + ".data");
        Directory.CreateDirectory(applicationDirectory);

        var storagePaths = new TrackerStoragePaths(applicationDirectory, databasePath);
        services.AddSingleton(storagePaths);
        services.AddDbContextFactory<ApplicationTrackerDbContext>(options =>
            options.UseSqlite($"Data Source={databasePath};Foreign Keys=True"));

        services.AddSingleton<ITrackerDataStore, TrackerDataStore>();
        services.AddSingleton<IDocumentArchive, FileSystemDocumentArchive>();
        services.AddSingleton<IApplicationExportWriter, FileSystemApplicationExportWriter>();
        services.AddSingleton<ICommunicationHandoffReader, JsonCommunicationHandoffReader>();
        services.AddSingleton<IJobSourceReader, JsonJobSourceReader>();
        services.AddSingleton<IJobSourceReader, CsvJobSourceReader>();
        services.AddSingleton<DatabaseInitializer>();

        services.AddSingleton<TrackerBackupService>();
        services.AddSingleton<PasswordProtectedBackupService>();
        services.AddSingleton<TrackerBackupCoordinator>();
        services.AddSingleton<PendingRestoreCoordinator>();
        services.AddSingleton<SnapshotPathRelocator>();
        services.AddSingleton<TrackerDiagnosticsService>();
        services.AddSingleton<ReleaseReadinessService>();
        return services;
    }
}
