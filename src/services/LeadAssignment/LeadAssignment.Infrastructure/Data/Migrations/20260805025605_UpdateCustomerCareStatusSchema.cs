using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeadAssignment.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCustomerCareStatusSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignedAt",
                table: "CustomerCareStatuses");

            migrationBuilder.DropColumn(
                name: "Deadline",
                table: "CustomerCareStatuses");

            migrationBuilder.DropColumn(
                name: "IsContactMade",
                table: "CustomerCareStatuses");

            migrationBuilder.DropColumn(
                name: "IsReassigned",
                table: "CustomerCareStatuses");

            migrationBuilder.DropColumn(
                name: "IsViolated",
                table: "CustomerCareStatuses");

            migrationBuilder.DropColumn(
                name: "ReassignedToId",
                table: "CustomerCareStatuses");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "CustomerAssignmentHistories");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "CustomerAssignmentHistories");

            migrationBuilder.RenameColumn(
                name: "ReassignedAt",
                table: "CustomerCareStatuses",
                newName: "StatusDate");

            migrationBuilder.RenameColumn(
                name: "FirstContactAt",
                table: "CustomerCareStatuses",
                newName: "ReportDate");

            migrationBuilder.AlterColumn<Guid>(
                name: "AssigneeId",
                table: "CustomerCareStatuses",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<int>(
                name: "FollowStatus",
                table: "CustomerCareStatuses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "CustomerCareStatuses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "CustomerCareStatuses",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FollowStatus",
                table: "CustomerCareStatuses");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "CustomerCareStatuses");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "CustomerCareStatuses");

            migrationBuilder.RenameColumn(
                name: "StatusDate",
                table: "CustomerCareStatuses",
                newName: "ReassignedAt");

            migrationBuilder.RenameColumn(
                name: "ReportDate",
                table: "CustomerCareStatuses",
                newName: "FirstContactAt");

            migrationBuilder.AlterColumn<Guid>(
                name: "AssigneeId",
                table: "CustomerCareStatuses",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AssignedAt",
                table: "CustomerCareStatuses",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "Deadline",
                table: "CustomerCareStatuses",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsContactMade",
                table: "CustomerCareStatuses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsReassigned",
                table: "CustomerCareStatuses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsViolated",
                table: "CustomerCareStatuses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "ReassignedToId",
                table: "CustomerCareStatuses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "CustomerAssignmentHistories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Reason",
                table: "CustomerAssignmentHistories",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
