using Microsoft.EntityFrameworkCore;
using SchoolCRM.Application.Interfaces.Repositories;
using SchoolCRM.Domain.Entities.Exam;
using SchoolCRM.Infrastructure.Data;

namespace SchoolCRM.Infrastructure.Repositories;

public class ExamRepository : GenericRepository<Exam>, IExamRepository
{
    public ExamRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Exam?> GetExamWithDetailsAsync(Guid id)
    {
        return await _dbSet
            .Include(e => e.ExamType)
            .Include(e => e.School)
            .Include(e => e.ClassRoom)
            .Include(e => e.AcademicYear)
            .Include(e => e.Schedules)
                .ThenInclude(s => s.Subject)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IReadOnlyList<Exam>> GetByAcademicYearAsync(Guid academicYearId)
    {
        return await _dbSet
            .Include(e => e.ExamType)
            .Where(e => e.AcademicYearId == academicYearId)
            .OrderBy(e => e.StartDate)
            .ToListAsync();
    }
}

public class ExamScheduleRepository : GenericRepository<ExamSchedule>, IExamScheduleRepository
{
    public ExamScheduleRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<ExamSchedule>> GetByExamAsync(Guid examId)
    {
        return await _dbSet
            .Include(es => es.Subject)
            .Include(es => es.Exam)
            .Where(es => es.ExamId == examId)
            .OrderBy(es => es.ExamDate)
            .ThenBy(es => es.StartTime)
            .ToListAsync();
    }

    public async Task<ExamSchedule?> GetScheduleWithMarksAsync(Guid id)
    {
        return await _dbSet
            .Include(es => es.Subject)
            .Include(es => es.Exam)
            .Include(es => es.Marks)
                .ThenInclude(m => m.Student)
                    .ThenInclude(s => s!.User)
            .FirstOrDefaultAsync(es => es.Id == id);
    }
}

public class MarkRepository : GenericRepository<Mark>, IMarkRepository
{
    public MarkRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Mark>> GetByExamScheduleAsync(Guid examScheduleId)
    {
        return await _dbSet
            .Include(m => m.Student)
                .ThenInclude(s => s!.User)
            .Include(m => m.ExamSchedule)
                .ThenInclude(es => es!.Subject)
            .Where(m => m.ExamScheduleId == examScheduleId)
            .OrderBy(m => m.Student!.RollNumber)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Mark>> GetByStudentAsync(Guid studentId, Guid examId)
    {
        return await _dbSet
            .Include(m => m.ExamSchedule)
                .ThenInclude(es => es!.Subject)
            .Include(m => m.ExamSchedule)
                .ThenInclude(es => es!.Exam)
            .Where(m => m.StudentId == studentId
                && m.ExamSchedule.ExamId == examId)
            .ToListAsync();
    }

    public async Task<Mark?> GetByStudentAndScheduleAsync(Guid studentId, Guid examScheduleId)
    {
        return await _dbSet
            .Include(m => m.Student)
            .Include(m => m.ExamSchedule)
            .FirstOrDefaultAsync(m => m.StudentId == studentId
                && m.ExamScheduleId == examScheduleId);
    }

    public async Task<bool> AreMarksPublishedAsync(Guid examId)
    {
        return await _dbSet
            .AnyAsync(m => m.ExamSchedule.ExamId == examId && m.IsPublished);
    }
}

public class ReportCardRepository : GenericRepository<ReportCard>, IReportCardRepository
{
    public ReportCardRepository(ApplicationDbContext context) : base(context) { }

    public async Task<ReportCard?> GetByStudentAndExamAsync(Guid studentId, Guid examId)
    {
        return await _dbSet
            .Include(rc => rc.Student)
                .ThenInclude(s => s!.User)
            .Include(rc => rc.Exam)
                .ThenInclude(e => e!.ExamType)
            .FirstOrDefaultAsync(rc => rc.StudentId == studentId && rc.ExamId == examId);
    }

    public async Task<IReadOnlyList<ReportCard>> GetByExamAsync(Guid examId)
    {
        return await _dbSet
            .Include(rc => rc.Student)
                .ThenInclude(s => s!.User)
            .Include(rc => rc.Exam)
            .Where(rc => rc.ExamId == examId)
            .OrderBy(rc => rc.Rank)
            .ToListAsync();
    }
}
