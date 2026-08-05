using SchoolCRM.Application.Interfaces.Repositories;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.Shared.Constants;
using SchoolCRM.Shared.Models;
using static SchoolCRM.Application.Interfaces.Services.IInventoryService;

namespace SchoolCRM.Infrastructure.Services;

public class InventoryService : IInventoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public InventoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<PagedResult<ItemDto>>> GetItemsAsync(
        PaginationQuery query, string? category, Guid? schoolId)
    {
        try
        {
            var items = await _unitOfWork.InventoryItems.GetAllAsync();
            var filtered = items.Where(i => !i.IsDeleted).ToList();
            var totalCount = filtered.Count;

            var pagedItems = filtered
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(i => new ItemDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Description = i.Description ?? string.Empty,
                    Category = i.Category?.Name ?? string.Empty,
                    Unit = i.Unit,
                    PurchasePrice = i.PurchasePrice,
                    SellingPrice = i.SellingPrice,
                    MinimumStock = i.MinimumStock,
                    IsActive = i.IsActive
                }).ToList();

            return ApiResponse<PagedResult<ItemDto>>.SuccessResponse(new PagedResult<ItemDto>
            {
                Items = pagedItems,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            });
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResult<ItemDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<ItemDto>> GetItemByIdAsync(Guid id)
    {
        try
        {
            var item = await _unitOfWork.InventoryItems.GetByIdAsync(id);
            if (item is null)
                return ApiResponse<ItemDto>.NotFoundResponse(ApplicationMessages.NotFound);

            return ApiResponse<ItemDto>.SuccessResponse(new ItemDto
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description ?? string.Empty,
                Category = item.Category?.Name ?? string.Empty,
                Unit = item.Unit,
                PurchasePrice = item.PurchasePrice,
                SellingPrice = item.SellingPrice,
                MinimumStock = item.MinimumStock,
                IsActive = item.IsActive
            });
        }
        catch (Exception ex)
        {
            return ApiResponse<ItemDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<ItemDto>> CreateItemAsync(CreateItemDto dto)
    {
        try
        {
            var item = new Domain.Entities.Inventory.InventoryItem
            {
                Name = dto.Name,
                Code = dto.Name[..Math.Min(3, dto.Name.Length)].ToUpper(),
                Description = dto.Description,
                Unit = dto.Unit,
                PurchasePrice = dto.PurchasePrice,
                SellingPrice = dto.SellingPrice,
                MinimumStock = dto.MinimumStock,
                MaximumStock = dto.MinimumStock * 10,
                CurrentStock = 0,
                IsActive = true,
                CategoryId = Guid.Empty,
                SchoolId = Guid.Empty,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.InventoryItems.AddAsync(item);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<ItemDto>.SuccessResponse(new ItemDto
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description ?? string.Empty,
                Unit = item.Unit,
                PurchasePrice = item.PurchasePrice,
                SellingPrice = item.SellingPrice,
                MinimumStock = item.MinimumStock,
                IsActive = true
            }, ApplicationMessages.CreateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse<ItemDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<ItemDto>> UpdateItemAsync(Guid id, CreateItemDto dto)
    {
        try
        {
            var item = await _unitOfWork.InventoryItems.GetByIdAsync(id);
            if (item is null)
                return ApiResponse<ItemDto>.NotFoundResponse(ApplicationMessages.NotFound);

            item.Name = dto.Name;
            item.Description = dto.Description;
            item.Unit = dto.Unit;
            item.PurchasePrice = dto.PurchasePrice;
            item.SellingPrice = dto.SellingPrice;
            item.MinimumStock = dto.MinimumStock;
            item.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.InventoryItems.UpdateAsync(item);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<ItemDto>.SuccessResponse(new ItemDto
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description ?? string.Empty,
                Unit = item.Unit,
                PurchasePrice = item.PurchasePrice,
                SellingPrice = item.SellingPrice,
                MinimumStock = item.MinimumStock,
                IsActive = item.IsActive
            }, ApplicationMessages.UpdateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse<ItemDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse> DeleteItemAsync(Guid id)
    {
        try
        {
            var item = await _unitOfWork.InventoryItems.GetByIdAsync(id);
            if (item is null)
                return ApiResponse.FailResponse(ApplicationMessages.NotFound);

            item.IsDeleted = true;
            item.DeletedAt = DateTime.UtcNow;
            await _unitOfWork.InventoryItems.UpdateAsync(item);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.SuccessResponse(ApplicationMessages.DeleteSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<StockDto>> GetStockAsync(Guid itemId)
    {
        try
        {
            var item = await _unitOfWork.InventoryItems.GetByIdAsync(itemId);
            if (item is null)
                return ApiResponse<StockDto>.NotFoundResponse(ApplicationMessages.NotFound);

            return ApiResponse<StockDto>.SuccessResponse(new StockDto
            {
                ItemId = item.Id,
                ItemName = item.Name,
                CurrentStock = item.CurrentStock,
                MinimumStock = item.MinimumStock,
                IsLowStock = item.CurrentStock <= item.MinimumStock
            });
        }
        catch (Exception ex)
        {
            return ApiResponse<StockDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse> AdjustStockAsync(AdjustStockDto dto)
    {
        try
        {
            var item = await _unitOfWork.InventoryItems.GetByIdAsync(dto.ItemId);
            if (item is null)
                return ApiResponse.FailResponse(ApplicationMessages.NotFound);

            if (dto.MovementType == "In" || dto.MovementType == "Purchase")
                item.CurrentStock += dto.Quantity;
            else if (dto.MovementType == "Out" || dto.MovementType == "Issue")
                item.CurrentStock -= dto.Quantity;

            item.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.InventoryItems.UpdateAsync(item);

            var transaction = new Domain.Entities.Inventory.StockTransaction
            {
                TransactionType = dto.MovementType,
                Quantity = dto.Quantity,
                UnitPrice = dto.UnitPrice ?? item.PurchasePrice,
                TotalAmount = dto.Quantity * (dto.UnitPrice ?? item.PurchasePrice),
                Remarks = dto.Reason,
                TransactionDate = DateTime.UtcNow,
                ItemId = dto.ItemId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<Domain.Entities.Inventory.StockTransaction>().AddAsync(transaction);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.SuccessResponse(ApplicationMessages.UpdateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<StockMovementDto>>> GetStockMovementsAsync(
        Guid itemId, DateTime? fromDate, DateTime? toDate)
    {
        try
        {
            var transactions = await _unitOfWork.Repository<Domain.Entities.Inventory.StockTransaction>()
                .FindAsync(t => t.ItemId == itemId && !t.IsDeleted);

            var filtered = transactions.AsEnumerable();
            if (fromDate.HasValue)
                filtered = filtered.Where(t => t.TransactionDate >= fromDate.Value);
            if (toDate.HasValue)
                filtered = filtered.Where(t => t.TransactionDate <= toDate.Value);

            var dtos = filtered.OrderByDescending(t => t.TransactionDate).Select(t => new StockMovementDto
            {
                Id = t.Id,
                ItemId = t.ItemId,
                ItemName = t.Item?.Name ?? string.Empty,
                MovementType = t.TransactionType,
                Quantity = t.Quantity,
                UnitPrice = t.UnitPrice,
                Reason = t.Remarks ?? string.Empty,
                Date = t.TransactionDate,
                CreatedBy = t.PerformedBy ?? string.Empty
            }).ToList();

            return ApiResponse<List<StockMovementDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<StockMovementDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<PagedResult<VendorDto>>> GetVendorsAsync(PaginationQuery query)
    {
        try
        {
            var vendors = await _unitOfWork.Repository<Domain.Entities.Inventory.Vendor>().GetAllAsync();
            var filtered = vendors.Where(v => !v.IsDeleted).ToList();
            var totalCount = filtered.Count;

            var pagedItems = filtered
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(v => new VendorDto
                {
                    Id = v.Id,
                    Name = v.Name,
                    ContactPerson = v.ContactPerson ?? string.Empty,
                    Phone = v.Phone,
                    Email = v.Email ?? string.Empty,
                    Address = v.Address ?? string.Empty,
                    GSTNumber = v.GSTNumber ?? string.Empty,
                    IsActive = v.IsActive
                }).ToList();

            return ApiResponse<PagedResult<VendorDto>>.SuccessResponse(new PagedResult<VendorDto>
            {
                Items = pagedItems,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            });
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResult<VendorDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<VendorDto>> GetVendorByIdAsync(Guid id)
    {
        try
        {
            var vendor = await _unitOfWork.Repository<Domain.Entities.Inventory.Vendor>().GetByIdAsync(id);
            if (vendor is null)
                return ApiResponse<VendorDto>.NotFoundResponse(ApplicationMessages.NotFound);

            return ApiResponse<VendorDto>.SuccessResponse(new VendorDto
            {
                Id = vendor.Id,
                Name = vendor.Name,
                ContactPerson = vendor.ContactPerson ?? string.Empty,
                Phone = vendor.Phone,
                Email = vendor.Email ?? string.Empty,
                Address = vendor.Address ?? string.Empty,
                GSTNumber = vendor.GSTNumber ?? string.Empty,
                IsActive = vendor.IsActive
            });
        }
        catch (Exception ex)
        {
            return ApiResponse<VendorDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<VendorDto>> CreateVendorAsync(CreateVendorDto dto)
    {
        try
        {
            var vendor = new Domain.Entities.Inventory.Vendor
            {
                Name = dto.Name,
                ContactPerson = dto.ContactPerson,
                Phone = dto.Phone,
                Email = dto.Email,
                Address = dto.Address,
                GSTNumber = dto.GSTNumber,
                IsActive = true,
                SchoolId = Guid.Empty,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<Domain.Entities.Inventory.Vendor>().AddAsync(vendor);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<VendorDto>.SuccessResponse(new VendorDto
            {
                Id = vendor.Id,
                Name = vendor.Name,
                ContactPerson = vendor.ContactPerson ?? string.Empty,
                Phone = vendor.Phone,
                Email = vendor.Email ?? string.Empty,
                Address = vendor.Address ?? string.Empty,
                GSTNumber = vendor.GSTNumber ?? string.Empty,
                IsActive = true
            }, ApplicationMessages.CreateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse<VendorDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<VendorDto>> UpdateVendorAsync(Guid id, CreateVendorDto dto)
    {
        try
        {
            var vendor = await _unitOfWork.Repository<Domain.Entities.Inventory.Vendor>().GetByIdAsync(id);
            if (vendor is null)
                return ApiResponse<VendorDto>.NotFoundResponse(ApplicationMessages.NotFound);

            vendor.Name = dto.Name;
            vendor.ContactPerson = dto.ContactPerson;
            vendor.Phone = dto.Phone;
            vendor.Email = dto.Email;
            vendor.Address = dto.Address;
            vendor.GSTNumber = dto.GSTNumber;
            vendor.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Repository<Domain.Entities.Inventory.Vendor>().UpdateAsync(vendor);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<VendorDto>.SuccessResponse(new VendorDto
            {
                Id = vendor.Id,
                Name = vendor.Name,
                ContactPerson = vendor.ContactPerson ?? string.Empty,
                Phone = vendor.Phone,
                Email = vendor.Email ?? string.Empty,
                Address = vendor.Address ?? string.Empty,
                GSTNumber = vendor.GSTNumber ?? string.Empty,
                IsActive = vendor.IsActive
            }, ApplicationMessages.UpdateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse<VendorDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse> DeleteVendorAsync(Guid id)
    {
        try
        {
            var vendor = await _unitOfWork.Repository<Domain.Entities.Inventory.Vendor>().GetByIdAsync(id);
            if (vendor is null)
                return ApiResponse.FailResponse(ApplicationMessages.NotFound);

            vendor.IsDeleted = true;
            vendor.DeletedAt = DateTime.UtcNow;
            await _unitOfWork.Repository<Domain.Entities.Inventory.Vendor>().UpdateAsync(vendor);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.SuccessResponse(ApplicationMessages.DeleteSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse.FailResponse(ex.Message);
        }
    }
}
