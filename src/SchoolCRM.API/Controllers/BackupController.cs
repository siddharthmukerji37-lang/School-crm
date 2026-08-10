using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolCRM.Infrastructure.Data;

namespace SchoolCRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BackupController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public BackupController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportAsync()
    {
        var data = new Dictionary<string, object>
        {
            ["Schools"] = await _db.Schools.AsNoTracking().ToListAsync(),
            ["AcademicYears"] = await _db.AcademicYears.AsNoTracking().ToListAsync(),
            ["Branches"] = await _db.Branches.AsNoTracking().ToListAsync(),
            ["Departments"] = await _db.Departments.AsNoTracking().ToListAsync(),
            ["ClassRooms"] = await _db.ClassRooms.AsNoTracking().ToListAsync(),
            ["Sections"] = await _db.Sections.AsNoTracking().ToListAsync(),
            ["Subjects"] = await _db.Subjects.AsNoTracking().ToListAsync(),
            ["Timetables"] = await _db.Timetables.AsNoTracking().ToListAsync(),
            ["Periods"] = await _db.Periods.AsNoTracking().ToListAsync(),
            ["Holidays"] = await _db.Holidays.AsNoTracking().ToListAsync(),
            ["SchoolEvents"] = await _db.SchoolEvents.AsNoTracking().ToListAsync(),
            ["Students"] = await _db.Students.AsNoTracking().ToListAsync(),
            ["StudentDocuments"] = await _db.StudentDocuments.AsNoTracking().ToListAsync(),
            ["StudentHealthRecords"] = await _db.StudentHealthRecords.AsNoTracking().ToListAsync(),
            ["StudentPromotions"] = await _db.StudentPromotions.AsNoTracking().ToListAsync(),
            ["StudentTransfers"] = await _db.StudentTransfers.AsNoTracking().ToListAsync(),
            ["StudentLeaves"] = await _db.StudentLeaves.AsNoTracking().ToListAsync(),
            ["Parents"] = await _db.Parents.AsNoTracking().ToListAsync(),
            ["GuardianDetails"] = await _db.GuardianDetails.AsNoTracking().ToListAsync(),
            ["Teachers"] = await _db.Teachers.AsNoTracking().ToListAsync(),
            ["TeacherDocuments"] = await _db.TeacherDocuments.AsNoTracking().ToListAsync(),
            ["TeacherLeaves"] = await _db.TeacherLeaves.AsNoTracking().ToListAsync(),
            ["TeacherSalaries"] = await _db.TeacherSalaries.AsNoTracking().ToListAsync(),
            ["TeacherPerformances"] = await _db.TeacherPerformances.AsNoTracking().ToListAsync(),
            ["Employees"] = await _db.Employees.AsNoTracking().ToListAsync(),
            ["Designations"] = await _db.Designations.AsNoTracking().ToListAsync(),
            ["EmployeeDocuments"] = await _db.EmployeeDocuments.AsNoTracking().ToListAsync(),
            ["EmployeeLeaves"] = await _db.EmployeeLeaves.AsNoTracking().ToListAsync(),
            ["EmployeeSalaries"] = await _db.EmployeeSalaries.AsNoTracking().ToListAsync(),
            ["Attendances"] = await _db.Attendances.AsNoTracking().ToListAsync(),
            ["AttendanceSummaries"] = await _db.AttendanceSummaries.AsNoTracking().ToListAsync(),
            ["ExamTypes"] = await _db.ExamTypes.AsNoTracking().ToListAsync(),
            ["Exams"] = await _db.Exams.AsNoTracking().ToListAsync(),
            ["ExamSchedules"] = await _db.ExamSchedules.AsNoTracking().ToListAsync(),
            ["Marks"] = await _db.Marks.AsNoTracking().ToListAsync(),
            ["GradeSystems"] = await _db.GradeSystems.AsNoTracking().ToListAsync(),
            ["ReportCards"] = await _db.ReportCards.AsNoTracking().ToListAsync(),
            ["FeeHeads"] = await _db.FeeHeads.AsNoTracking().ToListAsync(),
            ["FeeStructures"] = await _db.FeeStructures.AsNoTracking().ToListAsync(),
            ["FeeInstallments"] = await _db.FeeInstallments.AsNoTracking().ToListAsync(),
            ["FeeReceipts"] = await _db.FeeReceipts.AsNoTracking().ToListAsync(),
            ["FeeDiscounts"] = await _db.FeeDiscounts.AsNoTracking().ToListAsync(),
            ["Scholarships"] = await _db.Scholarships.AsNoTracking().ToListAsync(),
            ["BookCategories"] = await _db.BookCategories.AsNoTracking().ToListAsync(),
            ["Books"] = await _db.Books.AsNoTracking().ToListAsync(),
            ["BookIssues"] = await _db.BookIssues.AsNoTracking().ToListAsync(),
            ["TransportRoutes"] = await _db.TransportRoutes.AsNoTracking().ToListAsync(),
            ["Vehicles"] = await _db.Vehicles.AsNoTracking().ToListAsync(),
            ["Drivers"] = await _db.Drivers.AsNoTracking().ToListAsync(),
            ["PickupPoints"] = await _db.PickupPoints.AsNoTracking().ToListAsync(),
            ["StudentTransportAllocations"] = await _db.StudentTransportAllocations.AsNoTracking().ToListAsync(),
            ["Hostels"] = await _db.Hostels.AsNoTracking().ToListAsync(),
            ["HostelRooms"] = await _db.HostelRooms.AsNoTracking().ToListAsync(),
            ["HostelBeds"] = await _db.HostelBeds.AsNoTracking().ToListAsync(),
            ["HostelAllocations"] = await _db.HostelAllocations.AsNoTracking().ToListAsync(),
            ["HostelVisitors"] = await _db.HostelVisitors.AsNoTracking().ToListAsync(),
            ["InventoryCategories"] = await _db.InventoryCategories.AsNoTracking().ToListAsync(),
            ["Vendors"] = await _db.Vendors.AsNoTracking().ToListAsync(),
            ["InventoryItems"] = await _db.InventoryItems.AsNoTracking().ToListAsync(),
            ["StockTransactions"] = await _db.StockTransactions.AsNoTracking().ToListAsync(),
            ["AccountHeads"] = await _db.AccountHeads.AsNoTracking().ToListAsync(),
            ["LedgerEntries"] = await _db.LedgerEntries.AsNoTracking().ToListAsync(),
            ["Incomes"] = await _db.Incomes.AsNoTracking().ToListAsync(),
            ["Expenses"] = await _db.Expenses.AsNoTracking().ToListAsync(),
            ["BankAccounts"] = await _db.BankAccounts.AsNoTracking().ToListAsync(),
            ["Notifications"] = await _db.Notifications.AsNoTracking().ToListAsync(),
            ["Announcements"] = await _db.Announcements.AsNoTracking().ToListAsync(),
            ["Circulars"] = await _db.Circulars.AsNoTracking().ToListAsync(),
            ["ChatMessages"] = await _db.ChatMessages.AsNoTracking().ToListAsync(),
            ["Homeworks"] = await _db.Homeworks.AsNoTracking().ToListAsync(),
            ["HomeworkSubmissions"] = await _db.HomeworkSubmissions.AsNoTracking().ToListAsync(),
            ["Assignments"] = await _db.Assignments.AsNoTracking().ToListAsync(),
            ["AssignmentSubmissions"] = await _db.AssignmentSubmissions.AsNoTracking().ToListAsync(),
            ["SchoolSettings"] = await _db.SchoolSettings.AsNoTracking().ToListAsync(),
            ["EmailSettings"] = await _db.EmailSettings.AsNoTracking().ToListAsync(),
            ["SmsSettings"] = await _db.SmsSettings.AsNoTracking().ToListAsync(),
            ["AuditLogs"] = await _db.AuditLogs.AsNoTracking().ToListAsync(),
            ["LoginHistories"] = await _db.LoginHistories.AsNoTracking().ToListAsync(),
            ["Permissions"] = await _db.Permissions.AsNoTracking().ToListAsync(),
            ["RolePermissions"] = await _db.RolePermissions.AsNoTracking().ToListAsync(),
            ["DataBackups"] = await _db.DataBackups.AsNoTracking().ToListAsync(),
            ["StudentReports"] = await _db.StudentReports.AsNoTracking().ToListAsync()
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        var json = JsonSerializer.Serialize(data, options);
        var bytes = Encoding.UTF8.GetBytes(json);
        var fileName = $"school_backup_{DateTime.UtcNow:yyyy-MM-dd_HHmmss}.json";

        return File(bytes, "application/json", fileName);
    }
}
