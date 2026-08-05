using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeadAssignment.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class MakeTrainingSystemNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserReplicas");

            migrationBuilder.DropIndex(
                name: "IX_AssignmentQueue_TrainingSystem_Consultant_Unique",
                table: "AssignmentQueues");

            migrationBuilder.DropColumn(
                name: "NewStatusValue",
                table: "ContactEvidences");

            migrationBuilder.DropColumn(
                name: "OldStatusValue",
                table: "ContactEvidences");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "ContactEvidences");

            migrationBuilder.AlterColumn<int>(
                name: "TrainingSystem",
                table: "CustomerCareStatuses",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "FollowStatus",
                table: "ContactEvidences",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LeadStatus",
                table: "ContactEvidences",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TrainingSystem",
                table: "AssignmentQueues",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentQueue_TrainingSystem_Consultant_Unique",
                table: "AssignmentQueues",
                columns: new[] { "TrainingSystem", "ConsultantId" },
                unique: true,
                filter: "[TrainingSystem] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AssignmentQueue_TrainingSystem_Consultant_Unique",
                table: "AssignmentQueues");

            migrationBuilder.DropColumn(
                name: "FollowStatus",
                table: "ContactEvidences");

            migrationBuilder.DropColumn(
                name: "LeadStatus",
                table: "ContactEvidences");

            migrationBuilder.AlterColumn<int>(
                name: "TrainingSystem",
                table: "CustomerCareStatuses",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NewStatusValue",
                table: "ContactEvidences",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OldStatusValue",
                table: "ContactEvidences",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "ContactEvidences",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "TrainingSystem",
                table: "AssignmentQueues",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "UserReplicas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                name: "IX_AssignmentQueue_TrainingSystem_Consultant_Unique",
                table: "AssignmentQueues",
                columns: new[] { "TrainingSystem", "ConsultantId" },
                unique: true);
        }
    }
}
