using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeadAssignment.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveContactEvidence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContactEvidences");

            migrationBuilder.DropTable(
                name: "SystemConfigs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContactEvidences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConsultantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DurationSeconds = table.Column<int>(type: "int", nullable: true),
                    FileUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FollowStatus = table.Column<int>(type: "int", nullable: true),
                    LeadStatus = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactEvidences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemConfigs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemConfigs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContactEvidence_Customer_Consultant_Date",
                table: "ContactEvidences",
                columns: new[] { "CustomerId", "ConsultantId", "CreatedAt" });
        }
    }
}
