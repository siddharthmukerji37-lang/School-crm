using SchoolCRM.Shared.Models;

namespace SchoolCRM.Application.Interfaces.Services;

public interface IHomeworkService
{
    Task<ApiResponse<PagedResult<HomeworkDto>>> GetHomeworkAsync(
        PaginationQuery query, Guid? classRoomId, Guid? sectionId, Guid? subjectId, DateOnly? fromDate, DateOnly? toDate);

    Task<ApiResponse<HomeworkDto>> GetHomeworkByIdAsync(Guid id);

    Task<ApiResponse<HomeworkDto>> CreateHomeworkAsync(CreateHomeworkDto dto);

    Task<ApiResponse<HomeworkDto>> UpdateHomeworkAsync(Guid id, CreateHomeworkDto dto);

    Task<ApiResponse> DeleteHomeworkAsync(Guid id);

    Task<ApiResponse<PagedResult<AssignmentDto>>> GetAssignmentsAsync(
        PaginationQuery query, Guid? studentId, string? status);

    Task<ApiResponse<AssignmentDto>> GetAssignmentByIdAsync(Guid id);

    Task<ApiResponse<AssignmentDto>> SubmitAssignmentAsync(SubmitAssignmentDto dto);

    Task<ApiResponse<AssignmentDto>> GradeAssignmentAsync(Guid assignmentId, GradeAssignmentDto dto);

    public sealed class HomeworkDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public Guid ClassRoomId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public Guid SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public Guid TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public DateOnly AssignedDate { get; set; }
        public DateOnly DueDate { get; set; }
        public string? AttachmentUrl { get; set; }
        public bool IsActive { get; set; }
    }

    public sealed class CreateHomeworkDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid SubjectId { get; set; }
        public Guid ClassRoomId { get; set; }
        public Guid SectionId { get; set; }
        public DateOnly AssignedDate { get; set; }
        public DateOnly DueDate { get; set; }
        public string? AttachmentUrl { get; set; }
    }

    public sealed class AssignmentDto
    {
        public Guid Id { get; set; }
        public Guid HomeworkId { get; set; }
        public string HomeworkTitle { get; set; } = string.Empty;
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string? SubmissionText { get; set; }
        public string? AttachmentUrl { get; set; }
        public DateTime? SubmittedDate { get; set; }
        public decimal? Marks { get; set; }
        public string? Remarks { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? GradedBy { get; set; }
        public DateTime? GradedDate { get; set; }
    }

    public sealed class SubmitAssignmentDto
    {
        public Guid HomeworkId { get; set; }
        public Guid StudentId { get; set; }
        public string? SubmissionText { get; set; }
        public string? AttachmentUrl { get; set; }
    }

    public sealed class GradeAssignmentDto
    {
        public decimal Marks { get; set; }
        public string? Remarks { get; set; }
    }
}
