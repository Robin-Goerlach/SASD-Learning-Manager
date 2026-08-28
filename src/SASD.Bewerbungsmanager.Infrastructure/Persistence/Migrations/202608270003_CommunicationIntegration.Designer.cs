using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SASD.Bewerbungsmanager.Infrastructure.Persistence.Migrations;

[DbContext(typeof(ApplicationTrackerDbContext))]
[Migration("202608270003_CommunicationIntegration")]
partial class CommunicationIntegration
{
    /// <inheritdoc />
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
        => ApplicationTrackerModelSnapshot.BuildCommunicationIntegrationModel(modelBuilder);
}
