namespace SchoolCRM.Shared.Constants;

public static class ApplicationMessages
{
    public const string Success = "Operation completed successfully.";
    public const string Error = "An error occurred while processing your request.";
    public const string NotFound = "Resource not found.";
    public const string Unauthorized = "You are not authorized to perform this action.";
    public const string Forbidden = "You do not have permission to perform this action.";
    public const string ValidationFailed = "Validation failed. Please check your input.";
    public const string AlreadyExists = "A resource with the same information already exists.";
    public const string InvalidCredentials = "Invalid email or password.";
    public const string AccountLocked = "Your account has been locked. Please try again later.";
    public const string AccountDeactivated = "Your account has been deactivated.";
    public const string PasswordResetSent = "Password reset link has been sent to your email.";
    public const string PasswordResetSuccess = "Your password has been reset successfully.";
    public const string PasswordChangedSuccess = "Your password has been changed successfully.";
    public const string TokenRefreshFailed = "Invalid or expired refresh token.";
    public const string LogoutSuccess = "You have been logged out successfully.";
    public const string ProfileUpdated = "Your profile has been updated successfully.";
    public const string DeleteConfirm = "Are you sure you want to delete this resource?";
    public const string DeleteSuccess = "Resource deleted successfully.";
    public const string UpdateSuccess = "Resource updated successfully.";
    public const string CreateSuccess = "Resource created successfully.";
    public const string DuplicateRecord = "A record with the same details already exists.";
    public const string ForeignKeyViolation = "This record is referenced by other records and cannot be deleted.";
    public const string FileUploadSuccess = "File uploaded successfully.";
    public const string FileUploadFailed = "File upload failed.";
    public const string ExportSuccess = "Data exported successfully.";
    public const string ImportSuccess = "Data imported successfully.";
}

public static class CacheKeys
{
    public const string SchoolPrefix = "school_";
    public const string AcademicYearPrefix = "academic_year_";
    public const string ClassPrefix = "class_";
    public const string SectionPrefix = "section_";
    public const string SubjectPrefix = "subject_";
    public const string StudentPrefix = "student_";
    public const string TeacherPrefix = "teacher_";
    public const string ParentPrefix = "parent_";
    public const string EmployeePrefix = "employee_";
    public const string FeeStructurePrefix = "fee_structure_";
    public const string DashboardStats = "dashboard_stats_";
    public const string ReportCache = "report_";
    public const int DefaultExpirationMinutes = 30;
    public const int LongExpirationMinutes = 60;
    public const int ShortExpirationMinutes = 5;
}

public static class FileConstraints
{
    public const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10MB
    public const int MaxImageSizeBytes = 5 * 1024 * 1024; // 5MB
    public static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    public static readonly string[] AllowedDocumentExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt", ".csv" };
    public static readonly string[] AllowedExportExtensions = { ".pdf", ".xlsx", ".csv" };
}
