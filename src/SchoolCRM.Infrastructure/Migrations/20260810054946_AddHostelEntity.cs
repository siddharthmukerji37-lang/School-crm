using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolCRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHostelEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "HostelId",
                table: "HostelRooms",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "Hostels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Address = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WardenName = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WardenPhone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SchoolId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeletedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hostels", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql(@"
INSERT INTO `Hostels` (`Id`, `Name`, `Type`, `Address`, `WardenName`, `WardenPhone`, `IsActive`, `SchoolId`, `CreatedBy`, `CreatedAt`, `IsDeleted`)
VALUES (UUID(), 'Main Hostel', 'Co-ed', '', '', '', 1, '00000000-0000-0000-0000-000000000000', 'System', UTC_TIMESTAMP(6), 0);

UPDATE `HostelRooms`
SET `HostelId` = (SELECT `Id` FROM `Hostels` WHERE `Name` = 'Main Hostel' LIMIT 1)
WHERE `HostelId` IS NULL;
");

            migrationBuilder.CreateIndex(
                name: "IX_HostelRooms_HostelId",
                table: "HostelRooms",
                column: "HostelId");

            migrationBuilder.AddForeignKey(
                name: "FK_HostelRooms_Hostels_HostelId",
                table: "HostelRooms",
                column: "HostelId",
                principalTable: "Hostels",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HostelRooms_Hostels_HostelId",
                table: "HostelRooms");

            migrationBuilder.DropTable(
                name: "Hostels");

            migrationBuilder.DropIndex(
                name: "IX_HostelRooms_HostelId",
                table: "HostelRooms");

            migrationBuilder.DropColumn(
                name: "HostelId",
                table: "HostelRooms");
        }
    }
}
