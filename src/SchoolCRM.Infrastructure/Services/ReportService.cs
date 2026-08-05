using System.Text;
using SchoolCRM.Application.Interfaces.Repositories;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.Shared.Constants;
using SchoolCRM.Shared.Models;
using static SchoolCRM.Application.Interfaces.Services.IReportService;

namespace SchoolCRM.Infrastructure.Services;

public class ReportService : IReportService
{
    private readonly IUnitOfWork _unitOfWork;

    public ReportService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<byte[]>> GenerateStudentReportAsync(
        Guid schoolId, Guid? classRoomId, Guid? sectionId, string format)
    {
        try
        {
            var (items, _) = await _unitOfWork.Students.GetPagedStudentsAsync(
                1, 10000, null, null, null, sectionId, classRoomId, schoolId, null);

            var sb = new StringBuilder();
            sb.AppendLine("Student Report");
            sb.AppendLine($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine("Name,Admission Number,Class,Section,Status,Admission Date");

            foreach (var s in items)
            {
                sb.AppendLine($"{s.User?.FirstName} {s.User?.LastName},{s.AdmissionNumber}," +
                    $"{s.Section?.ClassRoom?.Name},{s.Section?.Name},{s.Status},{s.AdmissionDate:yyyy-MM-dd}");
            }

            return ApiResponse<byte[]>.SuccessResponse(Encoding.UTF8.GetBytes(sb.ToString()));
        }
        catch (Exception ex)
        {
            return ApiResponse<byte[]>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<byte[]>> GenerateAttendanceReportAsync(
        Guid schoolId, DateTime fromDate, DateTime toDate, Guid? classRoomId, string format)
    {
        try
        {
            var sb = new StringBuilder();
            sb.AppendLine("Attendance Report");
            sb.AppendLine($"Period: {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}");
            sb.AppendLine("Date,Student,Status,Remarks");

            for (var date = fromDate; date <= toDate; date = date.AddDays(1))
            {
                var attendances = await _unitOfWork.Attendances.GetByDateAsync(date, schoolId);
                foreach (var a in attendances)
                {
                    sb.AppendLine($"{a.Date:yyyy-MM-dd}," +
                        $"{a.Student?.User?.FirstName} {a.Student?.User?.LastName}," +
                        $"{a.Status},{a.Remarks}");
                }
            }

            return ApiResponse<byte[]>.SuccessResponse(Encoding.UTF8.GetBytes(sb.ToString()));
        }
        catch (Exception ex)
        {
            return ApiResponse<byte[]>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<byte[]>> GenerateFeeReportAsync(
        Guid schoolId, DateTime fromDate, DateTime toDate, Guid? classRoomId, string format)
    {
        try
        {
            var receipts = await _unitOfWork.FeeReceipts.GetAllAsync();
            var filtered = receipts.Where(r => !r.IsDeleted && r.PaidAt >= fromDate && r.PaidAt <= toDate);

            var sb = new StringBuilder();
            sb.AppendLine("Fee Collection Report");
            sb.AppendLine($"Period: {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}");
            sb.AppendLine("Receipt Number,Student,Amount,Fine,Total Paid,Payment Method,Date");

            foreach (var r in filtered)
            {
                sb.AppendLine($"{r.ReceiptNumber}," +
                    $"{r.FeeInstallment?.Student?.User?.FirstName} {r.FeeInstallment?.Student?.User?.LastName}," +
                    $"{r.Amount},{r.Fine},{r.TotalPaid},{r.PaymentMethod},{r.PaidAt:yyyy-MM-dd}");
            }

            return ApiResponse<byte[]>.SuccessResponse(Encoding.UTF8.GetBytes(sb.ToString()));
        }
        catch (Exception ex)
        {
            return ApiResponse<byte[]>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<byte[]>> GenerateExamReportAsync(
        Guid schoolId, Guid examId, Guid? classRoomId, string format)
    {
        try
        {
            var exam = await _unitOfWork.Exams.GetByIdAsync(examId);
            if (exam is null)
                return ApiResponse<byte[]>.NotFoundResponse(ApplicationMessages.NotFound);

            var sb = new StringBuilder();
            sb.AppendLine($"Exam Report: {exam.Name}");
            sb.AppendLine($"Period: {exam.StartDate:yyyy-MM-dd} to {exam.EndDate:yyyy-MM-dd}");
            sb.AppendLine("Student,Admission Number,Marks Obtained,Status");

            return ApiResponse<byte[]>.SuccessResponse(Encoding.UTF8.GetBytes(sb.ToString()));
        }
        catch (Exception ex)
        {
            return ApiResponse<byte[]>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<byte[]>> GenerateStudentReportCardAsync(
        Guid studentId, Guid examId, string format)
    {
        try
        {
            var student = await _unitOfWork.Students.GetStudentWithDetailsAsync(studentId);
            if (student is null)
                return ApiResponse<byte[]>.NotFoundResponse(ApplicationMessages.NotFound);

            var marks = await _unitOfWork.Marks.GetByStudentAsync(studentId, examId);

            var sb = new StringBuilder();
            sb.AppendLine("Student Report Card");
            sb.AppendLine($"Student: {student.User?.FirstName} {student.User?.LastName}");
            sb.AppendLine($"Admission: {student.AdmissionNumber}");
            sb.AppendLine($"Class: {student.Section?.ClassRoom?.Name} - {student.Section?.Name}");
            sb.AppendLine("Subject,Marks Obtained,Max Marks,Status");

            foreach (var m in marks)
            {
                sb.AppendLine($"{m.ExamSchedule?.Subject?.Name},{m.MarksObtained}," +
                    $"{m.ExamSchedule?.MaxMarks},{(m.MarksObtained >= m.ExamSchedule?.PassMarks ? "Pass" : "Fail")}");
            }

            return ApiResponse<byte[]>.SuccessResponse(Encoding.UTF8.GetBytes(sb.ToString()));
        }
        catch (Exception ex)
        {
            return ApiResponse<byte[]>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<byte[]>> GenerateEmployeeReportAsync(Guid schoolId, string format)
    {
        try
        {
            var employees = await _unitOfWork.Employees.GetAllAsync();
            var filtered = employees.Where(e => !e.IsDeleted && e.SchoolId == schoolId);

            var sb = new StringBuilder();
            sb.AppendLine("Employee Report");
            sb.AppendLine("Code,Name,Department,Status,Joining Date");

            foreach (var e in filtered)
            {
                sb.AppendLine($"{e.EmployeeCode},{e.User?.FirstName} {e.User?.LastName}," +
                    $"{e.Department?.Name},{e.Status},{e.JoiningDate:yyyy-MM-dd}");
            }

            return ApiResponse<byte[]>.SuccessResponse(Encoding.UTF8.GetBytes(sb.ToString()));
        }
        catch (Exception ex)
        {
            return ApiResponse<byte[]>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<byte[]>> GenerateInventoryReportAsync(Guid schoolId, string format)
    {
        try
        {
            var items = await _unitOfWork.InventoryItems.GetAllAsync();
            var filtered = items.Where(i => !i.IsDeleted && i.SchoolId == schoolId);

            var sb = new StringBuilder();
            sb.AppendLine("Inventory Report");
            sb.AppendLine("Name,Code,Stock,Minimum Stock,Status");

            foreach (var i in filtered)
            {
                sb.AppendLine($"{i.Name},{i.Code},{i.CurrentStock},{i.MinimumStock}," +
                    $"{(i.CurrentStock <= i.MinimumStock ? "Low Stock" : "OK")}");
            }

            return ApiResponse<byte[]>.SuccessResponse(Encoding.UTF8.GetBytes(sb.ToString()));
        }
        catch (Exception ex)
        {
            return ApiResponse<byte[]>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<byte[]>> GenerateAccountReportAsync(
        Guid schoolId, DateTime fromDate, DateTime toDate, string format)
    {
        try
        {
            var (totalIncome, _) = await _unitOfWork.Incomes.GetIncomeSummaryAsync(schoolId, fromDate, toDate);
            var (totalExpense, _) = await _unitOfWork.Expenses.GetExpenseSummaryAsync(schoolId, fromDate, toDate);

            var sb = new StringBuilder();
            sb.AppendLine("Account Report");
            sb.AppendLine($"Period: {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}");
            sb.AppendLine($"Total Income: {totalIncome:C}");
            sb.AppendLine($"Total Expense: {totalExpense:C}");
            sb.AppendLine($"Net Balance: {totalIncome - totalExpense:C}");

            return ApiResponse<byte[]>.SuccessResponse(Encoding.UTF8.GetBytes(sb.ToString()));
        }
        catch (Exception ex)
        {
            return ApiResponse<byte[]>.FailResponse(ex.Message);
        }
    }

    public Task<ApiResponse<List<ReportTemplateDto>>> GetReportTemplatesAsync()
    {
        var templates = new List<ReportTemplateDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Student List", Description = "List of all students", Category = "Students", FileType = "csv" },
            new() { Id = Guid.NewGuid(), Name = "Attendance Report", Description = "Monthly attendance summary", Category = "Attendance", FileType = "csv" },
            new() { Id = Guid.NewGuid(), Name = "Fee Collection", Description = "Fee collection summary", Category = "Fees", FileType = "csv" },
            new() { Id = Guid.NewGuid(), Name = "Exam Results", Description = "Exam results report", Category = "Exams", FileType = "csv" },
            new() { Id = Guid.NewGuid(), Name = "Employee Report", Description = "Employee listing", Category = "Employees", FileType = "csv" },
            new() { Id = Guid.NewGuid(), Name = "Inventory Report", Description = "Stock status report", Category = "Inventory", FileType = "csv" }
        };

        return Task.FromResult(ApiResponse<List<ReportTemplateDto>>.SuccessResponse(templates));
    }

    public async Task<ApiResponse<byte[]>> GenerateCustomReportAsync(CustomReportRequestDto dto)
    {
        try
        {
            return dto.ReportType.ToLower() switch
            {
                "students" => await GenerateStudentReportAsync(dto.SchoolId, null, null, dto.Format),
                "attendance" => await GenerateAttendanceReportAsync(dto.SchoolId, DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow, null, dto.Format),
                "fees" => await GenerateFeeReportAsync(dto.SchoolId, DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow, null, dto.Format),
                "employees" => await GenerateEmployeeReportAsync(dto.SchoolId, dto.Format),
                "inventory" => await GenerateInventoryReportAsync(dto.SchoolId, dto.Format),
                _ => ApiResponse<byte[]>.FailResponse("Invalid report type.")
            };
        }
        catch (Exception ex)
        {
            return ApiResponse<byte[]>.FailResponse(ex.Message);
        }
    }
}
