using System.Linq.Expressions;
using SchoolCRM.Application.Interfaces.Repositories;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.Domain.Entities.Library;
using SchoolCRM.Domain.Enums;
using SchoolCRM.Infrastructure.Data;
using SchoolCRM.Shared.Constants;
using SchoolCRM.Shared.Models;
using static SchoolCRM.Application.Interfaces.Services.ILibraryService;

namespace SchoolCRM.Infrastructure.Services;

public class LibraryService : ILibraryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public LibraryService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<PagedResult<BookDto>>> GetBooksAsync(PaginationQuery query, string? category, string? author)
    {
        try
        {
            var books = await _unitOfWork.Books.SearchBooksAsync(query.SearchTerm, null, null);
            var totalCount = books.Count;

            var pagedItems = books
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(b => new BookDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Author = b.Author ?? string.Empty,
                    ISBN = b.ISBN,
                    Category = b.Category?.Name ?? string.Empty,
                    Publisher = b.Publisher ?? string.Empty,
                    TotalCopies = b.TotalCopies,
                    AvailableCopies = b.AvailableCopies,
                    ShelfNumber = b.ShelfNumber ?? string.Empty,
                    Description = b.Description,
                    IsActive = !b.IsDeleted
                }).ToList();

            var pagedResult = new PagedResult<BookDto>
            {
                Items = pagedItems,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };

            return ApiResponse<PagedResult<BookDto>>.SuccessResponse(pagedResult);
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResult<BookDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<BookDto>> GetBookByIdAsync(Guid id)
    {
        try
        {
            var book = await _unitOfWork.Books.GetBookWithDetailsAsync(id);
            if (book is null)
                return ApiResponse<BookDto>.NotFoundResponse(ApplicationMessages.NotFound);

            return ApiResponse<BookDto>.SuccessResponse(new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author ?? string.Empty,
                ISBN = book.ISBN,
                Category = book.Category?.Name ?? string.Empty,
                Publisher = book.Publisher ?? string.Empty,
                TotalCopies = book.TotalCopies,
                AvailableCopies = book.AvailableCopies,
                ShelfNumber = book.ShelfNumber ?? string.Empty,
                Description = book.Description,
                IsActive = !book.IsDeleted
            });
        }
        catch (Exception ex)
        {
            return ApiResponse<BookDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<BookDto>> CreateBookAsync(CreateBookDto dto)
    {
        try
        {
            var schoolId = _currentUserService.SchoolId;
            if (schoolId is null || schoolId == Guid.Empty)
            {
                var schools = await _unitOfWork.Schools.GetAllAsync();
                schoolId = schools.FirstOrDefault()?.Id;
            }

            if (schoolId is null || schoolId == Guid.Empty)
                return ApiResponse<BookDto>.FailResponse("Unable to determine the current school context. Please sign in again.");

            var categoryId = await ResolveCategoryAsync(dto.Category, schoolId.Value);

            var book = new Book
            {
                Title = dto.Title,
                Author = dto.Author,
                ISBN = dto.ISBN,
                Publisher = dto.Publisher,
                TotalCopies = dto.TotalCopies,
                AvailableCopies = dto.AvailableCopies > 0 ? dto.AvailableCopies : dto.TotalCopies,
                ShelfNumber = dto.ShelfNumber,
                Description = dto.Description,
                Status = BookStatus.Available,
                CategoryId = categoryId,
                SchoolId = schoolId.Value,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Books.AddAsync(book);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<BookDto>.SuccessResponse(new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author ?? string.Empty,
                ISBN = book.ISBN,
                Category = book.Category?.Name ?? string.Empty,
                Publisher = book.Publisher ?? string.Empty,
                TotalCopies = book.TotalCopies,
                AvailableCopies = book.AvailableCopies,
                ShelfNumber = book.ShelfNumber ?? string.Empty,
                Description = book.Description,
                IsActive = true

            }, ApplicationMessages.CreateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse<BookDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<BookDto>> UpdateBookAsync(Guid id, CreateBookDto dto)
    {
        try
        {
            var book = await _unitOfWork.Books.GetByIdAsync(id);
            if (book is null)
                return ApiResponse<BookDto>.NotFoundResponse(ApplicationMessages.NotFound);

            book.Title = dto.Title;
            book.Author = dto.Author;
            book.ISBN = dto.ISBN;
            book.Publisher = dto.Publisher;
            book.TotalCopies = dto.TotalCopies;
            book.AvailableCopies = dto.AvailableCopies > 0 ? dto.AvailableCopies : dto.TotalCopies;
            book.ShelfNumber = dto.ShelfNumber;
            book.Description = dto.Description;
            book.CategoryId = await ResolveCategoryAsync(dto.Category, book.SchoolId);
            book.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Books.UpdateAsync(book);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<BookDto>.SuccessResponse(new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author ?? string.Empty,
                ISBN = book.ISBN,
                Category = book.Category?.Name ?? string.Empty,
                Publisher = book.Publisher ?? string.Empty,
                TotalCopies = book.TotalCopies,
                AvailableCopies = book.AvailableCopies,
                ShelfNumber = book.ShelfNumber ?? string.Empty,
                Description = book.Description,
                IsActive = true
            }, ApplicationMessages.UpdateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse<BookDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse> DeleteBookAsync(Guid id)
    {
        try
        {
            var book = await _unitOfWork.Books.GetByIdAsync(id);
            if (book is null)
                return ApiResponse.FailResponse(ApplicationMessages.NotFound);

            book.IsDeleted = true;
            book.DeletedAt = DateTime.UtcNow;
            await _unitOfWork.Books.UpdateAsync(book);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.SuccessResponse(ApplicationMessages.DeleteSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<BookIssueDto>> IssueBookAsync(BookIssueDto dto)
    {
        try
        {
            if (!dto.StudentId.HasValue && !dto.TeacherId.HasValue)
                return ApiResponse<BookIssueDto>.FailResponse("Select a student or teacher to issue the book.");

            var hasActive = await _unitOfWork.BookIssues.HasActiveIssueAsync(
                dto.BookId, dto.StudentId, dto.TeacherId);
            if (hasActive)
                return ApiResponse<BookIssueDto>.FailResponse(dto.StudentId.HasValue
                    ? "Student already has an active issue for this book."
                    : "Teacher already has an active issue for this book.");

            var book = await _unitOfWork.Books.GetByIdAsync(dto.BookId);
            if (book is null || book.AvailableCopies <= 0)
                return ApiResponse<BookIssueDto>.FailResponse("Book is not available.");

            var issue = new Domain.Entities.Library.BookIssue
            {
                BookId = dto.BookId,
                StudentId = dto.StudentId,
                TeacherId = dto.TeacherId,
                IssueDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(14),
                IsReturned = false,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.BookIssues.AddAsync(issue);

            book.AvailableCopies--;
            book.Status = book.AvailableCopies == 0 ? BookStatus.Issued : BookStatus.Available;
            await _unitOfWork.Books.UpdateAsync(book);

            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<BookIssueDto>.SuccessResponse(new BookIssueDto
            {
                Id = issue.Id,
                BookId = issue.BookId,
                BookTitle = book.Title,
                StudentId = issue.StudentId,
                TeacherId = issue.TeacherId,
                IssueDate = issue.IssueDate,
                DueDate = issue.DueDate,
                IsReturned = false
            }, ApplicationMessages.CreateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse<BookIssueDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<BookIssueDto>> ReturnBookAsync(Guid issueId, DateTime returnedDate)
    {
        try
        {
            var issue = await _unitOfWork.BookIssues.GetByIdAsync(issueId);
            if (issue is null)
                return ApiResponse<BookIssueDto>.NotFoundResponse(ApplicationMessages.NotFound);

            issue.ReturnDate = returnedDate;
            issue.IsReturned = true;
            issue.UpdatedAt = DateTime.UtcNow;

            if (returnedDate > issue.DueDate)
                issue.Fine = (decimal)(returnedDate - issue.DueDate).TotalDays * 5;

            var book = await _unitOfWork.Books.GetByIdAsync(issue.BookId);
            if (book is not null)
            {
                book.AvailableCopies++;
                book.Status = BookStatus.Available;
                await _unitOfWork.Books.UpdateAsync(book);
            }

            await _unitOfWork.BookIssues.UpdateAsync(issue);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<BookIssueDto>.SuccessResponse(new BookIssueDto
            {
                Id = issue.Id,
                BookId = issue.BookId,
                BookTitle = book?.Title ?? string.Empty,
                StudentId = issue.StudentId,
                TeacherId = issue.TeacherId,
                IssueDate = issue.IssueDate,
                DueDate = issue.DueDate,
                ReturnedDate = issue.ReturnDate,
                IsReturned = true,
                FineAmount = issue.Fine > 0 ? issue.Fine : null
            }, ApplicationMessages.UpdateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse<BookIssueDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<PagedResult<BookIssueDto>>> GetIssuedBooksAsync(
        PaginationQuery query, Guid? studentId, Guid? teacherId, bool? overdue)
    {
        try
        {
            Expression<Func<Domain.Entities.Library.BookIssue, bool>>? filter = i => !i.IsDeleted;
            if (studentId.HasValue)
                filter = i => !i.IsDeleted && i.StudentId == studentId.Value;
            else if (teacherId.HasValue)
                filter = i => !i.IsDeleted && i.TeacherId == teacherId.Value;

            var (items, totalCount) = await _unitOfWork.BookIssues.GetIssuedPagedAsync(
                query.PageNumber, query.PageSize, filter);

            var dtos = items.Select(i => new BookIssueDto
            {
                Id = i.Id,
                BookId = i.BookId,
                BookTitle = i.Book?.Title ?? string.Empty,
                StudentId = i.StudentId,
                StudentName = i.Student?.User is not null
                    ? $"{i.Student.User.FirstName} {i.Student.User.LastName}"
                    : string.Empty,
                TeacherId = i.TeacherId,
                TeacherName = i.Teacher?.User is not null
                    ? $"{i.Teacher.User.FirstName} {i.Teacher.User.LastName}"
                    : string.Empty,
                IssueDate = i.IssueDate,
                DueDate = i.DueDate,
                ReturnedDate = i.ReturnDate,
                IsReturned = i.IsReturned,
                FineAmount = i.Fine > 0 ? i.Fine : null
            }).ToList();

            var pagedResult = new PagedResult<BookIssueDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };

            return ApiResponse<PagedResult<BookIssueDto>>.SuccessResponse(pagedResult);
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResult<BookIssueDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<BookIssueDto>>> GetStudentIssuesAsync(Guid studentId)
    {
        try
        {
            var issues = await _unitOfWork.BookIssues.GetByStudentAsync(studentId);
            var dtos = issues.Select(i => new BookIssueDto
            {
                Id = i.Id,
                BookId = i.BookId,
                BookTitle = i.Book?.Title ?? string.Empty,
                StudentId = i.StudentId,
                StudentName = i.Student?.User is not null
                    ? $"{i.Student.User.FirstName} {i.Student.User.LastName}"
                    : string.Empty,
                IssueDate = i.IssueDate,
                DueDate = i.DueDate,
                ReturnedDate = i.ReturnDate,
                IsReturned = i.IsReturned,
                FineAmount = i.Fine > 0 ? i.Fine : null
            }).ToList();

            return ApiResponse<List<BookIssueDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<BookIssueDto>>.FailResponse(ex.Message);
        }
    }

    private async Task<Guid> ResolveCategoryAsync(string categoryName, Guid schoolId)
    {
        if (string.IsNullOrWhiteSpace(categoryName))
            categoryName = "Other";

        var category = (await _unitOfWork.Repository<BookCategory>()
                .FindAsync(c => c.SchoolId == schoolId && c.Name == categoryName))
            .FirstOrDefault();

        if (category is not null)
            return category.Id;

        var newCategory = new BookCategory
        {
            Id = Guid.NewGuid(),
            Name = categoryName,
            Code = $"CAT{Math.Abs(categoryName.GetHashCode()) % 1000:D3}",
            SchoolId = schoolId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<BookCategory>().AddAsync(newCategory);
        return newCategory.Id;
    }
}
