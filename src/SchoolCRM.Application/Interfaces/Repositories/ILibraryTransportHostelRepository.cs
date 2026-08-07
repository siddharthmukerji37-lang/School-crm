using SchoolCRM.Domain.Entities.Library;
using SchoolCRM.Domain.Entities.Transport;
using SchoolCRM.Domain.Entities.Hostel;

namespace SchoolCRM.Application.Interfaces.Repositories;

public interface IBookRepository : IGenericRepository<Book>
{
    Task<Book?> GetBookWithDetailsAsync(Guid id);
    Task<IReadOnlyList<Book>> SearchBooksAsync(string searchTerm, Guid? categoryId, Guid? schoolId);
    Task<IReadOnlyList<Book>> GetAvailableBooksAsync(Guid? schoolId);
}

public interface IBookIssueRepository : IGenericRepository<BookIssue>
{
    Task<IReadOnlyList<BookIssue>> GetByStudentAsync(Guid studentId);
    Task<IReadOnlyList<BookIssue>> GetOverdueBooksAsync();
    Task<bool> HasActiveIssueAsync(Guid bookId, Guid studentId);
    Task<(IReadOnlyList<BookIssue> Items, int TotalCount)> GetIssuedPagedAsync(
        int pageNumber, int pageSize,
        System.Linq.Expressions.Expression<Func<BookIssue, bool>>? filter = null);
}

public interface ITransportRouteRepository : IGenericRepository<TransportRoute>
{
    Task<TransportRoute?> GetRouteWithDetailsAsync(Guid id);
    Task<IReadOnlyList<Vehicle>> GetVehiclesWithDetailsAsync();
    Task<IReadOnlyList<StudentTransportAllocation>> GetAllocationsWithDetailsAsync();
}

public interface IHostelRoomRepository : IGenericRepository<HostelRoom>
{
    Task<HostelRoom?> GetRoomWithDetailsAsync(Guid id);
    Task<IReadOnlyList<HostelRoom>> GetAvailableRoomsAsync(Guid? schoolId);
}
