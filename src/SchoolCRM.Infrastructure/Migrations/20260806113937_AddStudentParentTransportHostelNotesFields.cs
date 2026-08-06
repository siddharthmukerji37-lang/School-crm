using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolCRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentParentTransportHostelNotesFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HostelRequired",
                table: "Students",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Students",
                type: "text",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ParentEmail",
                table: "Students",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ParentName",
                table: "Students",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ParentPhone",
                table: "Students",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "TransportRequired",
                table: "Students",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HostelRequired",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "ParentEmail",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "ParentName",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "ParentPhone",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "TransportRequired",
                table: "Students");
        }
    }
}
