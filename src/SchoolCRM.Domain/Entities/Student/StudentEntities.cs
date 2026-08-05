using SchoolCRM.Domain.Common;
using SchoolCRM.Domain.Enums;

namespace SchoolCRM.Domain.Entities.Student;

public class Student : BaseEntity
{
    public string AdmissionNumber { get; set; } = string.Empty;
    public string RollNumber { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public Guid SectionId { get; set; }
    public Guid SchoolId { get; set; }
    public DateTime AdmissionDate { get; set; }
    public StudentStatus Status { get; set; } = StudentStatus.Active;
    public string? PreviousSchool { get; set; }
    public string? TransferCertificateNumber { get; set; }
    public string? MedicalCondition { get; set; }
    public string? Allergies { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? TransportRoute { get; set; }
    public bool IsHostelResident { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankName { get; set; }

    public Identity.ApplicationUser User { get; set; } = null!;
    public School.Section Section { get; set; } = null!;
    public School.School School { get; set; } = null!;
    public Parent.Parent? Parent { get; set; }
    public Guid? ParentId { get; set; }

    public ICollection<StudentDocument> Documents { get; set; } = new List<StudentDocument>();
    public ICollection<StudentHealthRecord> HealthRecords { get; set; } = new List<StudentHealthRecord>();
    public ICollection<Attendance.Attendance> Attendances { get; set; } = new List<Attendance.Attendance>();
    public ICollection<Exam.Mark> Marks { get; set; } = new List<Exam.Mark>();
    public ICollection<Fee.FeeInstallment> FeeInstallments { get; set; } = new List<Fee.FeeInstallment>();
    public ICollection<Homework.HomeworkSubmission> HomeworkSubmissions { get; set; } = new List<Homework.HomeworkSubmission>();
    public ICollection<Library.BookIssue> BookIssues { get; set; } = new List<Library.BookIssue>();
    public ICollection<Transport.StudentTransportAllocation> TransportAllocations { get; set; } = new List<Transport.StudentTransportAllocation>();
    public ICollection<Hostel.HostelAllocation> HostelAllocations { get; set; } = new List<Hostel.HostelAllocation>();
}

public class StudentDocument : BaseEntity
{
    public string DocumentName { get; set; } = string.Empty;
    public DocumentType DocumentType { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public string? FileName { get; set; }
    public long? FileSize { get; set; }
    public Guid StudentId { get; set; }

    public Student Student { get; set; } = null!;
}

public class StudentHealthRecord : BaseEntity
{
    public decimal? Height { get; set; }
    public decimal? Weight { get; set; }
    public BloodGroup? BloodGroup { get; set; }
    public string? VisionLeft { get; set; }
    public string? VisionRight { get; set; }
    public string? MedicalConditions { get; set; }
    public string? Medications { get; set; }
    public string? Vaccinations { get; set; }
    public string? DoctorName { get; set; }
    public string? DoctorPhone { get; set; }
    public string? InsuranceProvider { get; set; }
    public string? InsurancePolicyNumber { get; set; }
    public DateTime RecordDate { get; set; }
    public string? Notes { get; set; }
    public Guid StudentId { get; set; }

    public Student Student { get; set; } = null!;
}

public class StudentPromotion : BaseEntity
{
    public Guid StudentId { get; set; }
    public Guid FromSectionId { get; set; }
    public Guid ToSectionId { get; set; }
    public Guid FromAcademicYearId { get; set; }
    public Guid ToAcademicYearId { get; set; }
    public string? Remarks { get; set; }
    public decimal? Percentage { get; set; }

    public Student Student { get; set; } = null!;
}

public class StudentTransfer : BaseEntity
{
    public Guid StudentId { get; set; }
    public TransferType TransferType { get; set; }
    public string? FromSchool { get; set; }
    public string? ToSchool { get; set; }
    public DateTime TransferDate { get; set; }
    public string? Reason { get; set; }
    public string? TransferCertificateNumber { get; set; }
    public string? Remarks { get; set; }

    public Student Student { get; set; } = null!;
}

public class StudentLeave : BaseEntity
{
    public Guid StudentId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? DocumentUrl { get; set; }
    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? Remarks { get; set; }

    public Student Student { get; set; } = null!;
}
