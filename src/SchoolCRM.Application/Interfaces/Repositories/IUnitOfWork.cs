namespace SchoolCRM.Application.Interfaces.Repositories;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<T> Repository<T>() where T : class;
    ISchoolRepository Schools { get; }
    IAcademicYearRepository AcademicYears { get; }
    IClassRoomRepository ClassRooms { get; }
    ISectionRepository Sections { get; }
    ISubjectRepository Subjects { get; }
    ITimetableRepository Timetables { get; }
    IStudentRepository Students { get; }
    IStudentDocumentRepository StudentDocuments { get; }
    IStudentHealthRecordRepository StudentHealthRecords { get; }
    IStudentLeaveRepository StudentLeaves { get; }
    IParentRepository Parents { get; }
    ITeacherRepository Teachers { get; }
    ITeacherLeaveRepository TeacherLeaves { get; }
    ITeacherSalaryRepository TeacherSalaries { get; }
    IEmployeeRepository Employees { get; }
    IEmployeeLeaveRepository EmployeeLeaves { get; }
    IEmployeeSalaryRepository EmployeeSalaries { get; }
    IDesignationRepository Designations { get; }
    IAttendanceRepository Attendances { get; }
    IExamRepository Exams { get; }
    IExamScheduleRepository ExamSchedules { get; }
    IMarkRepository Marks { get; }
    IReportCardRepository ReportCards { get; }
    IExamQuestionRepository ExamQuestions { get; }
    IExamSubmissionRepository ExamSubmissions { get; }
    IExamAnswerRepository ExamAnswers { get; }
    IFeeStructureRepository FeeStructures { get; }
    IFeeInstallmentRepository FeeInstallments { get; }
    IFeeReceiptRepository FeeReceipts { get; }
    IBookRepository Books { get; }
    IBookIssueRepository BookIssues { get; }
    ITransportRouteRepository TransportRoutes { get; }
    IHostelRoomRepository HostelRooms { get; }
    IHostelRepository Hostels { get; }
    INotificationRepository Notifications { get; }
    IAnnouncementRepository Announcements { get; }
    IChatMessageRepository ChatMessages { get; }
    IAuditLogRepository AuditLogs { get; }
    IAccountHeadRepository AccountHeads { get; }
    IIncomeRepository Incomes { get; }
    IExpenseRepository Expenses { get; }
    IInventoryItemRepository InventoryItems { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
