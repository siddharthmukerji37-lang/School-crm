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
            var presentCount = todayAttendance.Count(a => a.Status == AttendanceStatus.Present);
            var absentCount = todayAttendance.Count(a => a.Status == AttendanceStatus.Absent);
            var lateCount = todayAttendance.Count(a => a.Status == AttendanceStatus.Late);
            var totalAttendance = todayAttendance.Count;

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
                UpcomingExamsList = upcomingExamsList
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

        return result;
    }
}
