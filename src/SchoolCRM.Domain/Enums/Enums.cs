namespace SchoolCRM.Domain.Enums;

public enum RoleType
{
    SuperAdmin = 1,
    SchoolAdmin = 2,
    Principal = 3,
    VicePrincipal = 4,
    Teacher = 5,
    ClassTeacher = 6,
    Accountant = 7,
    Receptionist = 8,
    Librarian = 9,
    Student = 10,
    Parent = 11
}

public enum Gender
{
    Male = 1,
    Female = 2,
    Other = 3
}

public enum MaritalStatus
{
    Single = 1,
    Married = 2,
    Divorced = 3,
    Widowed = 4
}

public enum BloodGroup
{
    APositive = 1,
    ANegative = 2,
    BPositive = 3,
    BNegative = 4,
    OPositive = 5,
    ONegative = 6,
    ABPositive = 7,
    ABNegative = 8
}

public static class BloodGroupExtensions
{
    private static readonly Dictionary<string, BloodGroup> Map = new(StringComparer.OrdinalIgnoreCase)
    {
        ["A+"] = BloodGroup.APositive,
        ["A-"] = BloodGroup.ANegative,
        ["B+"] = BloodGroup.BPositive,
        ["B-"] = BloodGroup.BNegative,
        ["O+"] = BloodGroup.OPositive,
        ["O-"] = BloodGroup.ONegative,
        ["AB+"] = BloodGroup.ABPositive,
        ["AB-"] = BloodGroup.ABNegative,
    };

    public static BloodGroup? ParseBloodGroup(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (Map.TryGetValue(value, out var bloodGroup))
            return bloodGroup;

        if (Enum.TryParse<BloodGroup>(value, ignoreCase: true, out var parsed))
            return parsed;

        return null;
    }

    public static string? ToDisplayString(this BloodGroup? value)
    {
        return value switch
        {
            BloodGroup.APositive => "A+",
            BloodGroup.ANegative => "A-",
            BloodGroup.BPositive => "B+",
            BloodGroup.BNegative => "B-",
            BloodGroup.OPositive => "O+",
            BloodGroup.ONegative => "O-",
            BloodGroup.ABPositive => "AB+",
            BloodGroup.ABNegative => "AB-",
            _ => null
        };
    }
}

public enum AttendanceStatus
{
    Present = 1,
    Absent = 2,
    Late = 3,
    Excused = 4,
    HalfDay = 5,
    Leave = 6
}

public enum ApplicableUserType
{
    Teacher = 1,
    Employee = 2,
    Both = 3
}

public enum StudentStatus
{
    Active = 1,
    Inactive = 2,
    Graduated = 3,
    Transferred = 4,
    Expelled = 5
}

public enum FeeStatus
{
    Pending = 1,
    Partial = 2,
    Paid = 3,
    Overdue = 4,
    Waived = 5
}

public enum PaymentMethod
{
    Cash = 1,
    BankTransfer = 2,
    Card = 3,
    Online = 4,
    Cheque = 5
}

public enum ExamStatus
{
    Scheduled = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4
}

public enum BookStatus
{
    Available = 1,
    Issued = 2,
    Lost = 3,
    Damaged = 4,
    Retired = 5
}

public enum NotificationType
{
    Info = 1,
    Success = 2,
    Warning = 3,
    Error = 4,
    Announcement = 5
}

public enum TransactionType
{
    Income = 1,
    Expense = 2
}

public enum ChatMessageType
{
    Direct = 1,
    Class = 2
}

public enum HostelAllocationStatus
{
    Active = 1,
    CheckedOut = 2,
    Pending = 3
}

public enum TransportAllocationStatus
{
    Active = 1,
    Inactive = 2,
    Pending = 3
}

public enum EmployeeStatus
{
    Active = 1,
    OnLeave = 2,
    Terminated = 3,
    Resigned = 4
}

public enum TeacherStatus
{
    Active = 1,
    OnLeave = 2,
    Inactive = 3
}

public enum DocumentType
{
    AadharCard = 1,
    PanCard = 2,
    Passport = 3,
    Marksheet = 4,
    Certificate = 5,
    Photo = 6,
    Signature = 7,
    Other = 8
}

public enum HomeworkStatus
{
    Assigned = 1,
    Submitted = 2,
    Reviewed = 3,
    Rejected = 4,
    Completed = 5
}

public enum LeaveStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Cancelled = 4
}

public enum TransferType
{
    Incoming = 1,
    Outgoing = 2
}

public enum ReportFormat
{
    Pdf = 1,
    Excel = 2,
    Csv = 3
}

public enum AuditAction
{
    Create = 1,
    Update = 2,
    Delete = 3,
    Login = 4,
    Logout = 5,
    Export = 6,
    Import = 7
}

public enum QuestionType
{
    MCQ = 1,
    Descriptive = 2
}

public enum ApprovalStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3
}

public enum GradingStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3
}

public enum DeductionType
{
    FixedAmount = 1,
    Percentage = 2,
    PerDay = 3
}

public enum SalaryDeductionStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Applied = 4,
    Cancelled = 5
}
