using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Formal.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCustomTag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_CustomTags_CustomTagId",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_UserReplicas_Assignee",
                table: "Customers");

            migrationBuilder.DropTable(
                name: "AssignmentQueues");

            migrationBuilder.DropTable(
                name: "ContactEvidences");

            migrationBuilder.DropTable(
                name: "CustomTags");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "SlaTrackings");

            migrationBuilder.DropTable(
                name: "SystemConfigs");

            migrationBuilder.DropTable(
                name: "UserReplicas");

            migrationBuilder.DropIndex(
                name: "IX_Customers_Assignee",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_AssignmentHistory_Customer_Date",
                table: "CustomerAssignmentHistories");

            migrationBuilder.DropIndex(
                name: "IX_Courses_CustomTagId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "ReassignmentCount",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "CustomerAssignmentHistories");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "CustomerAssignmentHistories");

            migrationBuilder.DropColumn(
                name: "CustomTagId",
                table: "Courses");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Courses",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Courses",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerAssignmentHistories_CustomerId",
                table: "CustomerAssignmentHistories",
                column: "CustomerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CustomerAssignmentHistories_CustomerId",
                table: "CustomerAssignmentHistories");

            migrationBuilder.AddColumn<int>(
                name: "ReassignmentCount",
                table: "Customers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "CustomerAssignmentHistories",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Reason",
                table: "CustomerAssignmentHistories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Courses",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Courses",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<Guid>(
                name: "CustomTagId",
                table: "Courses",
                type: "uniqueidentifier",
                nullable: true);

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
                    TrainingSystem = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentQueues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContactEvidences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConsultantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DurationSeconds = table.Column<int>(type: "int", nullable: true),
                    FileUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NewStatusValue = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OldStatusValue = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactEvidences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactEvidences_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomTags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TrainingSystem = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomTags", x => x.Id);
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

            migrationBuilder.CreateTable(
                name: "SlaTrackings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssigneeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Deadline = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FirstContactAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsContactMade = table.Column<bool>(type: "bit", nullable: false),
                    IsReassigned = table.Column<bool>(type: "bit", nullable: false),
                    IsViolated = table.Column<bool>(type: "bit", nullable: false),
                    ReassignedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReassignedToId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SlaTrackings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SlaTrackings_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SystemConfigs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserReplicas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastSyncedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Mobile = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Role = table.Column<int>(type: "int", nullable: false),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserReplicas", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Assignee",
                table: "Customers",
                column: "Assignee");

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentHistory_Customer_Date",
                table: "CustomerAssignmentHistories",
                columns: new[] { "CustomerId", "AssignmentDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Courses_CustomTagId",
                table: "Courses",
                column: "CustomTagId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentQueue_TrainingSystem_Active",
                table: "AssignmentQueues",
                columns: new[] { "TrainingSystem", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentQueue_TrainingSystem_Consultant_Unique",
                table: "AssignmentQueues",
                columns: new[] { "TrainingSystem", "ConsultantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContactEvidence_Customer_Consultant_Date",
                table: "ContactEvidences",
                columns: new[] { "CustomerId", "ConsultantId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Notification_Recipient_Read_Date",
                table: "Notifications",
                columns: new[] { "RecipientId", "IsRead", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SlaTracking_ContactMade_Deadline_Reassigned",
                table: "SlaTrackings",
                columns: new[] { "IsContactMade", "Deadline", "IsReassigned" });

            migrationBuilder.CreateIndex(
                name: "IX_SlaTracking_Customer_Assignee",
                table: "SlaTrackings",
                columns: new[] { "CustomerId", "AssigneeId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_CustomTags_CustomTagId",
                table: "Courses",
                column: "CustomTagId",
                principalTable: "CustomTags",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_UserReplicas_Assignee",
                table: "Customers",
                column: "Assignee",
                principalTable: "UserReplicas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
