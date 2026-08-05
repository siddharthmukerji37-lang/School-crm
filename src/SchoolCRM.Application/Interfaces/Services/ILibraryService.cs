using SchoolCRM.Shared.Models;

namespace SchoolCRM.Application.Interfaces.Services;

public interface ILibraryService
{
    Task<ApiResponse<PagedResult<BookDto>>> GetBooksAsync(PaginationQuery query, string? category, string? author);

    Task<ApiResponse<BookDto>> GetBookByIdAsync(Guid id);

    Task<ApiResponse<BookDto>> CreateBookAsync(CreateBookDto dto);

    Task<ApiResponse<BookDto>> UpdateBookAsync(Guid id, CreateBookDto dto);

    Task<ApiResponse> DeleteBookAsync(Guid id);

    Task<ApiResponse<BookIssueDto>> IssueBookAsync(BookIssueDto dto);

    Task<ApiResponse<BookIssueDto>> ReturnBookAsync(Guid issueId, DateTime returnedDate);

    Task<ApiResponse<PagedResult<BookIssueDto>>> GetIssuedBooksAsync(
        PaginationQuery query, Guid? studentId, bool? overdue);

    Task<ApiResponse<List<BookIssueDto>>> GetStudentIssuesAsync(Guid studentId);

    public sealed class BookDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
        public string ShelfNumber { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public sealed class CreateBookDto
    {
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;
        public int TotalCopies { get; set; }
        public string ShelfNumber { get; set; } = string.Empty;
    }

    public sealed class BookIssueDto
    {
        public Guid Id { get; set; }
        public Guid BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnedDate { get; set; }
        public bool IsReturned { get; set; }
        public decimal? FineAmount { get; set; }
    }
}
