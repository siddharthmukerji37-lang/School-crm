using Microsoft.EntityFrameworkCore;
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
            var hostels = await _unitOfWork.Hostels.GetAllWithDetailsAsync();
            var filtered = hostels
                .Where(h => !h.IsDeleted && (!schoolId.HasValue || h.SchoolId == schoolId.Value))
                .OrderBy(h => h.Name)
                .ToList();

            var totalCount = filtered.Count;

            var pagedItems = filtered
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(h => ToHostelDto(h))
                .ToList();

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
            var hostel = await _unitOfWork.Hostels.GetHostelWithDetailsAsync(id);
            if (hostel is null)
                return ApiResponse<HostelDto>.NotFoundResponse(ApplicationMessages.NotFound);

            return ApiResponse<HostelDto>.SuccessResponse(ToHostelDto(hostel));
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
            var hostel = new Domain.Entities.Hostel.Hostel
            {
                Name = dto.Name,
                Type = dto.Type,
                Address = dto.Address ?? string.Empty,
                WardenName = dto.WardenName ?? string.Empty,
                WardenPhone = dto.WardenPhone ?? string.Empty,
                SchoolId = dto.SchoolId == Guid.Empty ? Guid.Empty : dto.SchoolId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Hostels.AddAsync(hostel);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<HostelDto>.SuccessResponse(ToHostelDto(hostel), ApplicationMessages.CreateSuccess);
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
            var hostel = await _unitOfWork.Hostels.GetByIdAsync(id);
            if (hostel is null)
                return ApiResponse<HostelDto>.NotFoundResponse(ApplicationMessages.NotFound);

            hostel.Name = dto.Name;
            hostel.Type = dto.Type;
            hostel.Address = dto.Address ?? string.Empty;
            hostel.WardenName = dto.WardenName ?? string.Empty;
            hostel.WardenPhone = dto.WardenPhone ?? string.Empty;
            hostel.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Hostels.UpdateAsync(hostel);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<HostelDto>.SuccessResponse(ToHostelDto(hostel), ApplicationMessages.UpdateSuccess);
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
            var hostel = await _unitOfWork.Hostels.GetByIdAsync(id);
            if (hostel is null)
                return ApiResponse.FailResponse(ApplicationMessages.NotFound);

            await _unitOfWork.Hostels.DeleteAsync(hostel);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.SuccessResponse(ApplicationMessages.DeleteSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<RoomDto>>> GetRoomsAsync(Guid? hostelId)
    {
        try
        {
            var rooms = await _unitOfWork.HostelRooms.GetAllRoomsWithDetailsAsync();
            var filtered = rooms
                .Where(r => !r.IsDeleted && (!hostelId.HasValue || r.HostelId == hostelId.Value))
                .ToList();

            var roomDtos = filtered.Select(r => ToRoomDto(r)).ToList();

            return ApiResponse<List<RoomDto>>.SuccessResponse(roomDtos);
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
            var hostel = await _unitOfWork.Hostels.GetByIdAsync(dto.HostelId);
            if (hostel is null || hostel.IsDeleted)
                return ApiResponse<RoomDto>.FailResponse("Hostel not found. Create a hostel before adding a room.");

            var room = new Domain.Entities.Hostel.HostelRoom
            {
                RoomNumber = dto.RoomNumber,
                RoomType = dto.RoomType,
                Capacity = dto.TotalBeds,
                MonthlyFee = dto.MonthlyFee,
                HostelId = dto.HostelId,
                SchoolId = hostel.SchoolId,
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

            room.Hostel = hostel;
            return ApiResponse<RoomDto>.SuccessResponse(ToRoomDto(room), ApplicationMessages.CreateSuccess);
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

            if (dto.HostelId != Guid.Empty)
            {
                var hostel = await _unitOfWork.Hostels.GetByIdAsync(dto.HostelId);
                if (hostel is null || hostel.IsDeleted)
                    return ApiResponse<RoomDto>.FailResponse("Hostel not found.");

                room.HostelId = dto.HostelId;
            }

            room.RoomNumber = dto.RoomNumber;
            room.RoomType = dto.RoomType;
            room.MonthlyFee = dto.MonthlyFee;
            room.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.HostelRooms.UpdateAsync(room);
            await _unitOfWork.SaveChangesAsync();

            var updated = await _unitOfWork.HostelRooms.GetRoomWithDetailsAsync(id);
            return ApiResponse<RoomDto>.SuccessResponse(ToRoomDto(updated), ApplicationMessages.UpdateSuccess);
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

            await _unitOfWork.HostelRooms.DeleteAsync(room);
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

            var beds = room.Beds?
                .Where(b => !b.IsDeleted)
                .Select(b => new BedDto
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

            var room = await _unitOfWork.HostelRooms.GetByIdAsync(bed.RoomId);
            if (room is null || room.Occupied >= room.Capacity)
                return ApiResponse.FailResponse("Room has no available beds.");

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

            room.Occupied++;
            await _unitOfWork.HostelRooms.UpdateAsync(room);

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
        PaginationQuery query, Guid? hostelId, Guid? studentId = null)
    {
        try
        {
            var allocations = await _unitOfWork.Repository<Domain.Entities.Hostel.HostelAllocation>().AsQueryable()
                .Include(a => a.Student)
                    .ThenInclude(s => s!.User)
                .Include(a => a.Room)
                    .ThenInclude(r => r!.Hostel)
                .Where(a => !a.IsDeleted)
                .ToListAsync();

            var filtered = allocations
                .Where(a => !hostelId.HasValue || a.Room != null && a.Room.HostelId == hostelId.Value)
                .Where(a => !studentId.HasValue || a.StudentId == studentId.Value)
                .ToList();

            var totalCount = filtered.Count;

            var pagedItems = filtered
                .OrderByDescending(a => a.AllocationDate)
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
                    HostelName = a.Room?.Hostel?.Name ?? string.Empty,
                    Status = a.Status.ToString(),
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

    private static HostelDto ToHostelDto(Domain.Entities.Hostel.Hostel h)
    {
        var rooms = h.Rooms?.Where(r => !r.IsDeleted).ToList() ?? new List<Domain.Entities.Hostel.HostelRoom>();
        return new HostelDto
        {
            Id = h.Id,
            Name = h.Name,
            Type = h.Type,
            Address = h.Address,
            WardenName = h.WardenName,
            WardenPhone = h.WardenPhone,
            TotalRooms = rooms.Count,
            TotalBeds = rooms.Sum(r => r.Capacity),
            IsActive = h.IsActive
        };
    }

    private static RoomDto ToRoomDto(Domain.Entities.Hostel.HostelRoom r)
    {
        return new RoomDto
        {
            Id = r.Id,
            HostelId = r.HostelId ?? Guid.Empty,
            HostelName = r.Hostel?.Name ?? string.Empty,
            RoomNumber = r.RoomNumber,
            RoomType = r.RoomType,
            TotalBeds = r.Capacity,
            AvailableBeds = r.Capacity - r.Occupied,
            MonthlyFee = r.MonthlyFee,
            IsActive = r.IsActive
        };
    }
}
