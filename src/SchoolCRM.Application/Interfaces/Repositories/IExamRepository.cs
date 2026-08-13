using SchoolCRM.Domain.Entities.Exam;

namespace SchoolCRM.Application.Interfaces.Repositories;

public interface IExamRepository : IGenericRepository<Exam>
{
    Task<Exam?> GetExamWithDetailsAsync(Guid id);
    Task<IReadOnlyList<Exam>> GetByAcademicYearAsync(Guid academicYearId);
}

public interface IExamScheduleRepository : IGenericRepository<ExamSchedule>
{
    Task<IReadOnlyList<ExamSchedule>> GetByExamAsync(Guid examId);
    Task<ExamSchedule?> GetScheduleWithMarksAsync(Guid id);
}

public interface IMarkRepository : IGenericRepository<Mark>
{
    Task<IReadOnlyList<Mark>> GetByExamScheduleAsync(Guid examScheduleId);
    Task<IReadOnlyList<Mark>> GetByExamAsync(Guid examId);
    Task<IReadOnlyList<Mark>> GetByStudentAsync(Guid studentId, Guid examId);
    Task<IReadOnlyList<Mark>> GetByStudentAllAsync(Guid studentId);
    Task<Mark?> GetByStudentAndScheduleAsync(Guid studentId, Guid examScheduleId);
    Task<bool> AreMarksPublishedAsync(Guid examId);
}

public interface IReportCardRepository : IGenericRepository<ReportCard>
{
    Task<ReportCard?> GetByStudentAndExamAsync(Guid studentId, Guid examId);
    Task<IReadOnlyList<ReportCard>> GetByExamAsync(Guid examId);
}

public interface IExamQuestionRepository : IGenericRepository<ExamQuestion>
{
    Task<IReadOnlyList<ExamQuestion>> GetByExamAsync(Guid examId);
}

public interface IExamSubmissionRepository : IGenericRepository<ExamSubmission>
{
    Task<IReadOnlyList<ExamSubmission>> GetByExamAsync(Guid examId);
    Task<IReadOnlyList<ExamSubmission>> GetByStudentAsync(Guid studentId);
    Task<ExamSubmission?> GetByExamAndStudentAsync(Guid examId, Guid studentId);
}

public interface IExamAnswerRepository : IGenericRepository<ExamAnswer>
{
    Task<IReadOnlyList<ExamAnswer>> GetBySubmissionAsync(Guid examSubmissionId);
}
