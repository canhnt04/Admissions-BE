using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeadAssignment.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAssignmentQueueAndNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssignmentQueues");

            migrationBuilder.DropTable(
                name: "Notifications");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssignmentQueues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConsultantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentLoad = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastAssignedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MaxLoad = table.Column<int>(type: "int", nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    TrainingSystem = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentQueues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RecipientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentQueue_TrainingSystem_Active",
                table: "AssignmentQueues",
                columns: new[] { "TrainingSystem", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentQueue_TrainingSystem_Consultant_Unique",
                table: "AssignmentQueues",
                columns: new[] { "TrainingSystem", "ConsultantId" },
                unique: true,
                filter: "[TrainingSystem] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_Recipient_Read_Date",
                table: "Notifications",
                columns: new[] { "RecipientId", "IsRead", "CreatedAt" });
        }
    }
}
