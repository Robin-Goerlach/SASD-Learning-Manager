using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SASD.Bewerbungsmanager.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AssistantWorkspace : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "assistant_sessions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                OpportunityId = table.Column<Guid>(type: "TEXT", nullable: true),
                ApplicationId = table.Column<Guid>(type: "TEXT", nullable: true),
                TaskKind = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                Title = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                ContextSha256 = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                PromptText = table.Column<string>(type: "TEXT", maxLength: 250000, nullable: false),
                ResponseText = table.Column<string>(type: "TEXT", maxLength: 250000, nullable: true),
                ProviderLabel = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                AdditionalInstructions = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                CompletedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_assistant_sessions", x => x.Id);
                table.ForeignKey(
                    name: "FK_assistant_sessions_applications_ApplicationId",
                    column: x => x.ApplicationId,
                    principalTable: "applications",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_assistant_sessions_opportunities_OpportunityId",
                    column: x => x.OpportunityId,
                    principalTable: "opportunities",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateIndex(
            name: "IX_assistant_sessions_ApplicationId",
            table: "assistant_sessions",
            column: "ApplicationId");
        migrationBuilder.CreateIndex(
            name: "IX_assistant_sessions_CreatedAtUtc",
            table: "assistant_sessions",
            column: "CreatedAtUtc");
        migrationBuilder.CreateIndex(
            name: "IX_assistant_sessions_OpportunityId",
            table: "assistant_sessions",
            column: "OpportunityId");
        migrationBuilder.CreateIndex(
            name: "IX_assistant_sessions_Status",
            table: "assistant_sessions",
            column: "Status");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
        => migrationBuilder.DropTable(name: "assistant_sessions");
}
