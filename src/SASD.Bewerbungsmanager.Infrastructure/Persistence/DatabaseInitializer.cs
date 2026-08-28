using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SASD.Bewerbungsmanager.Infrastructure.Persistence;

/// <summary>Applies pending EF Core migrations before the main form starts using the local database.</summary>
public sealed class DatabaseInitializer(
    IDbContextFactory<ApplicationTrackerDbContext> contextFactory,
    ILogger<DatabaseInitializer> logger)
{
    /// <summary>Creates or migrates the database to the schema expected by this application build.</summary>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        logger.LogInformation("Applying database migrations.");
        await context.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
        logger.LogInformation("Database migrations completed.");
    }
}
