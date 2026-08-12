using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolCRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSectionIdToExam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SectionId",
                table: "Exams",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_Exams_SectionId",
                table: "Exams",
                column: "SectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Exams_Sections_SectionId",
                table: "Exams",
                column: "SectionId",
                principalTable: "Sections",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exams_Sections_SectionId",
                table: "Exams");

            migrationBuilder.DropIndex(
                name: "IX_Exams_SectionId",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "SectionId",
                table: "Exams");
        }
    }
}
