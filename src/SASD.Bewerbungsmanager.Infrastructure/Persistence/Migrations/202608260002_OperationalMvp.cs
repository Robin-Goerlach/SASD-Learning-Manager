using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SASD.Bewerbungsmanager.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class OperationalMvp : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "activities",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                OpportunityId = table.Column<Guid>(type: "TEXT", nullable: true),
                ApplicationId = table.Column<Guid>(type: "TEXT", nullable: true),
                ContactId = table.Column<Guid>(type: "TEXT", nullable: true),
                OrganizationId = table.Column<Guid>(type: "TEXT", nullable: true),
                Kind = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                Subject = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                Notes = table.Column<string>(type: "TEXT", maxLength: 8000, nullable: true),
                OccurredAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                ScheduledAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                CompletedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_activities", x => x.Id);
                table.ForeignKey("FK_activities_applications_ApplicationId", x => x.ApplicationId, "applications", "Id", onDelete: ReferentialAction.SetNull);
                table.ForeignKey("FK_activities_contacts_ContactId", x => x.ContactId, "contacts", "Id", onDelete: ReferentialAction.SetNull);
                table.ForeignKey("FK_activities_opportunities_OpportunityId", x => x.OpportunityId, "opportunities", "Id", onDelete: ReferentialAction.SetNull);
                table.ForeignKey("FK_activities_organizations_OrganizationId", x => x.OrganizationId, "organizations", "Id", onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "documents",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Type = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                Label = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                Version = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                Language = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                Tags = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                OriginalPath = table.Column<string>(type: "TEXT", maxLength: 4096, nullable: false),
                Sha256 = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                SizeBytes = table.Column<long>(type: "INTEGER", nullable: false),
                IsArchived = table.Column<bool>(type: "INTEGER", nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
            },
            constraints: table => table.PrimaryKey("PK_documents", x => x.Id));

        migrationBuilder.CreateTable(
            name: "search_profiles",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                Source = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                Url = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false),
                CheckIntervalDays = table.Column<int>(type: "INTEGER", nullable: false),
                LastCheckedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                NextCheckAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                Notes = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
            },
            constraints: table => table.PrimaryKey("PK_search_profiles", x => x.Id));

        migrationBuilder.CreateTable(
            name: "work_items",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                OpportunityId = table.Column<Guid>(type: "TEXT", nullable: true),
                ApplicationId = table.Column<Guid>(type: "TEXT", nullable: true),
                ContactId = table.Column<Guid>(type: "TEXT", nullable: true),
                OrganizationId = table.Column<Guid>(type: "TEXT", nullable: true),
                Kind = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                Title = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                Notes = table.Column<string>(type: "TEXT", maxLength: 8000, nullable: true),
                DueAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                CompletedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_work_items", x => x.Id);
                table.ForeignKey("FK_work_items_applications_ApplicationId", x => x.ApplicationId, "applications", "Id", onDelete: ReferentialAction.SetNull);
                table.ForeignKey("FK_work_items_contacts_ContactId", x => x.ContactId, "contacts", "Id", onDelete: ReferentialAction.SetNull);
                table.ForeignKey("FK_work_items_opportunities_OpportunityId", x => x.OpportunityId, "opportunities", "Id", onDelete: ReferentialAction.SetNull);
                table.ForeignKey("FK_work_items_organizations_OrganizationId", x => x.OrganizationId, "organizations", "Id", onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "application_document_snapshots",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                ApplicationId = table.Column<Guid>(type: "TEXT", nullable: false),
                DocumentId = table.Column<Guid>(type: "TEXT", nullable: true),
                Type = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                Label = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                Version = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                Language = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                OriginalPath = table.Column<string>(type: "TEXT", maxLength: 4096, nullable: false),
                StoredPath = table.Column<string>(type: "TEXT", maxLength: 4096, nullable: false),
                Sha256 = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                CapturedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_application_document_snapshots", x => x.Id);
                table.ForeignKey("FK_application_document_snapshots_applications_ApplicationId", x => x.ApplicationId, "applications", "Id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_application_document_snapshots_documents_DocumentId", x => x.DocumentId, "documents", "Id", onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateIndex("IX_activities_ApplicationId", "activities", "ApplicationId");
        migrationBuilder.CreateIndex("IX_activities_ContactId", "activities", "ContactId");
        migrationBuilder.CreateIndex("IX_activities_OpportunityId", "activities", "OpportunityId");
        migrationBuilder.CreateIndex("IX_activities_OrganizationId", "activities", "OrganizationId");
        migrationBuilder.CreateIndex("IX_activities_ScheduledAtUtc", "activities", "ScheduledAtUtc");
        migrationBuilder.CreateIndex("IX_documents_Sha256", "documents", "Sha256");
        migrationBuilder.CreateIndex("IX_documents_Type_IsArchived", "documents", new[] { "Type", "IsArchived" });
        migrationBuilder.CreateIndex("IX_search_profiles_IsActive", "search_profiles", "IsActive");
        migrationBuilder.CreateIndex("IX_search_profiles_NextCheckAtUtc", "search_profiles", "NextCheckAtUtc");
        migrationBuilder.CreateIndex("IX_work_items_ApplicationId", "work_items", "ApplicationId");
        migrationBuilder.CreateIndex("IX_work_items_ContactId", "work_items", "ContactId");
        migrationBuilder.CreateIndex("IX_work_items_DueAtUtc", "work_items", "DueAtUtc");
        migrationBuilder.CreateIndex("IX_work_items_Kind_Status", "work_items", new[] { "Kind", "Status" });
        migrationBuilder.CreateIndex("IX_work_items_OpportunityId", "work_items", "OpportunityId");
        migrationBuilder.CreateIndex("IX_work_items_OrganizationId", "work_items", "OrganizationId");
        migrationBuilder.CreateIndex("IX_application_document_snapshots_ApplicationId", "application_document_snapshots", "ApplicationId");
        migrationBuilder.CreateIndex("IX_application_document_snapshots_DocumentId", "application_document_snapshots", "DocumentId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("activities");
        migrationBuilder.DropTable("application_document_snapshots");
        migrationBuilder.DropTable("search_profiles");
        migrationBuilder.DropTable("work_items");
        migrationBuilder.DropTable("documents");
    }
}
