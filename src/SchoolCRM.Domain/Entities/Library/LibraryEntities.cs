using SchoolCRM.Domain.Common;
using SchoolCRM.Domain.Enums;

namespace SchoolCRM.Domain.Entities.Library;

public class BookCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid SchoolId { get; set; }

    public ICollection<Book> Books { get; set; } = new List<Book>();
}

public class Book : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string? Author { get; set; }
    public string? Publisher { get; set; }
    public string? Edition { get; set; }
    public int? PublicationYear { get; set; }
    public int TotalCopies { get; set; }
    public int AvailableCopies { get; set; }
    public decimal Price { get; set; }
    public string? ShelfNumber { get; set; }
    public string? RackNumber { get; set; }
    public string? Description { get; set; }
    public string? CoverImageUrl { get; set; }
    public BookStatus Status { get; set; } = BookStatus.Available;
    public Guid CategoryId { get; set; }
    public Guid SchoolId { get; set; }

    public BookCategory Category { get; set; } = null!;
    public ICollection<BookIssue> Issues { get; set; } = new List<BookIssue>();
}

public class BookIssue : BaseEntity
{
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public decimal Fine { get; set; }
    public bool IsReturned { get; set; }
    public string? IssuedBy { get; set; }
    public string? ReturnedTo { get; set; }
    public string? Remarks { get; set; }
    public Guid BookId { get; set; }
    public Guid? StudentId { get; set; }
    public Guid? TeacherId { get; set; }

    public Book Book { get; set; } = null!;
    public Student.Student? Student { get; set; }
    public Teacher.Teacher? Teacher { get; set; }
}
