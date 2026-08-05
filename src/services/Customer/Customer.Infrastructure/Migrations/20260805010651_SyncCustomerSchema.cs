using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Customer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncCustomerSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Mobile",
                table: "Customers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Gender",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Customers",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<Guid>(
                name: "Assignee",
                table: "Customers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CCCD",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CCCDIssueDate",
                table: "Customers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Enrollment",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EquivalentDegree",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ethnic",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FatherName",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FinalStatus",
                table: "Customers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GraduationYear",
                table: "Customers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LatestSchool",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotherName",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OnlineMessageMobile",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParentMobile",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlaceOfBirth",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SaleStatus",
                table: "Customers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SchoolAddress",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StudentId",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmissionDate",
                table: "Customers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateTime",
                table: "Customers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserIdByOa",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Assignee",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CCCD",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CCCDIssueDate",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Enrollment",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "EquivalentDegree",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Ethnic",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "FatherName",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "FinalStatus",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "GraduationYear",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "LatestSchool",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "MotherName",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "OnlineMessageMobile",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "ParentMobile",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "PlaceOfBirth",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "SaleStatus",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "SchoolAddress",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "SubmissionDate",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "UpdateTime",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "UserIdByOa",
                table: "Customers");

            migrationBuilder.AlterColumn<string>(
                name: "Mobile",
                table: "Customers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Gender",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Customers",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
