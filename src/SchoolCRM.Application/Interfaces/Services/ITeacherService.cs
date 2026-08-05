using SchoolCRM.Application.DTOs.Teacher;
using SchoolCRM.Shared.Models;

namespace SchoolCRM.Application.Interfaces.Services;

public interface ITeacherService
{
    Task<ApiResponse<PagedResult<TeacherDto>>> GetTeachersAsync(
        PaginationQuery query, Guid? departmentId, string? status);

    Task<ApiResponse<TeacherDto>> GetTeacherByIdAsync(Guid id);

    Task<ApiResponse<TeacherDto>> CreateTeacherAsync(CreateTeacherDto dto);

    Task<ApiResponse<TeacherDto>> UpdateTeacherAsync(Guid id, UpdateTeacherDto dto);

    Task<ApiResponse> DeleteTeacherAsync(Guid id);
}
