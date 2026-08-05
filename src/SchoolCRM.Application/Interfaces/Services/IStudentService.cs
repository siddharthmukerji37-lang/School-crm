using SchoolCRM.Application.DTOs.Student;
using SchoolCRM.Shared.Models;

namespace SchoolCRM.Application.Interfaces.Services;

public interface IStudentService
{
    Task<ApiResponse<PagedResult<StudentDto>>> GetStudentsAsync(
        PaginationQuery query, Guid? sectionId, Guid? classRoomId, Guid? schoolId, string? status);

    Task<ApiResponse<StudentDto>> GetStudentByIdAsync(Guid id);

    Task<ApiResponse<StudentDto>> CreateStudentAsync(CreateStudentDto dto);

    Task<ApiResponse<StudentDto>> UpdateStudentAsync(Guid id, UpdateStudentDto dto);

    Task<ApiResponse> DeleteStudentAsync(Guid id);

    Task<ApiResponse<StudentDto>> PromoteStudentAsync(Guid id, PromoteStudentDto dto);

    Task<ApiResponse<PagedResult<StudentDto>>> SearchStudentsAsync(string searchTerm, PaginationQuery query);
}
