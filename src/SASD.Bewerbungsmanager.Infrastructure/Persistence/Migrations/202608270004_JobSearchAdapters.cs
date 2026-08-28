using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SASD.Bewerbungsmanager.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class JobSearchAdapters : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "job_leads",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                SearchProfileId = table.Column<Guid>(type: "TEXT", nullable: true),
                SourceSystem = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                ExternalJobId = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                FingerprintSha256 = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                Title = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                OrganizationName = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                Location = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                RemoteText = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                SalaryText = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                SourceUrl = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true),
                DescriptionText = table.Column<string>(type: "TEXT", maxLength: 100000, nullable: true),
                PublishedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                FoundAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                OpportunityId = table.Column<Guid>(type: "TEXT", nullable: true),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_job_leads", x => x.Id);
                table.ForeignKey(
                    name: "FK_job_leads_opportunities_OpportunityId",
                    column: x => x.OpportunityId,
                    principalTable: "opportunities",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_job_leads_search_profiles_SearchProfileId",
                    column: x => x.SearchProfileId,
                    principalTable: "search_profiles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateIndex(
            name: "IX_job_leads_FingerprintSha256",
            table: "job_leads",
            column: "FingerprintSha256",
            unique: true);
        migrationBuilder.CreateIndex(
            name: "IX_job_leads_FoundAtUtc",
            table: "job_leads",
            column: "FoundAtUtc");
        migrationBuilder.CreateIndex(
            name: "IX_job_leads_OpportunityId",
            table: "job_leads",
            column: "OpportunityId");
        migrationBuilder.CreateIndex(
            name: "IX_job_leads_SearchProfileId",
            table: "job_leads",
            column: "SearchProfileId");
        migrationBuilder.CreateIndex(
            name: "IX_job_leads_SourceSystem_ExternalJobId",
            table: "job_leads",
            columns: new[] { "SourceSystem", "ExternalJobId" });
        migrationBuilder.CreateIndex(
            name: "IX_job_leads_SourceUrl",
            table: "job_leads",
            column: "SourceUrl");
        migrationBuilder.CreateIndex(
            name: "IX_job_leads_Status",
            table: "job_leads",
            column: "Status");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
        => migrationBuilder.DropTable(name: "job_leads");
}
