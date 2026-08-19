using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using SchoolCRM.Application.Interfaces.Repositories;
using SchoolCRM.Infrastructure.Data;

namespace SchoolCRM.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly IServiceProvider _serviceProvider;
    private IDbContextTransaction? _transaction;
    private readonly Dictionary<Type, object> _repositories = new();

    public UnitOfWork(ApplicationDbContext context, IServiceProvider serviceProvider)
    {
        _context = context;
        _serviceProvider = serviceProvider;
    }

    public ISchoolRepository Schools => ResolveRepository<ISchoolRepository>();
    public IAcademicYearRepository AcademicYears => ResolveRepository<IAcademicYearRepository>();
    public IClassRoomRepository ClassRooms => ResolveRepository<IClassRoomRepository>();
    public ISectionRepository Sections => ResolveRepository<ISectionRepository>();
    public ISubjectRepository Subjects => ResolveRepository<ISubjectRepository>();
    public ITimetableRepository Timetables => ResolveRepository<ITimetableRepository>();
    public IStudentRepository Students => ResolveRepository<IStudentRepository>();
    public IStudentDocumentRepository StudentDocuments => ResolveRepository<IStudentDocumentRepository>();
    public IStudentHealthRecordRepository StudentHealthRecords => ResolveRepository<IStudentHealthRecordRepository>();
    public IStudentLeaveRepository StudentLeaves => ResolveRepository<IStudentLeaveRepository>();
    public IParentRepository Parents => ResolveRepository<IParentRepository>();
    public ITeacherRepository Teachers => ResolveRepository<ITeacherRepository>();
    public ITeacherLeaveRepository TeacherLeaves => ResolveRepository<ITeacherLeaveRepository>();
    public ITeacherSalaryRepository TeacherSalaries => ResolveRepository<ITeacherSalaryRepository>();
    public IEmployeeRepository Employees => ResolveRepository<IEmployeeRepository>();
    public IEmployeeLeaveRepository EmployeeLeaves => ResolveRepository<IEmployeeLeaveRepository>();
    public IEmployeeSalaryRepository EmployeeSalaries => ResolveRepository<IEmployeeSalaryRepository>();
    public IDesignationRepository Designations => ResolveRepository<IDesignationRepository>();
    public IAttendanceRepository Attendances => ResolveRepository<IAttendanceRepository>();
    public IExamRepository Exams => ResolveRepository<IExamRepository>();
    public IExamScheduleRepository ExamSchedules => ResolveRepository<IExamScheduleRepository>();
    public IMarkRepository Marks => ResolveRepository<IMarkRepository>();
    public IReportCardRepository ReportCards => ResolveRepository<IReportCardRepository>();
    public IExamQuestionRepository ExamQuestions => ResolveRepository<IExamQuestionRepository>();
    public IExamSubmissionRepository ExamSubmissions => ResolveRepository<IExamSubmissionRepository>();
    public IExamAnswerRepository ExamAnswers => ResolveRepository<IExamAnswerRepository>();
    public IFeeStructureRepository FeeStructures => ResolveRepository<IFeeStructureRepository>();
    public IFeeInstallmentRepository FeeInstallments => ResolveRepository<IFeeInstallmentRepository>();
    public IFeeReceiptRepository FeeReceipts => ResolveRepository<IFeeReceiptRepository>();
    public IBookRepository Books => ResolveRepository<IBookRepository>();
    public IBookIssueRepository BookIssues => ResolveRepository<IBookIssueRepository>();
    public ITransportRouteRepository TransportRoutes => ResolveRepository<ITransportRouteRepository>();
    public IHostelRoomRepository HostelRooms => ResolveRepository<IHostelRoomRepository>();
    public IHostelRepository Hostels => ResolveRepository<IHostelRepository>();
    public INotificationRepository Notifications => ResolveRepository<INotificationRepository>();
    public IAnnouncementRepository Announcements => ResolveRepository<IAnnouncementRepository>();
    public IChatMessageRepository ChatMessages => ResolveRepository<IChatMessageRepository>();
    public IAuditLogRepository AuditLogs => ResolveRepository<IAuditLogRepository>();
    public IAccountHeadRepository AccountHeads => ResolveRepository<IAccountHeadRepository>();
    public IIncomeRepository Incomes => ResolveRepository<IIncomeRepository>();
    public IExpenseRepository Expenses => ResolveRepository<IExpenseRepository>();
    public IInventoryItemRepository InventoryItems => ResolveRepository<IInventoryItemRepository>();
    public IAttendancePolicyRepository AttendancePolicies => ResolveRepository<IAttendancePolicyRepository>();
    public IAttendanceMonthlySummaryRepository AttendanceMonthlySummaries => ResolveRepository<IAttendanceMonthlySummaryRepository>();
    public ISalaryDeductionRepository SalaryDeductions => ResolveRepository<ISalaryDeductionRepository>();
    public ILeaveCalendarRepository LeaveCalendars => ResolveRepository<ILeaveCalendarRepository>();
    public ILeaveTypeRepository LeaveTypes => ResolveRepository<ILeaveTypeRepository>();
    public ILeaveTypeConfigRepository LeaveTypeConfigs => ResolveRepository<ILeaveTypeConfigRepository>();
    public ILeaveBalanceRepository LeaveBalances => ResolveRepository<ILeaveBalanceRepository>();
    public ILeaveRequestRepository LeaveRequests => ResolveRepository<ILeaveRequestRepository>();
    public ILeaveRequestDayRepository LeaveRequestDays => ResolveRepository<ILeaveRequestDayRepository>();

    public IGenericRepository<T> Repository<T>() where T : class
    {
        return ResolveRepository<IGenericRepository<T>>();
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
            throw new InvalidOperationException("Transaction has not been started.");

        await _transaction.CommitAsync(cancellationToken);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
            throw new InvalidOperationException("Transaction has not been started.");

        await _transaction.RollbackAsync(cancellationToken);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    private TRepository ResolveRepository<TRepository>() where TRepository : class
    {
        var type = typeof(TRepository);

        if (!_repositories.ContainsKey(type))
        {
            var repository = _serviceProvider.GetRequiredService<TRepository>();
            _repositories[type] = repository;
        }

        return (TRepository)_repositories[type];
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
