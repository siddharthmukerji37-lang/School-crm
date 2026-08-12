using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolCRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTeacherIdToBookIssue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "StudentId",
                table: "BookIssues",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "TeacherId",
                table: "BookIssues",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_BookIssues_TeacherId",
                table: "BookIssues",
                column: "TeacherId");

            migrationBuilder.AddForeignKey(
                name: "FK_BookIssues_Teachers_TeacherId",
                table: "BookIssues",
                column: "TeacherId",
                principalTable: "Teachers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookIssues_Teachers_TeacherId",
                table: "BookIssues");

            migrationBuilder.DropIndex(
                name: "IX_BookIssues_TeacherId",
                table: "BookIssues");

            migrationBuilder.DropColumn(
                name: "TeacherId",
                table: "BookIssues");

            migrationBuilder.AlterColumn<Guid>(
                name: "StudentId",
                table: "BookIssues",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldNullable: true)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");
        }
    }
}
