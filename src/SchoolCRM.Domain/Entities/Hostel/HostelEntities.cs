using SchoolCRM.Domain.Common;
using SchoolCRM.Domain.Enums;

namespace SchoolCRM.Domain.Entities.Hostel;

public class HostelRoom : BaseEntity
{
    public string RoomNumber { get; set; } = string.Empty;
    public string RoomType { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int Occupied { get; set; }
    public decimal MonthlyFee { get; set; }
    public string? Floor { get; set; }
    public string? Building { get; set; }
    public bool HasAC { get; set; }
    public bool HasWifi { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid SchoolId { get; set; }

    public ICollection<HostelBed> Beds { get; set; } = new List<HostelBed>();
    public ICollection<HostelAllocation> Allocations { get; set; } = new List<HostelAllocation>();
}

public class HostelBed : BaseEntity
{
    public string BedNumber { get; set; } = string.Empty;
    public bool IsOccupied { get; set; }
    public Guid RoomId { get; set; }

    public HostelRoom Room { get; set; } = null!;
}

public class HostelAllocation : BaseEntity
{
    public Guid StudentId { get; set; }
    public Guid RoomId { get; set; }
    public Guid? BedId { get; set; }
    public DateTime AllocationDate { get; set; }
    public DateTime? CheckOutDate { get; set; }
    public HostelAllocationStatus Status { get; set; } = HostelAllocationStatus.Active;
    public string? Remarks { get; set; }

    public Student.Student Student { get; set; } = null!;
    public HostelRoom Room { get; set; } = null!;
    public HostelBed? Bed { get; set; }
}

public class HostelVisitor : BaseEntity
{
    public string VisitorName { get; set; } = string.Empty;
    public string? Relationship { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? IdProof { get; set; }
    public DateTime VisitDate { get; set; }
    public TimeSpan ArrivalTime { get; set; }
    public TimeSpan? DepartureTime { get; set; }
    public string? Purpose { get; set; }
    public Guid StudentId { get; set; }
    public Guid RoomId { get; set; }
    public string? MeetingWith { get; set; }

    public Student.Student Student { get; set; } = null!;
    public HostelRoom Room { get; set; } = null!;
}
