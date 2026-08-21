using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SchoolCRM.Domain.Common;
using SchoolCRM.Domain.Entities.Identity;
using SchoolCRM.Domain.Entities.Account;
using SchoolCRM.Domain.Entities.Attendance;
using SchoolCRM.Domain.Entities.Employee;
using SchoolCRM.Domain.Entities.Exam;
using SchoolCRM.Domain.Entities.Fee;
using SchoolCRM.Domain.Entities.Hostel;
using SchoolCRM.Domain.Entities.Homework;
using SchoolCRM.Domain.Entities.Library;
using SchoolCRM.Domain.Entities.Notification;
using SchoolCRM.Domain.Entities.Parent;
using SchoolCRM.Domain.Entities.Report;
using SchoolCRM.Domain.Entities.School;
using SchoolCRM.Domain.Entities.Setting;
using SchoolCRM.Domain.Entities.Student;
using SchoolCRM.Domain.Entities.Teacher;
using StudentEntity = SchoolCRM.Domain.Entities.Student.Student;
using TeacherEntity = SchoolCRM.Domain.Entities.Teacher.Teacher;
using EmployeeEntity = SchoolCRM.Domain.Entities.Employee.Employee;
using SchoolCRM.Domain.Entities.Transport;
using SchoolCRM.Domain.Entities.Inventory;
using SchoolCRM.Domain.Entities.Leave;
using SchoolCRM.Domain.Entities.Payroll;

