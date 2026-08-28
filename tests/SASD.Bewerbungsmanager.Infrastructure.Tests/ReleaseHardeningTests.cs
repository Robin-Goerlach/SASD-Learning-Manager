using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SASD.Bewerbungsmanager.Infrastructure;
using SASD.Bewerbungsmanager.Infrastructure.Operations;

namespace SASD.Bewerbungsmanager.Infrastructure.Tests;

/// <summary>Integration coverage for the v0.7.0 release-hardening boundary.</summary>
public sealed class ReleaseHardeningTests
{
    [Fact]
    public async Task Password_protected_backup_roundtrip_preserves_plaintext_without_exposing_it()
    {
        var root = CreateTemporaryDirectory();
        try
        {
            var source = Path.Combine(root, "source.zip");
            var encrypted = Path.Combine(root, "backup.sasdbak");
            var restored = Path.Combine(root, "restored.zip");
            var payload = Encoding.UTF8.GetBytes("manifest.json\nsynthetic-private-backup-content");
            await File.WriteAllBytesAsync(source, payload);

            var service = new PasswordProtectedBackupService();
            await service.EncryptFileAsync(source, encrypted, "correct horse battery staple");

            Assert.True(service.IsEncryptedBackup(encrypted));
            var encryptedText = Encoding.UTF8.GetString(await File.ReadAllBytesAsync(encrypted));
            Assert.False(encryptedText.Contains("manifest.json", StringComparison.Ordinal));

            await service.DecryptFileAsync(encrypted, restored, "correct horse battery staple");
            Assert.Equal(payload, await File.ReadAllBytesAsync(restored));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Password_protected_backup_rejects_wrong_password()
    {
        var root = CreateTemporaryDirectory();
        try
        {
            var source = Path.Combine(root, "source.zip");
            var encrypted = Path.Combine(root, "backup.sasdbak");
            var restored = Path.Combine(root, "restored.zip");
            await File.WriteAllTextAsync(source, "synthetic-content");

            var service = new PasswordProtectedBackupService();
            await service.EncryptFileAsync(source, encrypted, "correct horse battery staple");

            var exception = await Assert.ThrowsAsync<InvalidDataException>(
                () => service.DecryptFileAsync(encrypted, restored, "wrong password value"));
            Assert.Contains("falsch", exception.Message, StringComparison.OrdinalIgnoreCase);
            Assert.False(File.Exists(restored));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Coordinator_creates_and_validates_encrypted_tracker_backup()
    {
        var environment = await CreateEnvironmentAsync();
        try
        {
            var backupPath = Path.Combine(environment.Root, "tracker.sasdbak");
            var coordinator = environment.Provider.GetRequiredService<TrackerBackupCoordinator>();

            var created = await coordinator.CreateBackupAsync(backupPath, "synthetic backup password");
            var validation = await coordinator.ValidateBackupAsync(backupPath, "synthetic backup password");

            Assert.True(coordinator.RequiresPassword(backupPath));
            Assert.True(validation.IsValid, string.Join(Environment.NewLine, validation.Errors));
            Assert.Equal(Path.GetFullPath(backupPath), created.Path);
        }
        finally
        {
            environment.Dispose();
        }
    }

    [Fact]
    public async Task Coordinator_stages_encrypted_backup_without_leaving_decrypted_archive()
    {
        var environment = await CreateEnvironmentAsync();
        try
        {
            var backupPath = Path.Combine(environment.Root, "tracker.sasdbak");
            var coordinator = environment.Provider.GetRequiredService<TrackerBackupCoordinator>();
            var paths = environment.Provider.GetRequiredService<SASD.Bewerbungsmanager.Infrastructure.Persistence.TrackerStoragePaths>();

            await coordinator.CreateBackupAsync(backupPath, "synthetic backup password");
            var staged = await coordinator.StageRestoreAsync(backupPath, "synthetic backup password");

            Assert.Equal(Path.GetFullPath(backupPath), staged.SourcePath);
            Assert.True(File.Exists(paths.PendingRestorePath));
            Assert.Empty(Directory.EnumerateFiles(Path.Combine(paths.ApplicationDirectory, "BackupWork", "EncryptedEnvelope"), "*.zip"));
        }
        finally
        {
            environment.Dispose();
        }
    }

    [Fact]
    public async Task Release_readiness_reports_healthy_database_as_non_blocking()
    {
        var environment = await CreateEnvironmentAsync();
        try
        {
            var paths = environment.Provider.GetRequiredService<SASD.Bewerbungsmanager.Infrastructure.Persistence.TrackerStoragePaths>();
            Directory.CreateDirectory(Path.Combine(paths.RecoveryDirectory, "synthetic-restore-proof"));

            var service = environment.Provider.GetRequiredService<ReleaseReadinessService>();
            var report = await service.CreateReportAsync();

            Assert.True(report.IsReadyForRc);
            Assert.Contains(report.Gates, item => item.Id == "DB-001" && item.Severity == ReleaseGateSeverity.Passed);
            Assert.Contains(report.Gates, item => item.Id == "DB-003" && item.Severity == ReleaseGateSeverity.Passed);
            Assert.Contains(report.Gates, item => item.Id == "IO-001" && item.Severity == ReleaseGateSeverity.Passed);
        }
        finally
        {
            environment.Dispose();
        }
    }

    private static string CreateTemporaryDirectory()
    {
        var root = Path.Combine(Path.GetTempPath(), $"sasd-release-hardening-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        return root;
    }

    private static async Task<TestEnvironment> CreateEnvironmentAsync()
    {
        var root = CreateTemporaryDirectory();
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
