using SchoolCRM.Application.DTOs.School;
using SchoolCRM.Shared.Models;

namespace SchoolCRM.Application.Interfaces.Services;

public interface ISchoolService
{
    Task<ApiResponse<PagedResult<SchoolDto>>> GetSchoolsAsync(PaginationQuery query);

    Task<ApiResponse<SchoolDto>> GetSchoolByIdAsync(Guid id);

    Task<ApiResponse<SchoolDto>> CreateSchoolAsync(SchoolDto dto);

    Task<ApiResponse<SchoolDto>> UpdateSchoolAsync(Guid id, SchoolDto dto);

    Task<ApiResponse> DeleteSchoolAsync(Guid id);

    Task<ApiResponse<List<AcademicYearDto>>> GetAcademicYearsAsync(Guid schoolId);

    Task<ApiResponse<AcademicYearDto>> CreateAcademicYearAsync(AcademicYearDto dto);

    Task<ApiResponse<AcademicYearDto>> UpdateAcademicYearAsync(Guid id, AcademicYearDto dto);

    Task<ApiResponse> SetActiveAcademicYearAsync(Guid schoolId, Guid academicYearId);

    Task<ApiResponse<List<ClassRoomDto>>> GetClassRoomsAsync(Guid schoolId, Guid? academicYearId);

    Task<ApiResponse<ClassRoomDto>> CreateClassRoomAsync(ClassRoomDto dto);

    Task<ApiResponse<ClassRoomDto>> UpdateClassRoomAsync(Guid id, ClassRoomDto dto);

    Task<ApiResponse> DeleteClassRoomAsync(Guid id);

    Task<ApiResponse<List<SectionDto>>> GetSectionsAsync(Guid classRoomId);

    Task<ApiResponse<SectionDto>> CreateSectionAsync(SectionDto dto);

    Task<ApiResponse<SectionDto>> UpdateSectionAsync(Guid id, SectionDto dto);

    Task<ApiResponse> DeleteSectionAsync(Guid id);

    Task<ApiResponse<List<SubjectDto>>> GetSubjectsAsync(Guid schoolId, Guid? classRoomId);

    Task<ApiResponse<List<DepartmentDto>>> GetDepartmentsAsync(Guid schoolId);

    Task<ApiResponse<SubjectDto>> CreateSubjectAsync(SubjectDto dto);

    Task<ApiResponse<SubjectDto>> UpdateSubjectAsync(Guid id, SubjectDto dto);

    Task<ApiResponse> DeleteSubjectAsync(Guid id);

    Task<ApiResponse<List<TimetableDto>>> GetTimetableAsync(Guid sectionId, DateOnly? date);

    Task<ApiResponse<List<TimetableDto>>> GetMyTeacherTimetableAsync();

    Task<ApiResponse<List<TimetableDto>>> GetMySectionTimetableAsync();

    Task<ApiResponse> SaveTimetableAsync(List<TimetableDto> dtos);
}
