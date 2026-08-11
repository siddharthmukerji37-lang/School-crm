using SchoolCRM.Application.DTOs.Exam;
using SchoolCRM.Shared.Models;

namespace SchoolCRM.Application.Interfaces.Services;

public interface IExamService
{
    Task<ApiResponse<PagedResult<ExamDto>>> GetExamsAsync(PaginationQuery query, Guid? classRoomId);

    Task<ApiResponse<ExamDto>> GetExamByIdAsync(Guid id);

    Task<ApiResponse<ExamDto>> CreateExamAsync(CreateExamDto dto);

    Task<ApiResponse<ExamDto>> UpdateExamAsync(Guid id, CreateExamDto dto);

    Task<ApiResponse> DeleteExamAsync(Guid id);

    Task<ApiResponse<List<ExamScheduleDto>>> GetExamScheduleAsync(Guid examId);

    Task<ApiResponse> UpdateExamScheduleAsync(Guid examId, List<ExamScheduleDto> scheduleDtos);

    Task<ApiResponse<List<MarkDto>>> GetMarksAsync(Guid examId, Guid? sectionId, Guid? subjectId);

    Task<ApiResponse> EnterMarksAsync(EnterMarksDto dto);

    Task<ApiResponse<ResultDto>> GetStudentResultAsync(Guid studentId, Guid examId);

    Task<ApiResponse<List<ResultDto>>> GetStudentResultsAsync(Guid studentId);

    Task<ApiResponse<PagedResult<ResultDto>>> GetResultsAsync(
        PaginationQuery query, Guid examId, Guid? classRoomId, Guid? sectionId);

    Task<ApiResponse<ReportCardDto>> GenerateReportCardAsync(Guid studentId, Guid examId);

    Task<ApiResponse<List<ExamQuestionDto>>> GetExamQuestionsAsync(Guid examId);

    Task<ApiResponse> AddExamQuestionsAsync(Guid examId, List<CreateExamQuestionDto> dtos);

    Task<ApiResponse<ExamQuestionDto>> UpdateExamQuestionAsync(Guid examId, Guid questionId, CreateExamQuestionDto dto);

    Task<ApiResponse> DeleteExamQuestionAsync(Guid examId, Guid questionId);

    Task<ApiResponse<ExamDto>> ApproveExamAsync(Guid id, bool approved, string? reason);

    Task<ApiResponse<ExamDto>> UploadQuestionPaperAsync(Guid examId, string? fileUrl, string? fileName);

    Task<ApiResponse<ExamSubmissionDto>> GetSubmissionAsync(Guid examId, Guid studentId);

    Task<ApiResponse<ExamSubmissionDto>> SubmitExamAsync(SubmitExamDto dto);

    Task<ApiResponse<List<ExamSubmissionDto>>> GetSubmissionsByExamAsync(Guid examId);

    Task<ApiResponse<List<ExamSubmissionDto>>> GetMySubmissionsAsync();

    Task<ApiResponse<ExamSubmissionDto>> GradeSubmissionAsync(Guid submissionId, GradeSubmissionDto dto);

    Task<ApiResponse<ExamSubmissionDto>> ApproveSubmissionGradingAsync(Guid submissionId, bool approved, string? reason);
}
