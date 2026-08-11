using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolCRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExamQuestionsAndApprovals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApprovalStatus",
                table: "Homeworks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "Homeworks",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedBy",
                table: "Homeworks",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Homeworks",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "ApprovalStatus",
                table: "Exams",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "Exams",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedBy",
                table: "Exams",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "QuestionPaperFileName",
                table: "Exams",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "QuestionPaperUrl",
                table: "Exams",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Exams",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "TeacherId",
                table: "Exams",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "ExamQuestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    QuestionText = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    QuestionType = table.Column<int>(type: "int", nullable: false),
                    OptionA = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OptionB = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OptionC = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OptionD = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CorrectAnswer = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Marks = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    ImageUrl = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ImageFileName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    ExamId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    SubjectId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
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
                    table.PrimaryKey("PK_ExamQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamQuestions_Exams_ExamId",
                        column: x => x.ExamId,
                        principalTable: "Exams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamQuestions_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ExamSubmissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    SubmittedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TotalMarksObtained = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    TotalMaxMarks = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    IsGraded = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    GradedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GradedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ExamId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    StudentId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
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
                    table.PrimaryKey("PK_ExamSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamSubmissions_Exams_ExamId",
                        column: x => x.ExamId,
                        principalTable: "Exams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamSubmissions_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ExamAnswers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    SelectedOption = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AnswerText = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ImageUrl = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsCorrect = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MarksObtained = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Remarks = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExamSubmissionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ExamQuestionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
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
                    table.PrimaryKey("PK_ExamAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamAnswers_ExamQuestions_ExamQuestionId",
                        column: x => x.ExamQuestionId,
                        principalTable: "ExamQuestions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ExamAnswers_ExamSubmissions_ExamSubmissionId",
                        column: x => x.ExamSubmissionId,
                        principalTable: "ExamSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Exams_TeacherId",
                table: "Exams",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamAnswers_ExamQuestionId",
                table: "ExamAnswers",
                column: "ExamQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamAnswers_ExamSubmissionId",
                table: "ExamAnswers",
                column: "ExamSubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamQuestions_ExamId",
                table: "ExamQuestions",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamQuestions_SubjectId",
                table: "ExamQuestions",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSubmissions_ExamId_StudentId",
                table: "ExamSubmissions",
                columns: new[] { "ExamId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExamSubmissions_StudentId",
                table: "ExamSubmissions",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Exams_Teachers_TeacherId",
                table: "Exams",
                column: "TeacherId",
                principalTable: "Teachers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exams_Teachers_TeacherId",
                table: "Exams");

            migrationBuilder.DropTable(
                name: "ExamAnswers");

            migrationBuilder.DropTable(
                name: "ExamQuestions");

            migrationBuilder.DropTable(
                name: "ExamSubmissions");

            migrationBuilder.DropIndex(
                name: "IX_Exams_TeacherId",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "Homeworks");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "Homeworks");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "Homeworks");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Homeworks");

            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "QuestionPaperFileName",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "QuestionPaperUrl",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "TeacherId",
                table: "Exams");
        }
    }
}
