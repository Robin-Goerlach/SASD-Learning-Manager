using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SASD.Bewerbungsmanager.Infrastructure.Persistence;

namespace SASD.Bewerbungsmanager.Infrastructure.Operations;

/// <summary>
/// Rebinds immutable document snapshot paths to the current LocalApplicationData root after a restore.
/// This is necessary when a valid backup is restored under a different Windows profile or machine.
/// </summary>
public sealed class SnapshotPathRelocator(
    IDbContextFactory<ApplicationTrackerDbContext> contextFactory,
    TrackerStoragePaths storagePaths,
    ILogger<SnapshotPathRelocator> logger)
{
    /// <summary>Repairs resolvable snapshot paths and leaves missing files untouched for diagnosis.</summary>
    public async Task RelocateAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var snapshots = await context.ApplicationDocumentSnapshots.ToListAsync(cancellationToken).ConfigureAwait(false);
        var changed = false;

        foreach (var snapshot in snapshots)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var applicationDirectory = Path.Combine(storagePaths.DocumentsDirectory, snapshot.ApplicationId.ToString("N"));
            if (!Directory.Exists(applicationDirectory))
            {
                continue;
            }

            var candidates = Directory.EnumerateFiles(applicationDirectory, snapshot.Sha256 + "*", SearchOption.TopDirectoryOnly)
                .Where(path => string.Equals(Path.GetFileNameWithoutExtension(path), snapshot.Sha256, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(Path.GetFileName(path), snapshot.Sha256, StringComparison.OrdinalIgnoreCase))
                .Take(2)
                .ToArray();

            if (candidates.Length != 1)
            {
                continue;
            }

            var resolved = Path.GetFullPath(candidates[0]);
            if (!string.Equals(snapshot.StoredPath, resolved, StringComparison.OrdinalIgnoreCase))
            {
                snapshot.StoredPath = resolved;
                changed = true;
            }
        }

        if (changed)
        {
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            logger.LogInformation("Relocated restored application-document snapshot paths.");
        }
    }
}
