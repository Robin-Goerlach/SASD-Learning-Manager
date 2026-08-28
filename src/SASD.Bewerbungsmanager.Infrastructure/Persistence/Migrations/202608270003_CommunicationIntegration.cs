using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SASD.Bewerbungsmanager.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class CommunicationIntegration : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "communication_messages",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                SourceSystem = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                ExternalMessageId = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                FingerprintSha256 = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                Direction = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                Kind = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                FromName = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                FromAddress = table.Column<string>(type: "TEXT", maxLength: 320, nullable: true),
                ToAddresses = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                Subject = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                BodyText = table.Column<string>(type: "TEXT", maxLength: 100000, nullable: false),
                MessageAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                SourceReference = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true),
                OpportunityId = table.Column<Guid>(type: "TEXT", nullable: true),
                ApplicationId = table.Column<Guid>(type: "TEXT", nullable: true),
                ContactId = table.Column<Guid>(type: "TEXT", nullable: true),
                OrganizationId = table.Column<Guid>(type: "TEXT", nullable: true),
                ActivityId = table.Column<Guid>(type: "TEXT", nullable: true),
                ImportedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_communication_messages", x => x.Id);
                table.ForeignKey("FK_communication_messages_activities_ActivityId", x => x.ActivityId, "activities", "Id", onDelete: ReferentialAction.SetNull);
                table.ForeignKey("FK_communication_messages_applications_ApplicationId", x => x.ApplicationId, "applications", "Id", onDelete: ReferentialAction.SetNull);
                table.ForeignKey("FK_communication_messages_contacts_ContactId", x => x.ContactId, "contacts", "Id", onDelete: ReferentialAction.SetNull);
                table.ForeignKey("FK_communication_messages_opportunities_OpportunityId", x => x.OpportunityId, "opportunities", "Id", onDelete: ReferentialAction.SetNull);
                table.ForeignKey("FK_communication_messages_organizations_OrganizationId", x => x.OrganizationId, "organizations", "Id", onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateIndex("IX_communication_messages_ActivityId", "communication_messages", "ActivityId");
        migrationBuilder.CreateIndex("IX_communication_messages_ApplicationId", "communication_messages", "ApplicationId");
        migrationBuilder.CreateIndex("IX_communication_messages_ContactId", "communication_messages", "ContactId");
        migrationBuilder.CreateIndex("IX_communication_messages_FingerprintSha256", "communication_messages", "FingerprintSha256", unique: true);
        migrationBuilder.CreateIndex("IX_communication_messages_MessageAtUtc", "communication_messages", "MessageAtUtc");
        migrationBuilder.CreateIndex("IX_communication_messages_OpportunityId", "communication_messages", "OpportunityId");
        migrationBuilder.CreateIndex("IX_communication_messages_OrganizationId", "communication_messages", "OrganizationId");
        migrationBuilder.CreateIndex("IX_communication_messages_SourceSystem_ExternalMessageId", "communication_messages", new[] { "SourceSystem", "ExternalMessageId" });
        migrationBuilder.CreateIndex("IX_communication_messages_Status", "communication_messages", "Status");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
        => migrationBuilder.DropTable(name: "communication_messages");
}
