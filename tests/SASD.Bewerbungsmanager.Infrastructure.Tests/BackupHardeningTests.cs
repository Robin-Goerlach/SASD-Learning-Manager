using System.IO.Compression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SASD.Bewerbungsmanager.Infrastructure;
using SASD.Bewerbungsmanager.Infrastructure.Operations;
using SASD.Bewerbungsmanager.Infrastructure.Persistence;

namespace SASD.Bewerbungsmanager.Infrastructure.Tests;

/// <summary>Integration coverage for the v0.6.0 local-data hardening boundary.</summary>
public sealed class BackupHardeningTests
{
    [Fact]
    public async Task Backup_roundtrip_validates_against_current_migrations()
    {
        var environment = await CreateEnvironmentAsync();
        try
        {
            var backup = Path.Combine(environment.Root, "tracker.zip");
            var service = environment.Provider.GetRequiredService<TrackerBackupService>();

            var created = await service.CreateBackupAsync(backup);
            var validation = await service.ValidateBackupAsync(backup);

            Assert.True(validation.IsValid, string.Join(Environment.NewLine, validation.Errors));
            Assert.True(created.FileCount >= 1);
            Assert.Contains(validation.Manifest!.Files, item => item.Path == "database/application-tracker.db");
        }
        finally
        {
            environment.Dispose();
        }
    }

    [Fact]
    public async Task Validation_rejects_tampered_authoritative_file()
    {
        var environment = await CreateEnvironmentAsync();
        try
        {
            var backup = Path.Combine(environment.Root, "tracker.zip");
            var service = environment.Provider.GetRequiredService<TrackerBackupService>();
            await service.CreateBackupAsync(backup);

            var extracted = Path.Combine(environment.Root, "tampered");
            ZipFile.ExtractToDirectory(backup, extracted);
            await File.AppendAllTextAsync(Path.Combine(extracted, "database", "application-tracker.db"), "tamper");
            File.Delete(backup);
            ZipFile.CreateFromDirectory(extracted, backup);

            var validation = await service.ValidateBackupAsync(backup);
            Assert.False(validation.IsValid);
            Assert.Contains(validation.Errors, item => item.Contains("Dateigröße", StringComparison.Ordinal));
        }
        finally
        {
            environment.Dispose();
        }
    }

    [Fact]
    public async Task Stage_restore_creates_restart_boundary_without_replacing_live_database()
    {
        var environment = await CreateEnvironmentAsync();
        try
        {
            var backup = Path.Combine(environment.Root, "tracker.zip");
            var service = environment.Provider.GetRequiredService<TrackerBackupService>();
            var paths = environment.Provider.GetRequiredService<TrackerStoragePaths>();
            await service.CreateBackupAsync(backup);
            var before = File.GetLastWriteTimeUtc(paths.DatabasePath);

            var staged = await service.StageRestoreAsync(backup);

            Assert.True(File.Exists(paths.PendingRestorePath));
            Assert.True(staged.FileCount >= 1);
            Assert.Equal(before, File.GetLastWriteTimeUtc(paths.DatabasePath));
        }
        finally
        {
            environment.Dispose();
        }
    }

    [Fact]
    public async Task Diagnostics_reports_integrity_and_counts_only()
    {
        var environment = await CreateEnvironmentAsync();
        try
        {
            var diagnostics = environment.Provider.GetRequiredService<TrackerDiagnosticsService>();
            var report = await diagnostics.CreateReportAsync();

            Assert.True(string.Equals("ok", report.QuickCheck, StringComparison.OrdinalIgnoreCase));
            Assert.Equal(0, report.ForeignKeyViolationCount);
            Assert.Contains("applications", report.RecordCounts.Keys);
            Assert.Empty(report.PendingMigrations);
        }
        finally
        {
            environment.Dispose();
        }
    }

    private static async Task<TestEnvironment> CreateEnvironmentAsync()
    {
        var root = Path.Combine(Path.GetTempPath(), $"sasd-hardening-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        var configuration = new ConfigurationManager
        {
            ["Database:Path"] = Path.Combine(root, "tracker.db"),
        };
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddTrackerInfrastructure(configuration);
        var provider = services.BuildServiceProvider();
        await provider.GetRequiredService<DatabaseInitializer>().InitializeAsync();
        return new TestEnvironment(root, provider);
    }

    private sealed class TestEnvironment(string root, ServiceProvider provider) : IDisposable
    {
        public string Root { get; } = root;
        public ServiceProvider Provider { get; } = provider;

        public void Dispose()
        {
            Provider.Dispose();
            if (Directory.Exists(Root))
            {
                Directory.Delete(Root, recursive: true);
            }
        }
    }
}
