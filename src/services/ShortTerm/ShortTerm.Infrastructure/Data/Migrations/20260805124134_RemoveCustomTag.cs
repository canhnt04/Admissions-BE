using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShortTerm.Infrastructure.Data.Migrations
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
                name: "CustomTags");

            migrationBuilder.DropTable(
                name: "UserReplicas");

            migrationBuilder.DropIndex(
                name: "IX_Customers_Assignee",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Courses_CustomTagId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "CustomTagId",
                table: "Courses");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CustomTagId",
                table: "Courses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CustomTags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrainingSystem = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomTags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserReplicas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                name: "IX_Courses_CustomTagId",
                table: "Courses",
                column: "CustomTagId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_CustomTags_CustomTagId",
                table: "Courses",
                column: "CustomTagId",
                principalTable: "CustomTags",
                principalColumn: "Id");

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
