using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SASD.Bewerbungsmanager.Infrastructure.Persistence.Migrations;

[DbContext(typeof(ApplicationTrackerDbContext))]
[Migration("202608270004_JobSearchAdapters")]
partial class JobSearchAdapters
{
    /// <inheritdoc />
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
        => ApplicationTrackerModelSnapshot.BuildJobSearchModel(modelBuilder);
}
