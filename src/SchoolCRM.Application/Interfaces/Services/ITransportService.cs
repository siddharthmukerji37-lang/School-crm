using SchoolCRM.Shared.Models;

namespace SchoolCRM.Application.Interfaces.Services;

public interface ITransportService
{
    Task<ApiResponse<PagedResult<RouteDto>>> GetRoutesAsync(PaginationQuery query, Guid? schoolId);

    Task<ApiResponse<RouteDto>> GetRouteByIdAsync(Guid id);

    Task<ApiResponse<RouteDto>> CreateRouteAsync(CreateRouteDto dto);

    Task<ApiResponse<RouteDto>> UpdateRouteAsync(Guid id, CreateRouteDto dto);

    Task<ApiResponse> DeleteRouteAsync(Guid id);

    Task<ApiResponse<PagedResult<VehicleDto>>> GetVehiclesAsync(PaginationQuery query, Guid? schoolId);

    Task<ApiResponse<VehicleDto>> GetVehicleByIdAsync(Guid id);

    Task<ApiResponse<VehicleDto>> CreateVehicleAsync(CreateVehicleDto dto);

    Task<ApiResponse<VehicleDto>> UpdateVehicleAsync(Guid id, CreateVehicleDto dto);

    Task<ApiResponse> DeleteVehicleAsync(Guid id);

    Task<ApiResponse> AllocateTransportAsync(TransportAllocationDto dto);

    Task<ApiResponse> DeallocateTransportAsync(Guid allocationId);

    Task<ApiResponse<PagedResult<TransportAllocationDto>>> GetAllocationsAsync(
        PaginationQuery query, Guid? routeId, Guid? vehicleId);

    public sealed class RouteDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string StartPoint { get; set; } = string.Empty;
        public string EndPoint { get; set; } = string.Empty;
        public decimal Distance { get; set; }
        public decimal MonthlyFee { get; set; }
        public string Stops { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public sealed class CreateRouteDto
    {
        public string Name { get; set; } = string.Empty;
        public string StartPoint { get; set; } = string.Empty;
        public string EndPoint { get; set; } = string.Empty;
        public decimal Distance { get; set; }
        public decimal MonthlyFee { get; set; }
        public string Stops { get; set; } = string.Empty;
    }

    public sealed class VehicleDto
    {
        public Guid Id { get; set; }
        public string RegistrationNumber { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public string DriverName { get; set; } = string.Empty;
        public string DriverPhone { get; set; } = string.Empty;
        public Guid RouteId { get; set; }
        public string RouteName { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public bool IsActive { get; set; }
    }

    public sealed class CreateVehicleDto
    {
        public string RegistrationNumber { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public string DriverName { get; set; } = string.Empty;
        public string DriverPhone { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public Guid RouteId { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public sealed class TransportAllocationDto
    {
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public Guid RouteId { get; set; }
        public string RouteName { get; set; } = string.Empty;
        public string PickUpPoint { get; set; } = string.Empty;
        public decimal MonthlyFee { get; set; }
        public bool IsActive { get; set; }
    }
}
