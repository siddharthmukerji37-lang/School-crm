using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolCRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSectionToHomework : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SectionId",
                table: "Homeworks",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_Homeworks_SectionId",
                table: "Homeworks",
                column: "SectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Homeworks_Sections_SectionId",
                table: "Homeworks",
                column: "SectionId",
                principalTable: "Sections",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Homeworks_Sections_SectionId",
                table: "Homeworks");

            migrationBuilder.DropIndex(
                name: "IX_Homeworks_SectionId",
                table: "Homeworks");

            migrationBuilder.DropColumn(
                name: "SectionId",
                table: "Homeworks");
        }
    }
}