namespace SchoolCRM.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid,
    ApplicationUserClaim, ApplicationUserRole, ApplicationUserLogin,
    ApplicationRoleClaim, ApplicationUserToken>
{
    private readonly ICurrentUserService? _currentUserService;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService? currentUserService = null)
        : base(options)
    {
        _currentUserService = currentUserService;
    }

    public DbSet<Domain.Entities.School.School> Schools => Set<Domain.Entities.School.School>();
    public DbSet<AcademicYear> AcademicYears => Set<AcademicYear>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<ClassRoom> ClassRooms => Set<ClassRoom>();
    public DbSet<Section> Sections => Set<Section>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<Timetable> Timetables => Set<Timetable>();
    public DbSet<Period> Periods => Set<Period>();
    public DbSet<Holiday> Holidays => Set<Holiday>();
    public DbSet<SchoolEvent> SchoolEvents => Set<SchoolEvent>();

    public DbSet<StudentEntity> Students => Set<StudentEntity>();
    public DbSet<StudentDocument> StudentDocuments => Set<StudentDocument>();
    public DbSet<StudentHealthRecord> StudentHealthRecords => Set<StudentHealthRecord>();
    public DbSet<StudentPromotion> StudentPromotions => Set<StudentPromotion>();
    public DbSet<StudentTransfer> StudentTransfers => Set<StudentTransfer>();
    public DbSet<StudentLeave> StudentLeaves => Set<StudentLeave>();

    public DbSet<Domain.Entities.Parent.Parent> Parents => Set<Domain.Entities.Parent.Parent>();
    public DbSet<GuardianDetail> GuardianDetails => Set<GuardianDetail>();

    public DbSet<TeacherEntity> Teachers => Set<TeacherEntity>();
    public DbSet<TeacherDocument> TeacherDocuments => Set<TeacherDocument>();
    public DbSet<TeacherLeave> TeacherLeaves => Set<TeacherLeave>();
    public DbSet<TeacherSalary> TeacherSalaries => Set<TeacherSalary>();
    public DbSet<TeacherPerformance> TeacherPerformances => Set<TeacherPerformance>();

    public DbSet<EmployeeEntity> Employees => Set<EmployeeEntity>();
    public DbSet<Designation> Designations => Set<Designation>();
    public DbSet<EmployeeDocument> EmployeeDocuments => Set<EmployeeDocument>();
    public DbSet<EmployeeLeave> EmployeeLeaves => Set<EmployeeLeave>();
    public DbSet<EmployeeSalary> EmployeeSalaries => Set<EmployeeSalary>();

    public DbSet<Domain.Entities.Attendance.Attendance> Attendances => Set<Domain.Entities.Attendance.Attendance>();
    public DbSet<AttendanceSummary> AttendanceSummaries => Set<AttendanceSummary>();
    public DbSet<AttendancePolicy> AttendancePolicies => Set<AttendancePolicy>();
    public DbSet<AttendanceMonthlySummary> AttendanceMonthlySummaries => Set<AttendanceMonthlySummary>();
    public DbSet<SalaryDeduction> SalaryDeductions => Set<SalaryDeduction>();

    public DbSet<LeaveCalendar> LeaveCalendars => Set<LeaveCalendar>();
    public DbSet<LeaveType> LeaveTypes => Set<LeaveType>();
    public DbSet<LeaveTypeConfig> LeaveTypeConfigs => Set<LeaveTypeConfig>();
    public DbSet<LeaveBalance> LeaveBalances => Set<LeaveBalance>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<LeaveRequestDay> LeaveRequestDays => Set<LeaveRequestDay>();

    public DbSet<PayrollSetting> PayrollSettings => Set<PayrollSetting>();
    public DbSet<SalaryProfile> SalaryProfiles => Set<SalaryProfile>();
    public DbSet<SalaryComponent> SalaryComponents => Set<SalaryComponent>();
    public DbSet<Domain.Entities.Payroll.Payroll> Payrolls => Set<Domain.Entities.Payroll.Payroll>();
    public DbSet<PayrollDeduction> PayrollDeductions => Set<PayrollDeduction>();
    public DbSet<Payslip> Payslips => Set<Payslip>();

    public DbSet<ExamType> ExamTypes => Set<ExamType>();
    public DbSet<Exam> Exams => Set<Exam>();
    public DbSet<ExamSchedule> ExamSchedules => Set<ExamSchedule>();
    public DbSet<ExamQuestion> ExamQuestions => Set<ExamQuestion>();
    public DbSet<ExamSubmission> ExamSubmissions => Set<ExamSubmission>();
    public DbSet<ExamAnswer> ExamAnswers => Set<ExamAnswer>();
    public DbSet<Mark> Marks => Set<Mark>();
    public DbSet<GradeSystem> GradeSystems => Set<GradeSystem>();
    public DbSet<ReportCard> ReportCards => Set<ReportCard>();

    public DbSet<FeeHead> FeeHeads => Set<FeeHead>();
    public DbSet<FeeStructure> FeeStructures => Set<FeeStructure>();
    public DbSet<FeeInstallment> FeeInstallments => Set<FeeInstallment>();
    public DbSet<FeeReceipt> FeeReceipts => Set<FeeReceipt>();
    public DbSet<FeeDiscount> FeeDiscounts => Set<FeeDiscount>();
    public DbSet<Scholarship> Scholarships => Set<Scholarship>();

    public DbSet<BookCategory> BookCategories => Set<BookCategory>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<BookIssue> BookIssues => Set<BookIssue>();

    public DbSet<TransportRoute> TransportRoutes => Set<TransportRoute>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<PickupPoint> PickupPoints => Set<PickupPoint>();
    public DbSet<StudentTransportAllocation> StudentTransportAllocations => Set<StudentTransportAllocation>();

    public DbSet<Hostel> Hostels => Set<Hostel>();
    public DbSet<HostelRoom> HostelRooms => Set<HostelRoom>();
    public DbSet<HostelBed> HostelBeds => Set<HostelBed>();
    public DbSet<HostelAllocation> HostelAllocations => Set<HostelAllocation>();
    public DbSet<HostelVisitor> HostelVisitors => Set<HostelVisitor>();

    public DbSet<InventoryCategory> InventoryCategories => Set<InventoryCategory>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();

    public DbSet<AccountHead> AccountHeads => Set<AccountHead>();
    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();
    public DbSet<Income> Incomes => Set<Income>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();

    public DbSet<Domain.Entities.Notification.Notification> Notifications => Set<Domain.Entities.Notification.Notification>();
    public DbSet<Announcement> Announcements => Set<Announcement>();
    public DbSet<Circular> Circulars => Set<Circular>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    public DbSet<Homework> Homeworks => Set<Homework>();
    public DbSet<HomeworkSubmission> HomeworkSubmissions => Set<HomeworkSubmission>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<AssignmentSubmission> AssignmentSubmissions => Set<AssignmentSubmission>();

    public DbSet<SchoolSetting> SchoolSettings => Set<SchoolSetting>();
    public DbSet<EmailSetting> EmailSettings => Set<EmailSetting>();
    public DbSet<SmsSetting> SmsSettings => Set<SmsSetting>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<LoginHistory> LoginHistories => Set<LoginHistory>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<DataBackup> DataBackups => Set<DataBackup>();

    public DbSet<StudentReport> StudentReports => Set<StudentReport>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType).Property<bool>("IsDeleted");
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(
                    IsDeletedFilter(entityType.ClrType));
            }
        }

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    private static System.Linq.Expressions.LambdaExpression IsDeletedFilter(Type entityType)
    {
        var parameter = System.Linq.Expressions.Expression.Parameter(entityType, "e");
        var property = System.Linq.Expressions.Expression.Property(parameter, "IsDeleted");
        var constant = System.Linq.Expressions.Expression.Constant(false);
        var comparison = System.Linq.Expressions.Expression.Equal(property, constant);
        return System.Linq.Expressions.Expression.Lambda(comparison, parameter);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            var userId = _currentUserService?.UserId ?? "System";

            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.CreatedBy = userId;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedBy = userId;
                    break;
                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = DateTime.UtcNow;
                    entry.Entity.DeletedBy = userId;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}

public interface ICurrentUserService
{
    string? UserId { get; }
    string? Email { get; }
    string? FullName { get; }
    Guid? SchoolId { get; }
    IReadOnlyList<string> Roles { get; }
}
