using AutoMapper;
using SchoolCRM.Application.DTOs.Attendance;
using SchoolCRM.Application.DTOs.Employee;
using SchoolCRM.Application.DTOs.Exam;
using SchoolCRM.Application.DTOs.Fee;
using SchoolCRM.Application.DTOs.Parent;
using SchoolCRM.Application.DTOs.School;
using SchoolCRM.Application.DTOs.Student;
using SchoolCRM.Application.DTOs.Teacher;
using SchoolCRM.Domain.Enums;

namespace SchoolCRM.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        ConfigureStudentMappings();
        ConfigureTeacherMappings();
        ConfigureParentMappings();
        ConfigureEmployeeMappings();
        ConfigureAttendanceMappings();
        ConfigureExamMappings();
        ConfigureFeeMappings();
        ConfigureNotificationMappings();
        ConfigureSchoolMappings();
    }

    private void ConfigureStudentMappings()
    {
        CreateMap<Domain.Entities.Student.Student, StudentDto>()
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.User.PhoneNumber))
            .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.User.Gender.ToString()))
            .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.User.DateOfBirth))
            .ForMember(dest => dest.SectionName, opt => opt.MapFrom(src => src.Section.Name))
            .ForMember(dest => dest.ClassRoomId, opt => opt.MapFrom(src => src.Section.ClassRoomId))
            .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.Section.ClassRoom.Name))
            .ForMember(dest => dest.ParentName, opt => opt.MapFrom(src =>
                src.Parent != null
                    ? $"{src.Parent.User.FirstName} {src.Parent.User.LastName}"
                    : null))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.ProfilePictureUrl, opt => opt.MapFrom(src => src.User.ProfilePictureUrl))
            .ForMember(dest => dest.AcademicYearName, opt => opt.MapFrom(src =>
                src.Section.ClassRoom.AcademicYear != null
                    ? src.Section.ClassRoom.AcademicYear.Name
                    : null))
            .ForMember(dest => dest.BloodGroup, opt => opt.MapFrom(src => src.User.BloodGroup.ToString()));

        CreateMap<CreateStudentDto, Domain.Entities.Student.Student>()
            .ForMember(dest => dest.SectionId, opt => opt.MapFrom(src => src.SectionId))
            .ForMember(dest => dest.ParentId, opt => opt.MapFrom(src => src.ParentId))
            .ForMember(dest => dest.AdmissionDate, opt => opt.MapFrom(src => src.AdmissionDate))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => StudentStatus.Active));

        CreateMap<UpdateStudentDto, Domain.Entities.Student.Student>()
            .ForMember(dest => dest.SectionId, opt => opt.MapFrom(src => src.SectionId))
            .ForMember(dest => dest.ParentId, opt => opt.MapFrom(src => src.ParentId))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<StudentStatus>(src.Status)));
    }

    private void ConfigureTeacherMappings()
    {
        CreateMap<Domain.Entities.Teacher.Teacher, TeacherDto>()
            .ForMember(dest => dest.EmployeeId, opt => opt.MapFrom(src => src.EmployeeCode))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.User.PhoneNumber))
            .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.User.Gender.ToString()))
            .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.User.DateOfBirth))
            .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src =>
                src.Department != null ? src.Department.Name : null))
            .ForMember(dest => dest.Salary, opt => opt.MapFrom(src => src.BasicSalary))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.User.Address))
            .ForMember(dest => dest.BloodGroup, opt => opt.MapFrom(src => src.User.BloodGroup.ToString()))
            .ForMember(dest => dest.ProfilePictureUrl, opt => opt.MapFrom(src => src.User.ProfilePictureUrl))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Experience, opt => opt.MapFrom(src => src.ExperienceYears));

        CreateMap<CreateTeacherDto, Domain.Entities.Teacher.Teacher>()
            .ForMember(dest => dest.DepartmentId, opt => opt.MapFrom(src => src.DepartmentId))
            .ForMember(dest => dest.JoiningDate, opt => opt.MapFrom(src => src.JoiningDate))
            .ForMember(dest => dest.Qualification, opt => opt.MapFrom(src => src.Qualification))
            .ForMember(dest => dest.Specialization, opt => opt.MapFrom(src => src.Specialization))
            .ForMember(dest => dest.ExperienceYears, opt => opt.MapFrom(src => src.Experience ?? 0))
            .ForMember(dest => dest.BasicSalary, opt => opt.MapFrom(src => src.Salary))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => TeacherStatus.Active));

        CreateMap<UpdateTeacherDto, Domain.Entities.Teacher.Teacher>()
            .ForMember(dest => dest.DepartmentId, opt => opt.MapFrom(src => src.DepartmentId))
            .ForMember(dest => dest.Qualification, opt => opt.MapFrom(src => src.Qualification))
            .ForMember(dest => dest.Specialization, opt => opt.MapFrom(src => src.Specialization))
            .ForMember(dest => dest.ExperienceYears, opt => opt.MapFrom(src => src.Experience ?? 0))
            .ForMember(dest => dest.BasicSalary, opt => opt.MapFrom(src => src.Salary))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<TeacherStatus>(src.Status)));
    }

    private void ConfigureParentMappings()
    {
        CreateMap<Domain.Entities.Parent.Parent, ParentDto>()
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.User.PhoneNumber))
            .ForMember(dest => dest.AlternativePhone, opt => opt.MapFrom(src => src.AlternatePhone))
            .ForMember(dest => dest.Occupation, opt => opt.MapFrom(src => src.Occupation))
            .ForMember(dest => dest.Relationship, opt => opt.MapFrom(src => src.Relationship))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.User.Address))
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.User.City))
            .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.User.State))
            .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.User.Country))
            .ForMember(dest => dest.PostalCode, opt => opt.MapFrom(src => src.User.PostalCode))
            .ForMember(dest => dest.ProfilePictureUrl, opt => opt.MapFrom(src => src.User.ProfilePictureUrl))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.User.IsActive))
            .ForMember(dest => dest.Children, opt => opt.MapFrom(src => src.Students));

        CreateMap<Domain.Entities.Student.Student, StudentChildDto>()
            .ForMember(dest => dest.StudentId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src =>
                $"{src.User.FirstName} {src.User.LastName}"))
            .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.Section.ClassRoom.Name))
            .ForMember(dest => dest.SectionName, opt => opt.MapFrom(src => src.Section.Name))
            .ForMember(dest => dest.AdmissionNumber, opt => opt.MapFrom(src => src.AdmissionNumber));

        CreateMap<CreateParentDto, Domain.Entities.Parent.Parent>()
            .ForMember(dest => dest.Occupation, opt => opt.MapFrom(src => src.Occupation))
            .ForMember(dest => dest.Relationship, opt => opt.MapFrom(src => src.Relationship))
            .ForMember(dest => dest.AlternatePhone, opt => opt.MapFrom(src => src.AlternativePhone));

        CreateMap<UpdateParentDto, Domain.Entities.Parent.Parent>()
            .ForMember(dest => dest.Occupation, opt => opt.MapFrom(src => src.Occupation))
            .ForMember(dest => dest.Relationship, opt => opt.MapFrom(src => src.Relationship))
            .ForMember(dest => dest.AlternatePhone, opt => opt.MapFrom(src => src.AlternativePhone));
    }

    private void ConfigureEmployeeMappings()
    {
        CreateMap<Domain.Entities.Employee.Employee, EmployeeDto>()
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.User.PhoneNumber))
            .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.User.Gender.ToString()))
            .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.User.DateOfBirth))
            .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src =>
                src.Department != null ? src.Department.Name : null))
            .ForMember(dest => dest.Designation, opt => opt.MapFrom(src =>
                src.Designation != null ? src.Designation.Name : null))
            .ForMember(dest => dest.Salary, opt => opt.MapFrom(src => src.BasicSalary))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.User.Address))
            .ForMember(dest => dest.BloodGroup, opt => opt.MapFrom(src => src.User.BloodGroup.ToString()))
            .ForMember(dest => dest.ProfilePictureUrl, opt => opt.MapFrom(src => src.User.ProfilePictureUrl))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<CreateEmployeeDto, Domain.Entities.Employee.Employee>()
            .ForMember(dest => dest.DepartmentId, opt => opt.MapFrom(src => src.DepartmentId))
            .ForMember(dest => dest.JoiningDate, opt => opt.MapFrom(src => src.JoiningDate))
            .ForMember(dest => dest.EmploymentType, opt => opt.MapFrom(src => src.EmployeeType))
            .ForMember(dest => dest.BasicSalary, opt => opt.MapFrom(src => src.Salary))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => EmployeeStatus.Active));

        CreateMap<UpdateEmployeeDto, Domain.Entities.Employee.Employee>()
            .ForMember(dest => dest.DepartmentId, opt => opt.MapFrom(src => src.DepartmentId))
            .ForMember(dest => dest.EmploymentType, opt => opt.MapFrom(src => src.EmployeeType))
            .ForMember(dest => dest.BasicSalary, opt => opt.MapFrom(src => src.Salary))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<EmployeeStatus>(src.Status)));
    }

    private void ConfigureAttendanceMappings()
    {
        CreateMap<Domain.Entities.Attendance.Attendance, AttendanceDto>()
            .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src =>
                src.Student != null
                    ? $"{src.Student.User.FirstName} {src.Student.User.LastName}"
                    : null))
            .ForMember(dest => dest.AdmissionNumber, opt => opt.MapFrom(src =>
                src.Student != null ? src.Student.AdmissionNumber : null))
            .ForMember(dest => dest.ClassRoomId, opt => opt.MapFrom(src =>
                src.Student != null ? src.Student.Section.ClassRoomId : Guid.Empty))
            .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src =>
                src.Student != null ? src.Student.Section.ClassRoom.Name : null))
            .ForMember(dest => dest.SectionId, opt => opt.MapFrom(src =>
                src.Student != null ? src.Student.SectionId : Guid.Empty))
            .ForMember(dest => dest.SectionName, opt => opt.MapFrom(src =>
                src.Student != null ? src.Student.Section.Name : null))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.MarkedBy, opt => opt.MapFrom(src => src.Remarks));
    }

    private void ConfigureExamMappings()
    {
        CreateMap<Domain.Entities.Exam.Exam, ExamDto>()
            .ForMember(dest => dest.ExamType, opt => opt.MapFrom(src => src.ExamType.Name))
            .ForMember(dest => dest.AcademicYearName, opt => opt.MapFrom(src => src.AcademicYear.Name))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.Status != ExamStatus.Cancelled))
            .ForMember(dest => dest.IsPublished, opt => opt.MapFrom(src => src.Status == ExamStatus.Completed));

        CreateMap<Domain.Entities.Exam.ExamSchedule, ExamScheduleDto>()
            .ForMember(dest => dest.SubjectName, opt => opt.MapFrom(src => src.Subject.Name))
            .ForMember(dest => dest.MaxMarks, opt => opt.MapFrom(src => src.MaxMarks))
            .ForMember(dest => dest.PassingMarks, opt => opt.MapFrom(src => src.PassMarks))
            .ForMember(dest => dest.Room, opt => opt.MapFrom(src => src.HallName))
            .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => TimeOnly.FromTimeSpan(src.StartTime)))
            .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => TimeOnly.FromTimeSpan(src.EndTime)));

        CreateMap<Domain.Entities.Exam.Mark, MarkDto>()
            .ForMember(dest => dest.ExamName, opt => opt.MapFrom(src => src.ExamSchedule.Exam.Name))
            .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src =>
                $"{src.Student.User.FirstName} {src.Student.User.LastName}"))
            .ForMember(dest => dest.AdmissionNumber, opt => opt.MapFrom(src => src.Student.AdmissionNumber))
            .ForMember(dest => dest.SubjectName, opt => opt.MapFrom(src => src.ExamSchedule.Subject.Name))
            .ForMember(dest => dest.MaxMarks, opt => opt.MapFrom(src => src.ExamSchedule.MaxMarks))
            .ForMember(dest => dest.IsPass, opt => opt.MapFrom(src =>
                !src.IsAbsent && src.MarksObtained >= src.ExamSchedule.PassMarks))
            .ForMember(dest => dest.GradedBy, opt => opt.MapFrom(src => src.EnteredBy))
            .ForMember(dest => dest.GradedDate, opt => opt.MapFrom(src => src.EnteredAt));
    }

    private void ConfigureFeeMappings()
    {
        CreateMap<Domain.Entities.Fee.FeeStructure, FeeStructureDto>()
            .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.ClassRoom.Name))
            .ForMember(dest => dest.AcademicYearName, opt => opt.MapFrom(src => src.AcademicYear.Name))
            .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.Amount))
            .ForMember(dest => dest.FeeType, opt => opt.MapFrom(src => src.FeeHead.Name))
            .ForMember(dest => dest.IsInstallmentApplicable, opt => opt.MapFrom(src => src.Installments.Any()))
            .ForMember(dest => dest.NumberOfInstallments, opt => opt.MapFrom(src =>
                src.Installments.Count))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.Installments, opt => opt.MapFrom(src => src.Installments));

        CreateMap<Domain.Entities.Fee.FeeInstallment, FeeInstallmentDto>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => $"Installment {src.InstallmentNumber}"))
            .ForMember(dest => dest.FeeStructureId, opt => opt.MapFrom(src => src.FeeStructureId))
            .ForMember(dest => dest.FineAmount, opt => opt.MapFrom(src => src.Fine));

        CreateMap<Domain.Entities.Fee.FeeReceipt, FeeReceiptDto>()
            .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src =>
                $"{src.FeeInstallment.Student.User.FirstName} {src.FeeInstallment.Student.User.LastName}"))
            .ForMember(dest => dest.AdmissionNumber, opt => opt.MapFrom(src =>
                src.FeeInstallment.Student.AdmissionNumber))
            .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src =>
                src.FeeInstallment.Student.Section.ClassRoom.Name))
            .ForMember(dest => dest.FeeStructureName, opt => opt.MapFrom(src =>
                src.FeeInstallment.FeeStructure.Name))
            .ForMember(dest => dest.InstallmentId, opt => opt.MapFrom(src => src.FeeInstallmentId))
            .ForMember(dest => dest.InstallmentName, opt => opt.MapFrom(src =>
                $"Installment {src.FeeInstallment.InstallmentNumber}"))
            .ForMember(dest => dest.FineAmount, opt => opt.MapFrom(src => src.Fine))
            .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.PaymentMethod.ToString()))
            .ForMember(dest => dest.PaymentDate, opt => opt.MapFrom(src => src.PaidAt));
    }

    private void ConfigureNotificationMappings()
    {
        CreateMap<Domain.Entities.Notification.Notification, SchoolCRM.Application.DTOs.Notification.NotificationDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => "Normal"))
            .ForMember(dest => dest.SenderName, opt => opt.MapFrom(src => src.User.UserName));
    }

    private void ConfigureSchoolMappings()
    {
        CreateMap<Domain.Entities.School.School, SchoolDto>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

        CreateMap<Domain.Entities.School.AcademicYear, AcademicYearDto>()
            .ForMember(dest => dest.StartYear, opt => opt.MapFrom(src => src.StartDate.Year))
            .ForMember(dest => dest.EndYear, opt => opt.MapFrom(src => src.EndDate.Year))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsCurrent));

        CreateMap<Domain.Entities.School.ClassRoom, ClassRoomDto>()
            .ForMember(dest => dest.AcademicYearName, opt => opt.MapFrom(src => src.AcademicYear.Name))
            .ForMember(dest => dest.TotalSections, opt => opt.MapFrom(src => src.Sections.Count))
            .ForMember(dest => dest.TotalStudents, opt => opt.MapFrom(src =>
                src.Sections.Sum(s => s.Students.Count)))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

        CreateMap<Domain.Entities.School.Section, SectionDto>()
            .ForMember(dest => dest.Capacity, opt => opt.MapFrom(src => (int?)src.Capacity))
            .ForMember(dest => dest.CurrentStrength, opt => opt.MapFrom(src => src.Students.Count))
            .ForMember(dest => dest.SectionTeacherName, opt => opt.MapFrom(src =>
                src.ClassTeacher != null
                    ? $"{src.ClassTeacher.User.FirstName} {src.ClassTeacher.User.LastName}"
                    : null))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

        CreateMap<Domain.Entities.School.Subject, SubjectDto>()
            .ForMember(dest => dest.SchoolId, opt => opt.MapFrom(src => src.ClassRoom.SchoolId))
            .ForMember(dest => dest.MaxMarks, opt => opt.MapFrom(src => (decimal?)src.TotalMarks))
            .ForMember(dest => dest.PassingMarks, opt => opt.MapFrom(src => (decimal?)src.PassMarks))
            .ForMember(dest => dest.SubjectOrder, opt => opt.MapFrom(src => (int?)src.SortOrder))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

        CreateMap<Domain.Entities.School.Timetable, TimetableDto>()
            .ForMember(dest => dest.SectionName, opt => opt.MapFrom(src => src.Section.Name))
            .ForMember(dest => dest.SubjectName, opt => opt.MapFrom(src => src.Subject.Name))
            .ForMember(dest => dest.TeacherName, opt => opt.MapFrom(src =>
                $"{src.Teacher.User.FirstName} {src.Teacher.User.LastName}"))
            .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => TimeOnly.FromTimeSpan(src.StartTime)))
            .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => TimeOnly.FromTimeSpan(src.EndTime)))
            .ForMember(dest => dest.AcademicYearId, opt => opt.MapFrom(src => src.ClassRoom.AcademicYearId));

        CreateMap<Domain.Entities.School.Department, DepartmentDto>()
            .ForMember(dest => dest.HeadId, opt => opt.MapFrom(src => src.HeadOfDepartmentId))
            .ForMember(dest => dest.TotalTeachers, opt => opt.MapFrom(src => src.Teachers.Count))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

        CreateMap<Domain.Entities.Notification.Announcement, AnnouncementDto>()
            .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Content))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => "Normal"))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.PublishDate ?? src.CreatedAt))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.ExpiryDate))
            .ForMember(dest => dest.TargetRole, opt => opt.MapFrom(src => src.TargetAudience))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedByName ?? string.Empty));

        CreateMap<Domain.Entities.School.SchoolEvent, EventDto>()
            .ForMember(dest => dest.EventType, opt => opt.MapFrom(src => src.EventType ?? "General"))
            .ForMember(dest => dest.IsHoliday, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));
    }
}
