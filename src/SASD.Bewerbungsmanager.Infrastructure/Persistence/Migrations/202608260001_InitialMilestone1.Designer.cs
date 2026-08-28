using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SASD.Bewerbungsmanager.Infrastructure.Persistence.Migrations;

[DbContext(typeof(ApplicationTrackerDbContext))]
[Migration("202608260001_InitialMilestone1")]
partial class InitialMilestone1
{
    /// <inheritdoc />
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
        => ApplicationTrackerModelSnapshot.BuildMilestone1Model(modelBuilder);
}
