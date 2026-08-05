using Microsoft.EntityFrameworkCore;
using SchoolCRM.Application.Interfaces.Repositories;
using SchoolCRM.Domain.Entities.Library;
using SchoolCRM.Domain.Entities.Transport;
using SchoolCRM.Domain.Entities.Hostel;
using SchoolCRM.Infrastructure.Data;

namespace SchoolCRM.Infrastructure.Repositories;

public class BookRepository : GenericRepository<Book>, IBookRepository
{
    public BookRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Book?> GetBookWithDetailsAsync(Guid id)
    {
        return await _dbSet
            .Include(b => b.Category)
            .Include(b => b.Issues)
                .ThenInclude(i => i.Student)
                    .ThenInclude(s => s!.User)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<IReadOnlyList<Book>> SearchBooksAsync(string searchTerm, Guid? categoryId, Guid? schoolId)
    {
        var query = _dbSet
            .Include(b => b.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(b =>
                b.Title.ToLower().Contains(term) ||
                b.ISBN.ToLower().Contains(term) ||
                (b.Author != null && b.Author.ToLower().Contains(term)));
        }

        if (categoryId.HasValue)
            query = query.Where(b => b.CategoryId == categoryId.Value);

        if (schoolId.HasValue)
            query = query.Where(b => b.SchoolId == schoolId.Value);

        return await query.OrderBy(b => b.Title).ToListAsync();
    }

    public async Task<IReadOnlyList<Book>> GetAvailableBooksAsync(Guid? schoolId)
    {
        var query = _dbSet
            .Include(b => b.Category)
            .Where(b => b.AvailableCopies > 0);

        if (schoolId.HasValue)
            query = query.Where(b => b.SchoolId == schoolId.Value);

        return await query.OrderBy(b => b.Title).ToListAsync();
    }
}

public class BookIssueRepository : GenericRepository<BookIssue>, IBookIssueRepository
{
    public BookIssueRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<BookIssue>> GetByStudentAsync(Guid studentId)
    {
        return await _dbSet
            .Include(bi => bi.Book)
                .ThenInclude(b => b!.Category)
            .Where(bi => bi.StudentId == studentId)
            .OrderByDescending(bi => bi.IssueDate)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<BookIssue>> GetOverdueBooksAsync()
    {
        return await _dbSet
            .Include(bi => bi.Book)
            .Include(bi => bi.Student)
                .ThenInclude(s => s!.User)
            .Where(bi => !bi.IsReturned && bi.DueDate < DateTime.UtcNow)
            .OrderBy(bi => bi.DueDate)
            .ToListAsync();
    }

    public async Task<bool> HasActiveIssueAsync(Guid bookId, Guid studentId)
    {
        return await _dbSet.AnyAsync(bi =>
            bi.BookId == bookId
            && bi.StudentId == studentId
            && !bi.IsReturned);
    }
}

public class TransportRouteRepository : GenericRepository<TransportRoute>, ITransportRouteRepository
{
    public TransportRouteRepository(ApplicationDbContext context) : base(context) { }

    public async Task<TransportRoute?> GetRouteWithDetailsAsync(Guid id)
    {
        return await _dbSet
            .Include(tr => tr.Vehicles)
                .ThenInclude(v => v.Driver)
            .Include(tr => tr.PickupPoints)
            .Include(tr => tr.Allocations)
                .ThenInclude(a => a.Student)
                    .ThenInclude(s => s!.User)
            .FirstOrDefaultAsync(tr => tr.Id == id);
    }
}

public class HostelRoomRepository : GenericRepository<HostelRoom>, IHostelRoomRepository
{
    public HostelRoomRepository(ApplicationDbContext context) : base(context) { }

    public async Task<HostelRoom?> GetRoomWithDetailsAsync(Guid id)
    {
        return await _dbSet
            .Include(hr => hr.Beds)
            .Include(hr => hr.Allocations)
                .ThenInclude(a => a.Student)
                    .ThenInclude(s => s!.User)
            .FirstOrDefaultAsync(hr => hr.Id == id);
    }

    public async Task<IReadOnlyList<HostelRoom>> GetAvailableRoomsAsync(Guid? schoolId)
    {
        var query = _dbSet
            .Where(hr => hr.IsActive && hr.Occupied < hr.Capacity);

        if (schoolId.HasValue)
            query = query.Where(hr => hr.SchoolId == schoolId.Value);

        return await query
            .OrderBy(hr => hr.RoomNumber)
            .ToListAsync();
    }
}
