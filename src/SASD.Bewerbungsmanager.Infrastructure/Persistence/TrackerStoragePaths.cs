namespace SASD.Bewerbungsmanager.Infrastructure.Persistence;

/// <summary>
/// Holds the authoritative local paths used by the tracker. Keeping the resolved database path in
/// one immutable object prevents backup, restore and EF Core from silently operating on different files.
/// </summary>
public sealed record TrackerStoragePaths(string ApplicationDirectory, string DatabasePath)
{
    /// <summary>Gets the root directory containing immutable application-document snapshots.</summary>
    public string DocumentsDirectory => Path.Combine(ApplicationDirectory, "Documents");

    /// <summary>Gets the private working directory used while a restore waits for the next startup.</summary>
    public string RestoreStagingDirectory => Path.Combine(ApplicationDirectory, "RestoreStaging");

    /// <summary>Gets the marker that makes a staged restore explicit and restart-bound.</summary>
    public string PendingRestorePath => Path.Combine(ApplicationDirectory, "pending-restore.json");

    /// <summary>Gets the directory in which pre-restore recovery copies are retained.</summary>
    public string RecoveryDirectory => Path.Combine(ApplicationDirectory, "RestoreRecovery");
}
