using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolCRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeAccountHeadNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_AccountHeads_AccountHeadId",
                table: "Expenses");

            migrationBuilder.DropForeignKey(
                name: "FK_Incomes_AccountHeads_AccountHeadId",
                table: "Incomes");

            migrationBuilder.AlterColumn<Guid>(
                name: "AccountHeadId",
                table: "Incomes",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AlterColumn<Guid>(
                name: "AccountHeadId",
                table: "Expenses",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_AccountHeads_AccountHeadId",
                table: "Expenses",
                column: "AccountHeadId",
                principalTable: "AccountHeads",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Incomes_AccountHeads_AccountHeadId",
                table: "Incomes",
                column: "AccountHeadId",
                principalTable: "AccountHeads",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_AccountHeads_AccountHeadId",
                table: "Expenses");

            migrationBuilder.DropForeignKey(
                name: "FK_Incomes_AccountHeads_AccountHeadId",
                table: "Incomes");

            migrationBuilder.AlterColumn<Guid>(
                name: "AccountHeadId",
                table: "Incomes",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldNullable: true)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AlterColumn<Guid>(
                name: "AccountHeadId",
                table: "Expenses",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldNullable: true)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_AccountHeads_AccountHeadId",
                table: "Expenses",
                column: "AccountHeadId",
                principalTable: "AccountHeads",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Incomes_AccountHeads_AccountHeadId",
                table: "Incomes",
                column: "AccountHeadId",
                principalTable: "AccountHeads",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
