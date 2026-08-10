using SchoolCRM.Application.DTOs.Notification;
using SchoolCRM.Application.Interfaces.Repositories;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.Domain.Entities.Notification;
using SchoolCRM.Shared.Constants;
using SchoolCRM.Shared.Models;

namespace SchoolCRM.Infrastructure.Services;

public class NoticeService : INoticeService
{
    private readonly IUnitOfWork _unitOfWork;

    public NoticeService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<PagedResult<NoticeDto>>> GetNoticesAsync(PaginationQuery query)
    {
        try
        {
            var (items, totalCount) = await _unitOfWork.Repository<Announcement>().GetPagedAsync(
                query.PageNumber,
                query.PageSize,
                filter: string.IsNullOrWhiteSpace(query.SearchTerm)
                    ? null
                    : (System.Linq.Expressions.Expression<Func<Announcement, bool>>)(a =>
                        a.Title.Contains(query.SearchTerm) || a.Content.Contains(query.SearchTerm)),
                orderBy: q => q.OrderByDescending(a => a.CreatedAt));

            var dtos = items.Select(MapToDto).ToList();

            var pagedResult = new PagedResult<NoticeDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                SearchTerm = query.SearchTerm
            };

            return ApiResponse<PagedResult<NoticeDto>>.SuccessResponse(pagedResult);
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResult<NoticeDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<NoticeDto>> GetNoticeByIdAsync(Guid id)
    {
        try
        {
            var notice = await _unitOfWork.Repository<Announcement>().GetByIdAsync(id);
            if (notice is null)
                return ApiResponse<NoticeDto>.NotFoundResponse(ApplicationMessages.NotFound);

            return ApiResponse<NoticeDto>.SuccessResponse(MapToDto(notice));
        }
        catch (Exception ex)
        {
            return ApiResponse<NoticeDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<NoticeDto>> CreateNoticeAsync(CreateNoticeDto dto, Guid userId, string createdByName)
    {
        try
        {
            var school = (await _unitOfWork.Schools.GetAllAsync()).FirstOrDefault();

            var notice = new Announcement
            {
                Title = dto.Title,
                Content = dto.Content,
                TargetAudience = dto.Type,
                Priority = dto.Priority,
                PublishDate = dto.PublishDate ?? DateTime.UtcNow,
                ExpiryDate = dto.ExpiryDate,
                IsPublished = dto.IsPublished,
                SchoolId = school?.Id ?? Guid.Empty,
                CreatedByName = createdByName,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<Announcement>().AddAsync(notice);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<NoticeDto>.SuccessResponse(MapToDto(notice), ApplicationMessages.CreateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse<NoticeDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<NoticeDto>> UpdateNoticeAsync(Guid id, UpdateNoticeDto dto)
    {
        try
        {
            var notice = await _unitOfWork.Repository<Announcement>().GetByIdAsync(id);
            if (notice is null)
                return ApiResponse<NoticeDto>.NotFoundResponse(ApplicationMessages.NotFound);

            notice.Title = dto.Title;
            notice.Content = dto.Content;
            notice.TargetAudience = dto.Type;
            notice.Priority = dto.Priority;
            notice.PublishDate = dto.PublishDate;
            notice.ExpiryDate = dto.ExpiryDate;
            notice.IsPublished = dto.IsPublished;
            notice.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Repository<Announcement>().UpdateAsync(notice);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<NoticeDto>.SuccessResponse(MapToDto(notice), ApplicationMessages.UpdateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse<NoticeDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse> DeleteNoticeAsync(Guid id)
    {
        try
        {
            var notice = await _unitOfWork.Repository<Announcement>().GetByIdAsync(id);
            if (notice is null)
                return ApiResponse.FailResponse(ApplicationMessages.NotFound);

            notice.IsDeleted = true;
            notice.DeletedAt = DateTime.UtcNow;
            await _unitOfWork.Repository<Announcement>().UpdateAsync(notice);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.SuccessResponse(ApplicationMessages.DeleteSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<NoticeDto>>> GetPublishedNoticesAsync()
    {
        try
        {
            var notices = await _unitOfWork.Repository<Announcement>().FindAsync(
                a => a.IsPublished && !a.IsDeleted);

            var ordered = notices
                .Where(a => !a.ExpiryDate.HasValue || a.ExpiryDate.Value >= DateTime.UtcNow)
                .OrderByDescending(a => a.PublishDate ?? a.CreatedAt)
                .ToList();

            var dtos = ordered.Select(MapToDto).ToList();
            return ApiResponse<List<NoticeDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<NoticeDto>>.FailResponse(ex.Message);
        }
    }

    private static NoticeDto MapToDto(Announcement notice)
    {
        return new NoticeDto
        {
            Id = notice.Id,
            Title = notice.Title,
            Content = notice.Content,
            Type = notice.TargetAudience,
            Priority = notice.Priority,
            PublishDate = notice.PublishDate,
            ExpiryDate = notice.ExpiryDate,
            IsPublished = notice.IsPublished,
            CreatedByName = notice.CreatedByName,
            CreatedAt = notice.CreatedAt
        };
    }
}
