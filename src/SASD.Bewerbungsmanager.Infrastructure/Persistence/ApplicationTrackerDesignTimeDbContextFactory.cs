using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SASD.Bewerbungsmanager.Infrastructure.Persistence;

/// <summary>
/// Creates a context for EF Core design-time commands such as <c>dotnet ef migrations add</c>.
/// The design-time database is intentionally placed in the temporary directory so migration
/// tooling never creates a personal-data database inside the Git repository.
/// </summary>
public sealed class ApplicationTrackerDesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationTrackerDbContext>
{
    /// <inheritdoc />
    public ApplicationTrackerDbContext CreateDbContext(string[] args)
    {
        var databasePath = Path.Combine(Path.GetTempPath(), "sasd-bewerbungsmanager-design-time.db");
        var options = new DbContextOptionsBuilder<ApplicationTrackerDbContext>()
            .UseSqlite($"Data Source={databasePath};Foreign Keys=True")
            .Options;

        return new ApplicationTrackerDbContext(options);
    }
}
