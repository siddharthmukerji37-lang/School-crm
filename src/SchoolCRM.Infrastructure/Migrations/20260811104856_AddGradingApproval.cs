using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolCRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGradingApproval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "GradingApprovedAt",
                table: "ExamSubmissions",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GradingApprovedBy",
                table: "ExamSubmissions",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "GradingRejectionReason",
                table: "ExamSubmissions",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "GradingStatus",
                table: "ExamSubmissions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GradingApprovedAt",
                table: "ExamSubmissions");

            migrationBuilder.DropColumn(
                name: "GradingApprovedBy",
                table: "ExamSubmissions");

            migrationBuilder.DropColumn(
                name: "GradingRejectionReason",
                table: "ExamSubmissions");

            migrationBuilder.DropColumn(
                name: "GradingStatus",
                table: "ExamSubmissions");
        }
    }
}
