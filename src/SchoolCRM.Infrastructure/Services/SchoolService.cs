using System.Linq.Expressions;
using SchoolCRM.Application.DTOs.School;
using SchoolCRM.Application.Interfaces.Repositories;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.Domain.Entities.School;
using SchoolCRM.Shared.Constants;
using SchoolCRM.Shared.Models;

namespace SchoolCRM.Infrastructure.Services;

public class SchoolService : ISchoolService
{
    private readonly IUnitOfWork _unitOfWork;

    public SchoolService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<PagedResult<SchoolDto>>> GetSchoolsAsync(PaginationQuery query)
    {
        try
        {
            var (items, totalCount) = await _unitOfWork.Schools.GetPagedAsync(
                query.PageNumber, query.PageSize,
                filter: s => !s.IsDeleted);

            var dtos = items.Select(MapSchoolToDto).ToList();

            var pagedResult = new PagedResult<SchoolDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };

            return ApiResponse<PagedResult<SchoolDto>>.SuccessResponse(pagedResult);
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResult<SchoolDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<SchoolDto>> GetSchoolByIdAsync(Guid id)
    {
        try
        {
            var school = await _unitOfWork.Schools.GetSchoolWithDetailsAsync(id);
            if (school is null)
                return ApiResponse<SchoolDto>.NotFoundResponse(ApplicationMessages.NotFound);

            return ApiResponse<SchoolDto>.SuccessResponse(MapSchoolToDto(school));
        }
        catch (Exception ex)
        {
            return ApiResponse<SchoolDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<SchoolDto>> CreateSchoolAsync(SchoolDto dto)
    {
        try
        {
            var existing = await _unitOfWork.Schools.GetSchoolByCodeAsync(dto.Code ?? string.Empty);
            if (existing is not null)
                return ApiResponse<SchoolDto>.FailResponse(ApplicationMessages.DuplicateRecord);

            var school = new Domain.Entities.School.School
            {
                Name = dto.Name,
                Code = dto.Code ?? string.Empty,
                Email = dto.Email,
                Phone = dto.Phone,
                Website = dto.Website,
                Address = dto.Address,
                City = dto.City,
                State = dto.State,
                Country = dto.Country,
                PostalCode = dto.PostalCode,
                LogoUrl = dto.LogoUrl,
                PrincipalName = dto.PrincipalName,
                EstablishedDate = dto.EstablishedDate,
                RegistrationNumber = dto.RegistrationNumber,
                AffiliationNumber = dto.AffiliationNumber,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Schools.AddAsync(school);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<SchoolDto>.SuccessResponse(MapSchoolToDto(school), ApplicationMessages.CreateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse<SchoolDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<SchoolDto>> UpdateSchoolAsync(Guid id, SchoolDto dto)
    {
        try
        {
            var school = await _unitOfWork.Schools.GetByIdAsync(id);
            if (school is null)
                return ApiResponse<SchoolDto>.NotFoundResponse(ApplicationMessages.NotFound);

            school.Name = dto.Name;
            school.Code = dto.Code ?? school.Code;
            school.Email = dto.Email;
            school.Phone = dto.Phone;
            school.Website = dto.Website;
            school.Address = dto.Address;
            school.City = dto.City;
            school.State = dto.State;
            school.Country = dto.Country;
            school.PostalCode = dto.PostalCode;
            school.LogoUrl = dto.LogoUrl;
            school.PrincipalName = dto.PrincipalName;
            school.EstablishedDate = dto.EstablishedDate;
            school.RegistrationNumber = dto.RegistrationNumber;
            school.AffiliationNumber = dto.AffiliationNumber;
            school.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Schools.UpdateAsync(school);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<SchoolDto>.SuccessResponse(MapSchoolToDto(school), ApplicationMessages.UpdateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse<SchoolDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse> DeleteSchoolAsync(Guid id)
    {
        try
        {
            var school = await _unitOfWork.Schools.GetByIdAsync(id);
            if (school is null)
                return ApiResponse.FailResponse(ApplicationMessages.NotFound);

            school.IsDeleted = true;
            school.DeletedAt = DateTime.UtcNow;
            await _unitOfWork.Schools.UpdateAsync(school);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.SuccessResponse(ApplicationMessages.DeleteSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<AcademicYearDto>>> GetAcademicYearsAsync(Guid schoolId)
    {
        try
        {
            var years = await _unitOfWork.AcademicYears.GetBySchoolIdAsync(schoolId);
            var dtos = years.Select(MapAcademicYearToDto).ToList();
            return ApiResponse<List<AcademicYearDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<AcademicYearDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<AcademicYearDto>> CreateAcademicYearAsync(AcademicYearDto dto)
    {
        try
        {
            var year = new AcademicYear
            {
                Name = dto.Name,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsCurrent = dto.IsActive,
                SchoolId = dto.SchoolId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.AcademicYears.AddAsync(year);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<AcademicYearDto>.SuccessResponse(MapAcademicYearToDto(year), ApplicationMessages.CreateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse<AcademicYearDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<AcademicYearDto>> UpdateAcademicYearAsync(Guid id, AcademicYearDto dto)
    {
        try
        {
            var year = await _unitOfWork.AcademicYears.GetByIdAsync(id);
            if (year is null)
                return ApiResponse<AcademicYearDto>.NotFoundResponse(ApplicationMessages.NotFound);

            year.Name = dto.Name;
            year.StartDate = dto.StartDate;
            year.EndDate = dto.EndDate;
            year.IsCurrent = dto.IsActive;
            year.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.AcademicYears.UpdateAsync(year);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<AcademicYearDto>.SuccessResponse(MapAcademicYearToDto(year), ApplicationMessages.UpdateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse<AcademicYearDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse> SetActiveAcademicYearAsync(Guid schoolId, Guid academicYearId)
    {
        try
        {
            var years = await _unitOfWork.AcademicYears.GetBySchoolIdAsync(schoolId);
            foreach (var year in years)
            {
                year.IsCurrent = year.Id == academicYearId;
                year.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.AcademicYears.UpdateAsync(year);
            }

            var school = await _unitOfWork.Schools.GetByIdAsync(schoolId);
            if (school is not null)
            {
                school.CurrentAcademicYearId = academicYearId;
                school.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.Schools.UpdateAsync(school);
            }

            await _unitOfWork.SaveChangesAsync();
            return ApiResponse.SuccessResponse(ApplicationMessages.UpdateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<ClassRoomDto>>> GetClassRoomsAsync(Guid schoolId, Guid? academicYearId)
    {
        try
        {
            List<ClassRoom> classRooms;
            if (academicYearId.HasValue)
            {
                classRooms = (await _unitOfWork.ClassRooms.GetByAcademicYearAsync(academicYearId.Value))
                    .Where(c => c.SchoolId == schoolId).ToList();
            }
            else
            {
                classRooms = (await _unitOfWork.ClassRooms.GetAllAsync())
                    .Where(c => c.SchoolId == schoolId && !c.IsDeleted).ToList();
            }

            var dtos = classRooms.Select(MapClassRoomToDto).ToList();
            return ApiResponse<List<ClassRoomDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<ClassRoomDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<ClassRoomDto>> CreateClassRoomAsync(ClassRoomDto dto)
    {
        try
        {
            var classRoom = new ClassRoom
            {
                Name = dto.Name,
                Code = dto.Code ?? string.Empty,
                Capacity = 0,
                SchoolId = dto.SchoolId,
                AcademicYearId = dto.AcademicYearId ?? Guid.Empty,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.ClassRooms.AddAsync(classRoom);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<ClassRoomDto>.SuccessResponse(MapClassRoomToDto(classRoom), ApplicationMessages.CreateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse<ClassRoomDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<ClassRoomDto>> UpdateClassRoomAsync(Guid id, ClassRoomDto dto)
    {
        try
        {
            var classRoom = await _unitOfWork.ClassRooms.GetByIdAsync(id);
            if (classRoom is null)
                return ApiResponse<ClassRoomDto>.NotFoundResponse(ApplicationMessages.NotFound);

            classRoom.Name = dto.Name;
            classRoom.Code = dto.Code ?? classRoom.Code;
            classRoom.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.ClassRooms.UpdateAsync(classRoom);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<ClassRoomDto>.SuccessResponse(MapClassRoomToDto(classRoom), ApplicationMessages.UpdateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse<ClassRoomDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse> DeleteClassRoomAsync(Guid id)
    {
        try
        {
            var classRoom = await _unitOfWork.ClassRooms.GetByIdAsync(id);
            if (classRoom is null)
                return ApiResponse.FailResponse(ApplicationMessages.NotFound);

            classRoom.IsDeleted = true;
            classRoom.DeletedAt = DateTime.UtcNow;
            await _unitOfWork.ClassRooms.UpdateAsync(classRoom);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.SuccessResponse(ApplicationMessages.DeleteSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<SectionDto>>> GetSectionsAsync(Guid classRoomId)
    {
        try
        {
            var sections = await _unitOfWork.Sections.GetByClassRoomAsync(classRoomId);
            var dtos = sections.Select(s => new SectionDto
            {
                Id = s.Id,
                ClassRoomId = s.ClassRoomId,
                Name = s.Name,
                Code = s.Code,
                Capacity = s.Capacity,
                SectionTeacherId = s.ClassTeacherId,
                SectionTeacherName = s.ClassTeacher?.User is not null
                    ? $"{s.ClassTeacher.User.FirstName} {s.ClassTeacher.User.LastName}"
                    : null
            }).ToList();

            return ApiResponse<List<SectionDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<SectionDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<SectionDto>> CreateSectionAsync(SectionDto dto)
    {
        try
        {
            var section = new Section
            {
                Name = dto.Name,
                Code = dto.Code ?? string.Empty,
                Capacity = dto.Capacity ?? 0,
                ClassRoomId = dto.ClassRoomId,
                ClassTeacherId = dto.SectionTeacherId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Sections.AddAsync(section);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<SectionDto>.SuccessResponse(new SectionDto
            {
                Id = section.Id,
                ClassRoomId = section.ClassRoomId,
                Name = section.Name,
                Code = section.Code,
                Capacity = section.Capacity
            }, ApplicationMessages.CreateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse<SectionDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<SectionDto>> UpdateSectionAsync(Guid id, SectionDto dto)
    {
        try
        {
            var section = await _unitOfWork.Sections.GetByIdAsync(id);
            if (section is null)
                return ApiResponse<SectionDto>.NotFoundResponse(ApplicationMessages.NotFound);

            section.Name = dto.Name;
            section.Code = dto.Code ?? section.Code;
            section.Capacity = dto.Capacity ?? section.Capacity;
            section.ClassTeacherId = dto.SectionTeacherId;
            section.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Sections.UpdateAsync(section);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<SectionDto>.SuccessResponse(new SectionDto
            {
                Id = section.Id,
                ClassRoomId = section.ClassRoomId,
                Name = section.Name,
                Code = section.Code,
                Capacity = section.Capacity
            }, ApplicationMessages.UpdateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse<SectionDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse> DeleteSectionAsync(Guid id)
    {
        try
        {
            var section = await _unitOfWork.Sections.GetByIdAsync(id);
            if (section is null)
                return ApiResponse.FailResponse(ApplicationMessages.NotFound);

            section.IsDeleted = true;
            section.DeletedAt = DateTime.UtcNow;
            await _unitOfWork.Sections.UpdateAsync(section);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.SuccessResponse(ApplicationMessages.DeleteSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<SubjectDto>>> GetSubjectsAsync(Guid schoolId, Guid? classRoomId)
    {
        try
        {
            List<Domain.Entities.School.Subject> subjects;
            if (classRoomId.HasValue)
            {
                subjects = (await _unitOfWork.Subjects.GetByClassRoomAsync(classRoomId.Value)).ToList();
            }
            else
            {
                subjects = (await _unitOfWork.Subjects.GetAllAsync())
                    .Where(s => !s.IsDeleted).ToList();
            }

            var dtos = subjects.Select(s => new SubjectDto
            {
                Id = s.Id,
                Name = s.Name,
                Code = s.Code,
                Description = s.Description,
                ClassRoomId = s.ClassRoomId,
                MaxMarks = s.TotalMarks,
                PassingMarks = s.PassMarks,
                IsElective = s.IsElective
            }).ToList();

            return ApiResponse<List<SubjectDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<SubjectDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<SubjectDto>> CreateSubjectAsync(SubjectDto dto)
    {
        try
        {
            var subject = new Domain.Entities.School.Subject
            {
                Name = dto.Name,
                Code = dto.Code ?? string.Empty,
                Description = dto.Description,
                ClassRoomId = dto.ClassRoomId ?? Guid.Empty,
                TotalMarks = dto.MaxMarks ?? 100,
                PassMarks = dto.PassingMarks ?? 40,
                IsElective = dto.IsElective,
                SortOrder = dto.SubjectOrder ?? 0,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Subjects.AddAsync(subject);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<SubjectDto>.SuccessResponse(new SubjectDto
            {
                Id = subject.Id,
                Name = subject.Name,
                Code = subject.Code,
                Description = subject.Description,
                ClassRoomId = subject.ClassRoomId,
                MaxMarks = subject.TotalMarks,
                PassingMarks = subject.PassMarks,
                IsElective = subject.IsElective
            }, ApplicationMessages.CreateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse<SubjectDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<SubjectDto>> UpdateSubjectAsync(Guid id, SubjectDto dto)
    {
        try
        {
            var subject = await _unitOfWork.Subjects.GetByIdAsync(id);
            if (subject is null)
                return ApiResponse<SubjectDto>.NotFoundResponse(ApplicationMessages.NotFound);

            subject.Name = dto.Name;
            subject.Code = dto.Code ?? subject.Code;
            subject.Description = dto.Description;
            subject.TotalMarks = dto.MaxMarks ?? subject.TotalMarks;
            subject.PassMarks = dto.PassingMarks ?? subject.PassMarks;
            subject.IsElective = dto.IsElective;
            subject.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Subjects.UpdateAsync(subject);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<SubjectDto>.SuccessResponse(new SubjectDto
            {
                Id = subject.Id,
                Name = subject.Name,
                Code = subject.Code,
                Description = subject.Description,
                ClassRoomId = subject.ClassRoomId,
                MaxMarks = subject.TotalMarks,
                PassingMarks = subject.PassMarks,
                IsElective = subject.IsElective
            }, ApplicationMessages.UpdateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse<SubjectDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse> DeleteSubjectAsync(Guid id)
    {
        try
        {
            var subject = await _unitOfWork.Subjects.GetByIdAsync(id);
            if (subject is null)
                return ApiResponse.FailResponse(ApplicationMessages.NotFound);

            subject.IsDeleted = true;
            subject.DeletedAt = DateTime.UtcNow;
            await _unitOfWork.Subjects.UpdateAsync(subject);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.SuccessResponse(ApplicationMessages.DeleteSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<TimetableDto>>> GetTimetableAsync(Guid sectionId, DateOnly? date)
    {
        try
        {
            var dayOfWeek = date?.DayOfWeek ?? DateTime.UtcNow.DayOfWeek;
            var timetables = await _unitOfWork.Timetables.GetBySectionAndDayAsync(sectionId, dayOfWeek);
            var dtos = timetables.Select(t => new TimetableDto
            {
                Id = t.Id,
                SectionId = t.SectionId,
                SectionName = t.Section?.Name ?? string.Empty,
                SubjectId = t.SubjectId,
                SubjectName = t.Subject?.Name ?? string.Empty,
                TeacherId = t.TeacherId,
                TeacherName = t.Teacher?.User is not null
                    ? $"{t.Teacher.User.FirstName} {t.Teacher.User.LastName}"
                    : null,
                DayOfWeek = t.DayOfWeek,
                StartTime = TimeOnly.FromTimeSpan(t.StartTime),
                EndTime = TimeOnly.FromTimeSpan(t.EndTime),
                Room = t.ClassRoom?.RoomNumber
            }).ToList();

            return ApiResponse<List<TimetableDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<TimetableDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse> SaveTimetableAsync(List<TimetableDto> dtos)
    {
        try
        {
            foreach (var dto in dtos)
            {
                if (dto.Id != Guid.Empty)
                {
                    var existing = await _unitOfWork.Timetables.GetByIdAsync(dto.Id);
                    if (existing is not null)
                    {
                        existing.DayOfWeek = dto.DayOfWeek;
                        existing.StartTime = dto.StartTime.ToTimeSpan();
                        existing.EndTime = dto.EndTime.ToTimeSpan();
                        existing.SubjectId = dto.SubjectId;
                        existing.TeacherId = dto.TeacherId ?? Guid.Empty;
                        existing.UpdatedAt = DateTime.UtcNow;
                        await _unitOfWork.Timetables.UpdateAsync(existing);
                    }
                }
                else
                {
                    var timetable = new Timetable
                    {
                        SectionId = dto.SectionId,
                        SubjectId = dto.SubjectId,
                        TeacherId = dto.TeacherId ?? Guid.Empty,
                        ClassRoomId = Guid.Empty,
                        DayOfWeek = dto.DayOfWeek,
                        PeriodNumber = 0,
                        StartTime = dto.StartTime.ToTimeSpan(),
                        EndTime = dto.EndTime.ToTimeSpan(),
                        CreatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.Timetables.AddAsync(timetable);
                }
            }

            await _unitOfWork.SaveChangesAsync();
            return ApiResponse.SuccessResponse(ApplicationMessages.UpdateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse.FailResponse(ex.Message);
        }
    }

    private static SchoolDto MapSchoolToDto(Domain.Entities.School.School school)
    {
        return new SchoolDto
        {
            Id = school.Id,
            Name = school.Name,
            Code = school.Code,
            Address = school.Address,
            City = school.City,
            State = school.State,
            Country = school.Country,
            PostalCode = school.PostalCode,
            Phone = school.Phone,
            Email = school.Email,
            Website = school.Website,
            LogoUrl = school.LogoUrl,
            EstablishedDate = school.EstablishedDate,
            RegistrationNumber = school.RegistrationNumber,
            AffiliationNumber = school.AffiliationNumber,
            PrincipalName = school.PrincipalName,
            IsActive = !school.IsDeleted
        };
    }

    private static AcademicYearDto MapAcademicYearToDto(AcademicYear year)
    {
        return new AcademicYearDto
        {
            Id = year.Id,
            SchoolId = year.SchoolId,
            Name = year.Name,
            StartYear = year.StartDate.Year,
            EndYear = year.EndDate.Year,
            StartDate = year.StartDate,
            EndDate = year.EndDate,
            IsActive = year.IsCurrent
        };
    }

    private static ClassRoomDto MapClassRoomToDto(ClassRoom classRoom)
    {
        return new ClassRoomDto
        {
            Id = classRoom.Id,
            SchoolId = classRoom.SchoolId,
            Name = classRoom.Name,
            Code = classRoom.Code,
            AcademicYearId = classRoom.AcademicYearId,
            AcademicYearName = classRoom.AcademicYear?.Name,
            TotalSections = classRoom.Sections?.Count ?? 0
        };
    }
}
