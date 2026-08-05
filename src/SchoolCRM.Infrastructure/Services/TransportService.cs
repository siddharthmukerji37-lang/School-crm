using System.Linq.Expressions;
using SchoolCRM.Application.Interfaces.Repositories;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.Domain.Enums;
using SchoolCRM.Shared.Constants;
using SchoolCRM.Shared.Models;
using static SchoolCRM.Application.Interfaces.Services.ITransportService;

namespace SchoolCRM.Infrastructure.Services;

public class TransportService : ITransportService
{
    private readonly IUnitOfWork _unitOfWork;

    public TransportService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<PagedResult<RouteDto>>> GetRoutesAsync(PaginationQuery query, Guid? schoolId)
    {
        try
        {
            var routes = await _unitOfWork.TransportRoutes.GetAllAsync();
            var filtered = routes.Where(r => !r.IsDeleted).ToList();
            var totalCount = filtered.Count;

            var pagedItems = filtered
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(r => new RouteDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Distance = r.Distance,
                    MonthlyFee = r.MonthlyFee,
                    IsActive = r.IsActive
                }).ToList();

            return ApiResponse<PagedResult<RouteDto>>.SuccessResponse(new PagedResult<RouteDto>
            {
                Items = pagedItems,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            });
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResult<RouteDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<RouteDto>> GetRouteByIdAsync(Guid id)
    {
        try
        {
            var route = await _unitOfWork.TransportRoutes.GetRouteWithDetailsAsync(id);
            if (route is null)
                return ApiResponse<RouteDto>.NotFoundResponse(ApplicationMessages.NotFound);

            return ApiResponse<RouteDto>.SuccessResponse(new RouteDto
            {
                Id = route.Id,
                Name = route.Name,
                Distance = route.Distance,
                MonthlyFee = route.MonthlyFee,
                IsActive = route.IsActive
            });
        }
        catch (Exception ex)
        {
            return ApiResponse<RouteDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<RouteDto>> CreateRouteAsync(CreateRouteDto dto)
    {
        try
        {
            var route = new Domain.Entities.Transport.TransportRoute
            {
                Name = dto.Name,
                Code = dto.Name[..Math.Min(3, dto.Name.Length)].ToUpper(),
                Distance = dto.Distance,
                MonthlyFee = dto.MonthlyFee,
                IsActive = true,
                SchoolId = Guid.Empty,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.TransportRoutes.AddAsync(route);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<RouteDto>.SuccessResponse(new RouteDto
            {
                Id = route.Id,
                Name = route.Name,
                Distance = route.Distance,
                MonthlyFee = route.MonthlyFee,
                IsActive = route.IsActive
            }, ApplicationMessages.CreateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse<RouteDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<RouteDto>> UpdateRouteAsync(Guid id, CreateRouteDto dto)
    {
        try
        {
            var route = await _unitOfWork.TransportRoutes.GetByIdAsync(id);
            if (route is null)
                return ApiResponse<RouteDto>.NotFoundResponse(ApplicationMessages.NotFound);

            route.Name = dto.Name;
            route.Distance = dto.Distance;
            route.MonthlyFee = dto.MonthlyFee;
            route.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.TransportRoutes.UpdateAsync(route);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<RouteDto>.SuccessResponse(new RouteDto
            {
                Id = route.Id,
                Name = route.Name,
                Distance = route.Distance,
                MonthlyFee = route.MonthlyFee,
                IsActive = route.IsActive
            }, ApplicationMessages.UpdateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse<RouteDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse> DeleteRouteAsync(Guid id)
    {
        try
        {
            var route = await _unitOfWork.TransportRoutes.GetByIdAsync(id);
            if (route is null)
                return ApiResponse.FailResponse(ApplicationMessages.NotFound);

            route.IsDeleted = true;
            route.DeletedAt = DateTime.UtcNow;
            await _unitOfWork.TransportRoutes.UpdateAsync(route);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.SuccessResponse(ApplicationMessages.DeleteSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<PagedResult<VehicleDto>>> GetVehiclesAsync(PaginationQuery query, Guid? schoolId)
    {
        try
        {
            var vehicles = await _unitOfWork.Repository<Domain.Entities.Transport.Vehicle>().GetAllAsync();
            var filtered = vehicles.Where(v => !v.IsDeleted).ToList();
            var totalCount = filtered.Count;

            var pagedItems = filtered
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(v => new VehicleDto
                {
                    Id = v.Id,
                    RegistrationNumber = v.VehicleNumber,
                    VehicleType = v.VehicleType,
                    DriverName = v.Driver?.Name ?? string.Empty,
                    DriverPhone = v.Driver?.Phone ?? string.Empty,
                    Capacity = v.Capacity,
                    IsActive = v.IsActive
                }).ToList();

            return ApiResponse<PagedResult<VehicleDto>>.SuccessResponse(new PagedResult<VehicleDto>
            {
                Items = pagedItems,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            });
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResult<VehicleDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<VehicleDto>> GetVehicleByIdAsync(Guid id)
    {
        try
        {
            var vehicle = await _unitOfWork.Repository<Domain.Entities.Transport.Vehicle>().GetByIdAsync(id);
            if (vehicle is null)
                return ApiResponse<VehicleDto>.NotFoundResponse(ApplicationMessages.NotFound);

            return ApiResponse<VehicleDto>.SuccessResponse(new VehicleDto
            {
                Id = vehicle.Id,
                RegistrationNumber = vehicle.VehicleNumber,
                VehicleType = vehicle.VehicleType,
                DriverName = vehicle.Driver?.Name ?? string.Empty,
                DriverPhone = vehicle.Driver?.Phone ?? string.Empty,
                Capacity = vehicle.Capacity,
                IsActive = vehicle.IsActive
            });
        }
        catch (Exception ex)
        {
            return ApiResponse<VehicleDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<VehicleDto>> CreateVehicleAsync(CreateVehicleDto dto)
    {
        try
        {
            var driver = new Domain.Entities.Transport.Driver
            {
                Name = dto.DriverName,
                Phone = dto.DriverPhone,
                SchoolId = Guid.Empty,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Repository<Domain.Entities.Transport.Driver>().AddAsync(driver);
            await _unitOfWork.SaveChangesAsync();

            var vehicle = new Domain.Entities.Transport.Vehicle
            {
                VehicleNumber = dto.RegistrationNumber,
                VehicleType = dto.VehicleType,
                Capacity = dto.Capacity,
                DriverId = driver.Id,
                RouteId = Guid.Empty,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<Domain.Entities.Transport.Vehicle>().AddAsync(vehicle);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<VehicleDto>.SuccessResponse(new VehicleDto
            {
                Id = vehicle.Id,
                RegistrationNumber = vehicle.VehicleNumber,
                VehicleType = vehicle.VehicleType,
                DriverName = driver.Name,
                DriverPhone = driver.Phone,
                Capacity = vehicle.Capacity,
                IsActive = true
            }, ApplicationMessages.CreateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse<VehicleDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<VehicleDto>> UpdateVehicleAsync(Guid id, CreateVehicleDto dto)
    {
        try
        {
            var vehicle = await _unitOfWork.Repository<Domain.Entities.Transport.Vehicle>().GetByIdAsync(id);
            if (vehicle is null)
                return ApiResponse<VehicleDto>.NotFoundResponse(ApplicationMessages.NotFound);

            vehicle.VehicleNumber = dto.RegistrationNumber;
            vehicle.VehicleType = dto.VehicleType;
            vehicle.Capacity = dto.Capacity;
            vehicle.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Repository<Domain.Entities.Transport.Vehicle>().UpdateAsync(vehicle);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<VehicleDto>.SuccessResponse(new VehicleDto
            {
                Id = vehicle.Id,
                RegistrationNumber = vehicle.VehicleNumber,
                VehicleType = vehicle.VehicleType,
                DriverName = vehicle.Driver?.Name ?? string.Empty,
                DriverPhone = vehicle.Driver?.Phone ?? string.Empty,
                Capacity = vehicle.Capacity,
                IsActive = vehicle.IsActive
            }, ApplicationMessages.UpdateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse<VehicleDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse> DeleteVehicleAsync(Guid id)
    {
        try
        {
            var vehicle = await _unitOfWork.Repository<Domain.Entities.Transport.Vehicle>().GetByIdAsync(id);
            if (vehicle is null)
                return ApiResponse.FailResponse(ApplicationMessages.NotFound);

            vehicle.IsDeleted = true;
            vehicle.DeletedAt = DateTime.UtcNow;
            await _unitOfWork.Repository<Domain.Entities.Transport.Vehicle>().UpdateAsync(vehicle);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.SuccessResponse(ApplicationMessages.DeleteSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse> AllocateTransportAsync(TransportAllocationDto dto)
    {
        try
        {
            var allocation = new Domain.Entities.Transport.StudentTransportAllocation
            {
                StudentId = dto.StudentId,
                RouteId = dto.RouteId,
                Status = TransportAllocationStatus.Active,
                StartDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<Domain.Entities.Transport.StudentTransportAllocation>().AddAsync(allocation);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.SuccessResponse(ApplicationMessages.CreateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse> DeallocateTransportAsync(Guid allocationId)
    {
        try
        {
            var allocation = await _unitOfWork.Repository<Domain.Entities.Transport.StudentTransportAllocation>().GetByIdAsync(allocationId);
            if (allocation is null)
                return ApiResponse.FailResponse(ApplicationMessages.NotFound);

            allocation.Status = TransportAllocationStatus.Inactive;
            allocation.EndDate = DateTime.UtcNow;
            allocation.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Repository<Domain.Entities.Transport.StudentTransportAllocation>().UpdateAsync(allocation);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.SuccessResponse(ApplicationMessages.UpdateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<PagedResult<TransportAllocationDto>>> GetAllocationsAsync(
        PaginationQuery query, Guid? routeId, Guid? vehicleId)
    {
        try
        {
            var allocations = await _unitOfWork.Repository<Domain.Entities.Transport.StudentTransportAllocation>().GetAllAsync();
            var filtered = allocations.Where(a => !a.IsDeleted).ToList();
            var totalCount = filtered.Count;

            var pagedItems = filtered
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(a => new TransportAllocationDto
                {
                    Id = a.Id,
                    StudentId = a.StudentId,
                    StudentName = a.Student?.User is not null
                        ? $"{a.Student.User.FirstName} {a.Student.User.LastName}"
                        : string.Empty,
                    RouteId = a.RouteId,
                    RouteName = a.Route?.Name ?? string.Empty,
                    MonthlyFee = a.Route?.MonthlyFee ?? 0,
                    IsActive = a.Status == TransportAllocationStatus.Active
                }).ToList();

            return ApiResponse<PagedResult<TransportAllocationDto>>.SuccessResponse(new PagedResult<TransportAllocationDto>
            {
                Items = pagedItems,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            });
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResult<TransportAllocationDto>>.FailResponse(ex.Message);
        }
    }
}
