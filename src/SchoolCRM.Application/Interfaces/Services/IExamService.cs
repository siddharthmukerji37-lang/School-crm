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

    Task<ApiResponse<PagedResult<ResultDto>>> GetResultsAsync(
        PaginationQuery query, Guid examId, Guid? classRoomId, Guid? sectionId);

    Task<ApiResponse<ReportCardDto>> GenerateReportCardAsync(Guid studentId, Guid examId);
}
