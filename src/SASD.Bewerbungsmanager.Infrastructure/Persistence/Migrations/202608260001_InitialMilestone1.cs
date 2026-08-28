using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SASD.Bewerbungsmanager.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class InitialMilestone1 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "organizations",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                Type = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                Website = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true),
                Notes = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                IsArchived = table.Column<bool>(type: "INTEGER", nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
            },
            constraints: table => table.PrimaryKey("PK_organizations", x => x.Id));

        migrationBuilder.CreateTable(
            name: "contacts",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                OrganizationId = table.Column<Guid>(type: "TEXT", nullable: true),
                FullName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                Role = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                Email = table.Column<string>(type: "TEXT", maxLength: 320, nullable: true),
                Phone = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                LinkedInUrl = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true),
                Notes = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                IsArchived = table.Column<bool>(type: "INTEGER", nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_contacts", x => x.Id);
                table.ForeignKey("FK_contacts_organizations_OrganizationId", x => x.OrganizationId, "organizations", "Id", onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "opportunities",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                EmployerOrganizationId = table.Column<Guid>(type: "TEXT", nullable: true),
                IntermediaryOrganizationId = table.Column<Guid>(type: "TEXT", nullable: true),
                Title = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                DescriptionSnapshot = table.Column<string>(type: "TEXT", maxLength: 100000, nullable: false),
                Location = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                RemoteText = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                SalaryText = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                FoundAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                PublishedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                DeadlineAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_opportunities", x => x.Id);
                table.ForeignKey("FK_opportunities_organizations_EmployerOrganizationId", x => x.EmployerOrganizationId, "organizations", "Id", onDelete: ReferentialAction.SetNull);
                table.ForeignKey("FK_opportunities_organizations_IntermediaryOrganizationId", x => x.IntermediaryOrganizationId, "organizations", "Id", onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "applications",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                OpportunityId = table.Column<Guid>(type: "TEXT", nullable: false),
                StartedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                SubmittedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                Stage = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                Channel = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                SalaryExpectation = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                ClosedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                Outcome = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_applications", x => x.Id);
                table.ForeignKey("FK_applications_opportunities_OpportunityId", x => x.OpportunityId, "opportunities", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "source_links",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                OpportunityId = table.Column<Guid>(type: "TEXT", nullable: false),
                Source = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                Url = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false),
                ExternalId = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                CapturedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_source_links", x => x.Id);
                table.ForeignKey("FK_source_links_opportunities_OpportunityId", x => x.OpportunityId, "opportunities", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "application_status_history",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                ApplicationId = table.Column<Guid>(type: "TEXT", nullable: false),
                Stage = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                ChangedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                Note = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_application_status_history", x => x.Id);
                table.ForeignKey("FK_application_status_history_applications_ApplicationId", x => x.ApplicationId, "applications", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex("IX_organizations_Name", "organizations", "Name");
        migrationBuilder.CreateIndex("IX_contacts_FullName", "contacts", "FullName");
        migrationBuilder.CreateIndex("IX_contacts_OrganizationId", "contacts", "OrganizationId");
        migrationBuilder.CreateIndex("IX_opportunities_EmployerOrganizationId", "opportunities", "EmployerOrganizationId");
        migrationBuilder.CreateIndex("IX_opportunities_IntermediaryOrganizationId", "opportunities", "IntermediaryOrganizationId");
        migrationBuilder.CreateIndex("IX_opportunities_Status", "opportunities", "Status");
        migrationBuilder.CreateIndex("IX_opportunities_FoundAtUtc", "opportunities", "FoundAtUtc");
        migrationBuilder.CreateIndex("IX_source_links_OpportunityId", "source_links", "OpportunityId");
        migrationBuilder.CreateIndex("IX_applications_OpportunityId", "applications", "OpportunityId");
        migrationBuilder.CreateIndex("IX_applications_Stage", "applications", "Stage");
        migrationBuilder.CreateIndex(
            "IX_application_status_history_ApplicationId_ChangedAtUtc",
            "application_status_history",
            new[] { "ApplicationId", "ChangedAtUtc" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("application_status_history");
        migrationBuilder.DropTable("source_links");
        migrationBuilder.DropTable("applications");
        migrationBuilder.DropTable("contacts");
        migrationBuilder.DropTable("opportunities");
        migrationBuilder.DropTable("organizations");
    }
}
