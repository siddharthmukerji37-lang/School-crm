using SchoolCRM.Application.DTOs.Dashboard;
using SchoolCRM.Application.Interfaces.Repositories;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.Domain.Enums;
using SchoolCRM.Shared.Models;

using DashboardStudentDto = SchoolCRM.Application.DTOs.Dashboard.StudentDto;
using DashboardAnnouncementDto = SchoolCRM.Application.DTOs.Dashboard.AnnouncementDto;
using DashboardExamDto = SchoolCRM.Application.DTOs.Dashboard.ExamDto;

namespace SchoolCRM.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;

    public DashboardService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<DashboardDto>> GetDashboardStatsAsync(Guid schoolId, string userRole)
    {
        try
        {
            schoolId = await ResolveSchoolIdAsync(schoolId);

            var totalStudents = await _unitOfWork.Students.CountAsync(s => !s.IsDeleted && s.SchoolId == schoolId);
            var totalTeachers = await _unitOfWork.Teachers.CountAsync(t => !t.IsDeleted && t.SchoolId == schoolId);
            var totalStaff = await _unitOfWork.Employees.CountAsync(e => !e.IsDeleted && e.SchoolId == schoolId);
            var totalClasses = await _unitOfWork.ClassRooms.CountAsync(c => !c.IsDeleted && c.SchoolId == schoolId);

            var today = DateTime.UtcNow.Date;
            var monthlyStart = new DateTime(today.Year, today.Month, 1);
            var todayAttendance = await _unitOfWork.Attendances.GetByDateAsync(today, schoolId);
            var studentAttendance = todayAttendance.Where(a => a.StudentId.HasValue).ToList();
            var presentCount = studentAttendance.Count(a => a.Status == AttendanceStatus.Present);
            var absentCount = studentAttendance.Count(a => a.Status == AttendanceStatus.Absent);
            var lateCount = studentAttendance.Count(a => a.Status == AttendanceStatus.Late);
            var totalAttendance = studentAttendance.Count;

            var teacherAttendance = todayAttendance.Where(a => a.TeacherId.HasValue).ToList();
            var employeeAttendance = todayAttendance.Where(a => a.EmployeeId.HasValue).ToList();

            var totalActiveTeachers = await _unitOfWork.Teachers.CountAsync(t =>
                !t.IsDeleted && t.SchoolId == schoolId && t.Status == TeacherStatus.Active);
            var totalActiveEmployees = await _unitOfWork.Employees.CountAsync(e =>
                !e.IsDeleted && e.SchoolId == schoolId && e.Status == EmployeeStatus.Active);

            var receipts = (await _unitOfWork.FeeReceipts.GetAllAsync())
                .Where(r => !r.IsDeleted).ToList();
            var totalCollected = receipts.Sum(r => r.TotalPaid);
            var todayCollected = receipts.Where(r => r.PaidAt.Date == today).Sum(r => r.TotalPaid);
            var monthlyCollected = receipts.Where(r => r.PaidAt >= monthlyStart).Sum(r => r.TotalPaid);

            var studentIds = (await _unitOfWork.Students.GetBySchoolAsync(schoolId))
                .Select(s => s.Id).ToHashSet();
            var installments = (await _unitOfWork.FeeInstallments.GetAllAsync())
                .Where(i => !i.IsDeleted && studentIds.Contains(i.StudentId)).ToList();
            var totalPending = installments
                .Where(i => i.PaidAmount < i.Amount)
                .Sum(i => Math.Max(0, i.Amount - i.PaidAmount - i.Discount - i.Scholarship));
            var overdueFees = installments
                .Where(i => i.PaidAmount < i.Amount && i.DueDate.Date < today)
                .Sum(i => Math.Max(0, i.Amount - i.PaidAmount - i.Discount - i.Scholarship));

            var pendingFeeStudents = await BuildPendingFeeStudentsAsync(installments, today);
            var examResults = await BuildExamResultsAsync(schoolId);

            var todayBirthdays = await GetTodayBirthdaysAsync(schoolId, today);

            var recentAdmissions = (await _unitOfWork.Students.GetPagedStudentsAsync(
                1, 5, null, null, null, null, null, schoolId, null)).Items
                .Select(s => new DashboardStudentDto
                {
                    Id = s.Id,
                    Name = $"{s.User?.FirstName} {s.User?.LastName}",
                    AdmissionNumber = s.AdmissionNumber,
                    ClassName = s.Section?.ClassRoom?.Name ?? string.Empty,
                    SectionName = s.Section?.Name ?? string.Empty,
                    AdmissionDate = s.AdmissionDate,
                    ProfilePictureUrl = s.User?.ProfilePictureUrl
                }).ToList();

            var announcements = await _unitOfWork.Announcements.GetPublishedAsync(schoolId);
            var latestAnnouncements = announcements.Take(5).Select(a => new DashboardAnnouncementDto
            {
                Id = a.Id,
                Title = a.Title,
                Message = a.Content,
                Priority = "Normal",
                CreatedAt = a.CreatedAt,
                CreatedBy = a.CreatedByName ?? string.Empty
            }).ToList();

            var exams = await _unitOfWork.Exams.GetPagedAsync(
                1, 5, e => !e.IsDeleted && e.SchoolId == schoolId && e.StartDate > DateTime.UtcNow);
            var upcomingExamsList = exams.Items.Select(e => new DashboardExamDto
            {
                Id = e.Id,
                Name = e.Name,
                ExamType = e.ExamType?.Name ?? string.Empty,
                StartDate = e.StartDate,
                EndDate = e.EndDate
            }).ToList();

            var dashboard = new DashboardDto
            {
                TotalStudents = totalStudents,
                TotalTeachers = totalTeachers,
                TotalStaff = totalStaff,
                TotalClasses = totalClasses,
                TodayAttendance = new AttendanceOverviewDto
                {
                    Total = totalAttendance,
                    Present = presentCount,
                    Absent = absentCount,
                    Late = lateCount,
                    AttendancePercentage = totalAttendance > 0
                        ? Math.Round((decimal)presentCount / totalAttendance * 100, 2)
                        : 0
                },
                StaffAttendance = new StaffAttendanceOverviewDto
                {
                    TotalTeachers = totalActiveTeachers,
                    TeachersMarked = teacherAttendance.Count,
                    TeachersPresent = teacherAttendance.Count(a => a.Status == AttendanceStatus.Present),
                    TeachersAbsent = teacherAttendance.Count(a => a.Status != AttendanceStatus.Present),
                    TotalEmployees = totalActiveEmployees,
                    EmployeesMarked = employeeAttendance.Count,
                    EmployeesPresent = employeeAttendance.Count(a => a.Status == AttendanceStatus.Present),
                    EmployeesAbsent = employeeAttendance.Count(a => a.Status != AttendanceStatus.Present)
                },
                FeesCollected = new FeeOverviewDto
                {
                    TotalCollected = totalCollected,
                    TodayCollected = todayCollected,
                    MonthlyCollected = monthlyCollected,
                    TotalPending = totalPending,
                    OverdueFees = overdueFees
                },
                PendingFees = totalPending,
                UpcomingExams = upcomingExamsList.Count,
                TodayBirthdays = todayBirthdays,
                LatestAnnouncements = latestAnnouncements,
                RecentAdmissions = recentAdmissions,
                UpcomingExamsList = upcomingExamsList,
                PendingFeeStudents = pendingFeeStudents,
                ExamResults = examResults
            };

            return ApiResponse<DashboardDto>.SuccessResponse(dashboard);
        }
        catch (Exception ex)
        {
            return ApiResponse<DashboardDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<ChartDataDto>>> GetAttendanceChartDataAsync(Guid schoolId, int months)
    {
        try
        {
            schoolId = await ResolveSchoolIdAsync(schoolId);
            var chartData = new List<ChartDataDto>();
            var today = DateTime.UtcNow.Date;

            for (int i = months - 1; i >= 0; i--)
            {
                var month = today.AddMonths(-i);
                var startOfMonth = new DateTime(month.Year, month.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                var attendances = new List<Domain.Entities.Attendance.Attendance>();
                for (var date = startOfMonth; date <= endOfMonth && date <= today; date = date.AddDays(1))
                {
                    var dayAttendances = await _unitOfWork.Attendances.GetByDateAsync(date, schoolId);
                    attendances.AddRange(dayAttendances);
                }

                var totalDays = attendances.Count;
                var presentDays = attendances.Count(a => a.Status == AttendanceStatus.Present);

                chartData.Add(new ChartDataDto
                {
                    Label = startOfMonth.ToString("MMM yyyy"),
                    Value = totalDays > 0 ? Math.Round((decimal)presentDays / totalDays * 100, 2) : 0,
                    Date = startOfMonth
                });
            }

            return ApiResponse<List<ChartDataDto>>.SuccessResponse(chartData);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<ChartDataDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<ChartDataDto>>> GetFeeChartDataAsync(Guid schoolId, int months)
    {
        try
        {
            schoolId = await ResolveSchoolIdAsync(schoolId);
            var chartData = new List<ChartDataDto>();
            var today = DateTime.UtcNow.Date;
            var receipts = await _unitOfWork.FeeReceipts.GetAllAsync();

            for (int i = months - 1; i >= 0; i--)
            {
                var month = today.AddMonths(-i);
                var startOfMonth = new DateTime(month.Year, month.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                var monthlyTotal = receipts
                    .Where(r => !r.IsDeleted && r.PaidAt >= startOfMonth && r.PaidAt <= endOfMonth)
                    .Sum(r => r.TotalPaid);

                chartData.Add(new ChartDataDto
                {
                    Label = startOfMonth.ToString("MMM yyyy"),
                    Value = monthlyTotal,
                    Date = startOfMonth
                });
            }

            return ApiResponse<List<ChartDataDto>>.SuccessResponse(chartData);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<ChartDataDto>>.FailResponse(ex.Message);
        }
    }

    private async Task<List<PendingFeeStudentDto>> BuildPendingFeeStudentsAsync(
        List<Domain.Entities.Fee.FeeInstallment> installments, DateTime today)
    {
        var result = new List<PendingFeeStudentDto>();

        var groups = installments
            .Where(i => i.PaidAmount < i.Amount)
            .GroupBy(i => i.StudentId)
            .Select(g => new
            {
                StudentId = g.Key,
                PendingAmount = g.Sum(i => Math.Max(0, i.Amount - i.PaidAmount - i.Discount - i.Scholarship)),
                IsOverdue = g.Any(i => i.PaidAmount < i.Amount && i.DueDate.Date < today)
            })
            .Where(x => x.PendingAmount > 0)
            .OrderByDescending(x => x.PendingAmount)
            .Take(10)
            .ToList();

        foreach (var group in groups)
        {
            var student = await _unitOfWork.Students.GetStudentWithDetailsAsync(group.StudentId);
            if (student is null)
                continue;

            result.Add(new PendingFeeStudentDto
            {
                StudentId = student.Id,
                StudentName = $"{student.User.FirstName} {student.User.LastName}".Trim(),
                AdmissionNumber = student.AdmissionNumber,
                ClassName = student.Section?.ClassRoom?.Name ?? string.Empty,
                PendingAmount = group.PendingAmount,
                IsOverdue = group.IsOverdue
            });
        }

        return result;
    }

    private async Task<List<ExamResultChartDto>> BuildExamResultsAsync(Guid schoolId)
    {
        var result = new List<ExamResultChartDto>();

        var exams = (await _unitOfWork.Exams.FindAsync(e =>
                !e.IsDeleted && e.SchoolId == schoolId))
            .Where(e => e.ApprovalStatus == ApprovalStatus.Approved)
            .OrderByDescending(e => e.StartDate)
            .ToList();

        var includedExams = 0;
        foreach (var exam in exams)
        {
            var classStats = new Dictionary<string, (string ClassName, string SectionName, int Passed, int Failed)>();

            void AddStudentResult(string className, string sectionName, bool isPassed)
            {
                var key = $"{className}|{sectionName}";
                if (!classStats.TryGetValue(key, out var stat))
                    stat = (className, sectionName, 0, 0);

                stat = (stat.ClassName, stat.SectionName, stat.Passed + (isPassed ? 1 : 0), stat.Failed + (isPassed ? 0 : 1));
                classStats[key] = stat;
            }

            var marks = await _unitOfWork.Marks.GetByExamAsync(exam.Id);
            foreach (var studentGroup in marks.GroupBy(m => m.StudentId))
            {
                var className = studentGroup.First().Student?.Section?.ClassRoom?.Name ?? string.Empty;
                var sectionName = studentGroup.First().Student?.Section?.Name ?? string.Empty;
                var isPassed = studentGroup.All(m => !m.IsAbsent && m.MarksObtained >= m.ExamSchedule.PassMarks);
                AddStudentResult(className, sectionName, isPassed);
            }

            var approvedSubmissions = (await _unitOfWork.ExamSubmissions.FindAsync(s =>
                    s.ExamId == exam.Id && !s.IsDeleted && s.GradingStatus == GradingStatus.Approved))
                .ToList();

            foreach (var submission in approvedSubmissions)
            {
                var student = await _unitOfWork.Students.GetStudentWithDetailsAsync(submission.StudentId);
                var className = student?.Section?.ClassRoom?.Name ?? string.Empty;
                var sectionName = student?.Section?.Name ?? string.Empty;
                var isPassed = submission.TotalMaxMarks > 0 &&
                    submission.TotalMarksObtained >= submission.TotalMaxMarks * 0.4m;
                AddStudentResult(className, sectionName, isPassed);
            }

            if (classStats.Count == 0)
                continue;

            foreach (var stat in classStats.Values)
            {
                result.Add(new ExamResultChartDto
                {
                    ExamId = exam.Id,
                    ExamName = exam.Name,
                    ClassName = stat.ClassName,
                    SectionName = stat.SectionName,
                    PassedCount = stat.Passed,
                    FailedCount = stat.Failed,
                    TotalCount = stat.Passed + stat.Failed
                });
            }

            includedExams++;
            if (includedExams >= 5)
                break;
        }

        return result;
    }

    private async Task<Guid> ResolveSchoolIdAsync(Guid schoolId)
    {
        if (schoolId != Guid.Empty)
            return schoolId;

        var schools = await _unitOfWork.Schools.GetAllAsync();
        return schools.FirstOrDefault()?.Id ?? Guid.Empty;
    }

    private async Task<List<BirthdayDto>> GetTodayBirthdaysAsync(Guid schoolId, DateTime today)
    {
        var result = new List<BirthdayDto>();

        var students = (await _unitOfWork.Students.GetPagedStudentsAsync(
            1, int.MaxValue, null, null, null, null, null, schoolId, null)).Items;
        foreach (var s in students.Where(s =>
            s.User?.DateOfBirth is { } dob && dob.Month == today.Month && dob.Day == today.Day))
        {
            result.Add(new BirthdayDto
            {
                Id = s.Id,
                Name = $"{s.User!.FirstName} {s.User!.LastName}".Trim(),
                Type = "Student",
                ClassName = s.Section?.ClassRoom?.Name,
                DateOfBirth = s.User!.DateOfBirth!.Value
            });
        }

        var teachers = (await _unitOfWork.Teachers.GetPagedTeachersAsync(
            1, int.MaxValue, null, null, null, null, schoolId, null)).Items;
        foreach (var t in teachers.Where(t =>
            t.User?.DateOfBirth is { } dob && dob.Month == today.Month && dob.Day == today.Day))
        {
            result.Add(new BirthdayDto
            {
                Id = t.Id,
                Name = $"{t.User!.FirstName} {t.User!.LastName}".Trim(),
                Type = "Teacher",
                ClassName = t.DepartmentName,
                DateOfBirth = t.User!.DateOfBirth!.Value
            });
        }

        var employees = (await _unitOfWork.Employees.GetPagedEmployeesAsync(
            1, int.MaxValue, null, null, null, null, null, null)).Items
            .Where(e => e.SchoolId == schoolId);
        foreach (var e in employees.Where(e =>
            e.User?.DateOfBirth is { } dob && dob.Month == today.Month && dob.Day == today.Day))
        {
            result.Add(new BirthdayDto
            {
                Id = e.Id,
                Name = $"{e.User!.FirstName} {e.User!.LastName}".Trim(),
                Type = "Employee",
                ClassName = e.Department?.Name,
                DateOfBirth = e.User!.DateOfBirth!.Value
            });
        }

        return result;
    }
}
