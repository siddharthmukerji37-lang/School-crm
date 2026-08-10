using SchoolCRM.Shared.Models;

namespace SchoolCRM.Application.Interfaces.Services;

public interface IHostelService
{
    Task<ApiResponse<PagedResult<HostelDto>>> GetHostelsAsync(PaginationQuery query, Guid? schoolId);

    Task<ApiResponse<HostelDto>> GetHostelByIdAsync(Guid id);

    Task<ApiResponse<HostelDto>> CreateHostelAsync(HostelDto dto);

    Task<ApiResponse<HostelDto>> UpdateHostelAsync(Guid id, HostelDto dto);

    Task<ApiResponse> DeleteHostelAsync(Guid id);

    Task<ApiResponse<List<RoomDto>>> GetRoomsAsync(Guid? hostelId);

    Task<ApiResponse<RoomDto>> CreateRoomAsync(CreateRoomDto dto);

    Task<ApiResponse<RoomDto>> UpdateRoomAsync(Guid id, CreateRoomDto dto);

    Task<ApiResponse> DeleteRoomAsync(Guid id);

    Task<ApiResponse<List<BedDto>>> GetBedsAsync(Guid roomId);

    Task<ApiResponse> AllocateBedAsync(BedAllocationDto dto);

    Task<ApiResponse> DeallocateBedAsync(Guid allocationId);

    Task<ApiResponse<PagedResult<BedAllocationDto>>> GetAllocationsAsync(
        PaginationQuery query, Guid? hostelId);

    public sealed class HostelDto
    {
        public Guid Id { get; set; }
        public Guid SchoolId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string WardenName { get; set; } = string.Empty;
        public string WardenPhone { get; set; } = string.Empty;
        public int TotalRooms { get; set; }
        public int TotalBeds { get; set; }
        public bool IsActive { get; set; }
    }

    public sealed class RoomDto
    {
        public Guid Id { get; set; }
        public Guid HostelId { get; set; }
        public string HostelName { get; set; } = string.Empty;
        public string RoomNumber { get; set; } = string.Empty;
        public string RoomType { get; set; } = string.Empty;
        public int TotalBeds { get; set; }
        public int AvailableBeds { get; set; }
        public decimal MonthlyFee { get; set; }
        public bool IsActive { get; set; }
    }

    public sealed class CreateRoomDto
    {
        public Guid HostelId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string RoomType { get; set; } = string.Empty;
        public int TotalBeds { get; set; }
        public decimal MonthlyFee { get; set; }
    }

    public sealed class BedDto
    {
        public Guid Id { get; set; }
        public Guid RoomId { get; set; }
        public string BedNumber { get; set; } = string.Empty;
        public bool IsOccupied { get; set; }
    }

    public sealed class BedAllocationDto
    {
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public Guid BedId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string HostelName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime AllocationDate { get; set; }
        public DateTime? DeallocationDate { get; set; }
        public bool IsActive { get; set; }
    }
}
