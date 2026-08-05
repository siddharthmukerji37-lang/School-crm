using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolCRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClassRoomIdToExam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClassRoomId",
                table: "Exams",
                type: "char(36)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Exams_ClassRoomId",
                table: "Exams",
                column: "ClassRoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_Exams_ClassRooms_ClassRoomId",
                table: "Exams",
                column: "ClassRoomId",
                principalTable: "ClassRooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exams_ClassRooms_ClassRoomId",
                table: "Exams");

            migrationBuilder.DropIndex(
                name: "IX_Exams_ClassRoomId",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "ClassRoomId",
                table: "Exams");
        }
    }
}
