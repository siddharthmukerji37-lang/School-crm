using System.Linq.Expressions;
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

    public async Task<(IReadOnlyList<BookIssue> Items, int TotalCount)> GetIssuedPagedAsync(
        int pageNumber, int pageSize,
        Expression<Func<BookIssue, bool>>? filter = null)
    {
        var query = _dbSet
            .Include(bi => bi.Book)
            .Include(bi => bi.Student)
                .ThenInclude(s => s!.User)
            .AsQueryable();

        if (filter is not null)
            query = query.Where(filter);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(bi => bi.IssueDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
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

    public async Task<IReadOnlyList<Vehicle>> GetVehiclesWithDetailsAsync()
    {
        return await _context.Set<Vehicle>()
            .Include(v => v.Driver)
            .Include(v => v.Route)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<StudentTransportAllocation>> GetAllocationsWithDetailsAsync()
    {
        return await _context.Set<StudentTransportAllocation>()
            .Include(a => a.Student)
                .ThenInclude(s => s!.User)
            .Include(a => a.Route)
            .ToListAsync();
    }
}

public class HostelRoomRepository : GenericRepository<HostelRoom>, IHostelRoomRepository
{
    public HostelRoomRepository(ApplicationDbContext context) : base(context) { }

    public async Task<HostelRoom?> GetRoomWithDetailsAsync(Guid id)
    {
        return await _dbSet
            .Include(hr => hr.Hostel)
            .Include(hr => hr.Beds)
            .Include(hr => hr.Allocations)
                .ThenInclude(a => a.Student)
                    .ThenInclude(s => s!.User)
            .FirstOrDefaultAsync(hr => hr.Id == id);
    }

    public async Task<IReadOnlyList<HostelRoom>> GetAvailableRoomsAsync(Guid? schoolId)
    {
        var query = _dbSet
            .Include(hr => hr.Hostel)
            .Where(hr => hr.IsActive && hr.Occupied < hr.Capacity);

        if (schoolId.HasValue)
            query = query.Where(hr => hr.SchoolId == schoolId.Value);

        return await query
            .OrderBy(hr => hr.RoomNumber)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<HostelRoom>> GetAllRoomsWithDetailsAsync()
    {
        return await _dbSet
            .Include(hr => hr.Hostel)
            .Include(hr => hr.Beds)
            .OrderBy(hr => hr.RoomNumber)
            .ToListAsync();
    }
}

public class HostelRepository : GenericRepository<Hostel>, IHostelRepository
{
    public HostelRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Hostel?> GetHostelWithDetailsAsync(Guid id)
    {
        return await _dbSet
            .Include(h => h.Rooms)
                .ThenInclude(r => r.Beds)
            .FirstOrDefaultAsync(h => h.Id == id);
    }

    public async Task<IReadOnlyList<Hostel>> GetAllWithDetailsAsync()
    {
        return await _dbSet
            .Include(h => h.Rooms)
                .ThenInclude(r => r.Beds)
            .OrderBy(h => h.Name)
            .ToListAsync();
    }
}
