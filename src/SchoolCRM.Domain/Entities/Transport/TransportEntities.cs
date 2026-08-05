using SchoolCRM.Domain.Common;
using SchoolCRM.Domain.Enums;

namespace SchoolCRM.Domain.Entities.Transport;

public class TransportRoute : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public decimal Distance { get; set; }
    public TimeSpan? MorningPickupTime { get; set; }
    public TimeSpan? EveningDropTime { get; set; }
    public decimal MonthlyFee { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }
    public Guid SchoolId { get; set; }

    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    public ICollection<PickupPoint> PickupPoints { get; set; } = new List<PickupPoint>();
    public ICollection<StudentTransportAllocation> Allocations { get; set; } = new List<StudentTransportAllocation>();
}

public class Vehicle : BaseEntity
{
    public string VehicleNumber { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string? Model { get; set; }
    public string? Color { get; set; }
    public DateTime? InsuranceExpiry { get; set; }
    public DateTime? PUCExpiry { get; set; }
    public bool IsActive { get; set; } = true;
    public string? GpsDeviceId { get; set; }
    public Guid DriverId { get; set; }
    public Guid RouteId { get; set; }

    public Driver Driver { get; set; } = null!;
    public TransportRoute Route { get; set; } = null!;
}

public class Driver : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? LicenseNumber { get; set; }
    public DateTime? LicenseExpiry { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid SchoolId { get; set; }

    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}

public class PickupPoint : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public TimeSpan? MorningTime { get; set; }
    public TimeSpan? EveningTime { get; set; }
    public Guid RouteId { get; set; }

    public TransportRoute Route { get; set; } = null!;
}

public class StudentTransportAllocation : BaseEntity
{
    public Guid StudentId { get; set; }
    public Guid RouteId { get; set; }
    public Guid? PickupPointId { get; set; }
    public TransportAllocationStatus Status { get; set; } = TransportAllocationStatus.Active;
    public string? PickupType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public Student.Student Student { get; set; } = null!;
    public TransportRoute Route { get; set; } = null!;
    public PickupPoint? PickupPoint { get; set; }
}
