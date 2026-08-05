using System.ComponentModel.DataAnnotations;

namespace SchoolCRM.Application.DTOs.Common;

public sealed class IdNameDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
}

public sealed class FileUploadDto
{
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? FilePath { get; set; }
    public DateTime UploadedAt { get; set; }
    public string UploadedBy { get; set; } = string.Empty;
}

public sealed class ExportRequestDto
{
    [Required(ErrorMessage = "Export type is required")]
    public string ExportType { get; set; } = string.Empty;

    [Required(ErrorMessage = "Format is required")]
    [RegularExpression("^(pdf|excel|csv)$", ErrorMessage = "Format must be pdf, excel, or csv")]
    public string Format { get; set; } = "pdf";

    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public Guid? SchoolId { get; set; }
    public Guid? ClassRoomId { get; set; }
    public Guid? SectionId { get; set; }
    public Dictionary<string, string>? Filters { get; set; }
    public List<string>? Columns { get; set; }
}

public sealed class ApiResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
}

public sealed class DropdownItemDto
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? Group { get; set; }
    public bool IsDisabled { get; set; }
}

public sealed class LookupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? Description { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}

public sealed class DateRangeDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public sealed class BulkActionDto<T>
{
    public List<Guid> Ids { get; set; } = new();
    public T? Payload { get; set; }
}
