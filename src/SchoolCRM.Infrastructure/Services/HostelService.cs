using SchoolCRM.Application.Interfaces.Repositories;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.Domain.Enums;
using SchoolCRM.Shared.Constants;
using SchoolCRM.Shared.Models;
using static SchoolCRM.Application.Interfaces.Services.IHostelService;

namespace SchoolCRM.Infrastructure.Services;

public class HostelService : IHostelService
{
    private readonly IUnitOfWork _unitOfWork;

    public HostelService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<PagedResult<HostelDto>>> GetHostelsAsync(PaginationQuery query, Guid? schoolId)
    {
        try
        {
            var rooms = await _unitOfWork.HostelRooms.GetAllAsync();
            var filtered = rooms.Where(r => !r.IsDeleted).ToList();
            var totalCount = filtered.Count;

            var pagedItems = filtered
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(r => new HostelDto
                {
                    Id = r.Id,
                    Name = $"{r.Building ?? string.Empty} - {r.RoomNumber}",
                    Type = r.RoomType,
                    TotalRooms = r.Capacity,
                    IsActive = r.IsActive
                }).ToList();

            return ApiResponse<PagedResult<HostelDto>>.SuccessResponse(new PagedResult<HostelDto>
            {
                Items = pagedItems,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            });
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResult<HostelDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<HostelDto>> GetHostelByIdAsync(Guid id)
    {
        try
        {
            var room = await _unitOfWork.HostelRooms.GetRoomWithDetailsAsync(id);
            if (room is null)
                return ApiResponse<HostelDto>.NotFoundResponse(ApplicationMessages.NotFound);

            return ApiResponse<HostelDto>.SuccessResponse(new HostelDto
            {
                Id = room.Id,
                Name = $"{room.Building ?? string.Empty} - {room.RoomNumber}",
                Type = room.RoomType,
                TotalRooms = room.Capacity,
                IsActive = room.IsActive
            });
        }
        catch (Exception ex)
        {
            return ApiResponse<HostelDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<HostelDto>> CreateHostelAsync(HostelDto dto)
    {
        try
        {
            var room = new Domain.Entities.Hostel.HostelRoom
            {
                RoomNumber = dto.Name,
                RoomType = dto.Type,
                Capacity = dto.TotalRooms,
                SchoolId = Guid.Empty,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.HostelRooms.AddAsync(room);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<HostelDto>.SuccessResponse(new HostelDto
            {
                Id = room.Id,
                Name = room.RoomNumber,
                Type = room.RoomType,
                TotalRooms = room.Capacity,
                IsActive = true
            }, ApplicationMessages.CreateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse<HostelDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<HostelDto>> UpdateHostelAsync(Guid id, HostelDto dto)
    {
        try
        {
            var room = await _unitOfWork.HostelRooms.GetByIdAsync(id);
            if (room is null)
                return ApiResponse<HostelDto>.NotFoundResponse(ApplicationMessages.NotFound);

            room.RoomNumber = dto.Name;
            room.RoomType = dto.Type;
            room.Capacity = dto.TotalRooms;
            room.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.HostelRooms.UpdateAsync(room);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<HostelDto>.SuccessResponse(new HostelDto
            {
                Id = room.Id,
                Name = room.RoomNumber,
                Type = room.RoomType,
                TotalRooms = room.Capacity,
                IsActive = room.IsActive
            }, ApplicationMessages.UpdateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse<HostelDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse> DeleteHostelAsync(Guid id)
    {
        try
        {
            var room = await _unitOfWork.HostelRooms.GetByIdAsync(id);
            if (room is null)
                return ApiResponse.FailResponse(ApplicationMessages.NotFound);

            room.IsDeleted = true;
            room.DeletedAt = DateTime.UtcNow;
            await _unitOfWork.HostelRooms.UpdateAsync(room);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.SuccessResponse(ApplicationMessages.DeleteSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<RoomDto>>> GetRoomsAsync(Guid hostelId)
    {
        try
        {
            var room = await _unitOfWork.HostelRooms.GetRoomWithDetailsAsync(hostelId);
            if (room is null)
                return ApiResponse<List<RoomDto>>.NotFoundResponse(ApplicationMessages.NotFound);

            var beds = room.Beds?.Select(b => new BedDto
            {
                Id = b.Id,
                RoomId = b.RoomId,
                BedNumber = b.BedNumber,
                IsOccupied = b.IsOccupied
            }).ToList() ?? new List<BedDto>();

            var roomDto = new RoomDto
            {
                Id = room.Id,
                RoomNumber = room.RoomNumber,
                RoomType = room.RoomType,
                TotalBeds = room.Capacity,
                AvailableBeds = room.Capacity - room.Occupied,
                MonthlyFee = room.MonthlyFee,
                IsActive = room.IsActive
            };

            return ApiResponse<List<RoomDto>>.SuccessResponse(new List<RoomDto> { roomDto });
        }
        catch (Exception ex)
        {
            return ApiResponse<List<RoomDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<RoomDto>> CreateRoomAsync(CreateRoomDto dto)
    {
        try
        {
            var room = new Domain.Entities.Hostel.HostelRoom
            {
                RoomNumber = dto.RoomNumber,
                RoomType = dto.RoomType,
                Capacity = dto.TotalBeds,
                MonthlyFee = dto.MonthlyFee,
                SchoolId = Guid.Empty,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.HostelRooms.AddAsync(room);

            for (int i = 1; i <= dto.TotalBeds; i++)
            {
                var bed = new Domain.Entities.Hostel.HostelBed
                {
                    BedNumber = $"{dto.RoomNumber}-B{i}",
                    IsOccupied = false,
                    RoomId = room.Id,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.Repository<Domain.Entities.Hostel.HostelBed>().AddAsync(bed);
            }

            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<RoomDto>.SuccessResponse(new RoomDto
            {
                Id = room.Id,
                RoomNumber = room.RoomNumber,
                RoomType = room.RoomType,
                TotalBeds = room.Capacity,
                AvailableBeds = room.Capacity,
                MonthlyFee = room.MonthlyFee,
                IsActive = true
            }, ApplicationMessages.CreateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse<RoomDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<RoomDto>> UpdateRoomAsync(Guid id, CreateRoomDto dto)
    {
        try
        {
            var room = await _unitOfWork.HostelRooms.GetByIdAsync(id);
            if (room is null)
                return ApiResponse<RoomDto>.NotFoundResponse(ApplicationMessages.NotFound);

            room.RoomNumber = dto.RoomNumber;
            room.RoomType = dto.RoomType;
            room.MonthlyFee = dto.MonthlyFee;
            room.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.HostelRooms.UpdateAsync(room);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<RoomDto>.SuccessResponse(new RoomDto
            {
                Id = room.Id,
                RoomNumber = room.RoomNumber,
                RoomType = room.RoomType,
                TotalBeds = room.Capacity,
                AvailableBeds = room.Capacity - room.Occupied,
                MonthlyFee = room.MonthlyFee,
                IsActive = room.IsActive
            }, ApplicationMessages.UpdateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse<RoomDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse> DeleteRoomAsync(Guid id)
    {
        try
        {
            var room = await _unitOfWork.HostelRooms.GetByIdAsync(id);
            if (room is null)
                return ApiResponse.FailResponse(ApplicationMessages.NotFound);

            room.IsDeleted = true;
            room.DeletedAt = DateTime.UtcNow;
            await _unitOfWork.HostelRooms.UpdateAsync(room);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.SuccessResponse(ApplicationMessages.DeleteSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<BedDto>>> GetBedsAsync(Guid roomId)
    {
        try
        {
            var room = await _unitOfWork.HostelRooms.GetRoomWithDetailsAsync(roomId);
            if (room is null)
                return ApiResponse<List<BedDto>>.NotFoundResponse(ApplicationMessages.NotFound);

            var beds = room.Beds?.Select(b => new BedDto
            {
                Id = b.Id,
                RoomId = b.RoomId,
                BedNumber = b.BedNumber,
                IsOccupied = b.IsOccupied
            }).ToList() ?? new List<BedDto>();

            return ApiResponse<List<BedDto>>.SuccessResponse(beds);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<BedDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse> AllocateBedAsync(BedAllocationDto dto)
    {
        try
        {
            var bed = await _unitOfWork.Repository<Domain.Entities.Hostel.HostelBed>().GetByIdAsync(dto.BedId);
            if (bed is null || bed.IsOccupied)
                return ApiResponse.FailResponse("Bed is not available.");

            var allocation = new Domain.Entities.Hostel.HostelAllocation
            {
                StudentId = dto.StudentId,
                RoomId = bed.RoomId,
                BedId = dto.BedId,
                AllocationDate = DateTime.UtcNow,
                Status = HostelAllocationStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<Domain.Entities.Hostel.HostelAllocation>().AddAsync(allocation);

            bed.IsOccupied = true;
            await _unitOfWork.Repository<Domain.Entities.Hostel.HostelBed>().UpdateAsync(bed);

            var room = await _unitOfWork.HostelRooms.GetByIdAsync(bed.RoomId);
            if (room is not null)
            {
                room.Occupied++;
                await _unitOfWork.HostelRooms.UpdateAsync(room);
            }

            await _unitOfWork.SaveChangesAsync();
            return ApiResponse.SuccessResponse(ApplicationMessages.CreateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse> DeallocateBedAsync(Guid allocationId)
    {
        try
        {
            var allocation = await _unitOfWork.Repository<Domain.Entities.Hostel.HostelAllocation>().GetByIdAsync(allocationId);
            if (allocation is null)
                return ApiResponse.FailResponse(ApplicationMessages.NotFound);

            allocation.Status = HostelAllocationStatus.CheckedOut;
            allocation.CheckOutDate = DateTime.UtcNow;
            allocation.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Repository<Domain.Entities.Hostel.HostelAllocation>().UpdateAsync(allocation);

            if (allocation.BedId.HasValue)
            {
                var bed = await _unitOfWork.Repository<Domain.Entities.Hostel.HostelBed>().GetByIdAsync(allocation.BedId.Value);
                if (bed is not null)
                {
                    bed.IsOccupied = false;
                    await _unitOfWork.Repository<Domain.Entities.Hostel.HostelBed>().UpdateAsync(bed);
                }
            }

            var room = await _unitOfWork.HostelRooms.GetByIdAsync(allocation.RoomId);
            if (room is not null && room.Occupied > 0)
            {
                room.Occupied--;
                await _unitOfWork.HostelRooms.UpdateAsync(room);
            }

            await _unitOfWork.SaveChangesAsync();
            return ApiResponse.SuccessResponse(ApplicationMessages.UpdateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<PagedResult<BedAllocationDto>>> GetAllocationsAsync(
        PaginationQuery query, Guid? hostelId)
    {
        try
        {
            var allocations = await _unitOfWork.Repository<Domain.Entities.Hostel.HostelAllocation>().GetAllAsync();
            var filtered = allocations.Where(a => !a.IsDeleted).ToList();
            var totalCount = filtered.Count;

            var pagedItems = filtered
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(a => new BedAllocationDto
                {
                    Id = a.Id,
                    StudentId = a.StudentId,
                    StudentName = a.Student?.User is not null
                        ? $"{a.Student.User.FirstName} {a.Student.User.LastName}"
                        : string.Empty,
                    BedId = a.BedId ?? Guid.Empty,
                    RoomNumber = a.Room?.RoomNumber ?? string.Empty,
                    AllocationDate = a.AllocationDate,
                    DeallocationDate = a.CheckOutDate,
                    IsActive = a.Status == HostelAllocationStatus.Active
                }).ToList();

            return ApiResponse<PagedResult<BedAllocationDto>>.SuccessResponse(new PagedResult<BedAllocationDto>
            {
                Items = pagedItems,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            });
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResult<BedAllocationDto>>.FailResponse(ex.Message);
        }
    }
}
