using Microsoft.AspNetCore.Identity;
using SchoolCRM.Application.DTOs.Parent;
using SchoolCRM.Application.Interfaces.Repositories;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.Domain.Entities.Identity;
using SchoolCRM.Domain.Enums;
using SchoolCRM.Shared.Constants;
using SchoolCRM.Shared.Models;

namespace SchoolCRM.Infrastructure.Services;

public class ParentService : IParentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;

    public ParentService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    public async Task<ApiResponse<PagedResult<ParentDto>>> GetParentsAsync(PaginationQuery query)
    {
        try
        {
            var (items, totalCount) = await _unitOfWork.Parents.GetPagedParentsAsync(
                query.PageNumber, query.PageSize, query.SearchTerm, query.SortColumn, query.SortOrder);

            var dtos = items.Select(MapToDto).ToList();

            var pagedResult = new PagedResult<ParentDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                SearchTerm = query.SearchTerm,
                SortColumn = query.SortColumn,
                SortOrder = query.SortOrder
            };

            return ApiResponse<PagedResult<ParentDto>>.SuccessResponse(pagedResult);
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResult<ParentDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<ParentDto>> GetParentByIdAsync(Guid id)
    {
        try
        {
            var parent = await _unitOfWork.Parents.GetParentWithDetailsAsync(id);
            if (parent is null)
                return ApiResponse<ParentDto>.NotFoundResponse(ApplicationMessages.NotFound);

            return ApiResponse<ParentDto>.SuccessResponse(MapToDto(parent));
        }
        catch (Exception ex)
        {
            return ApiResponse<ParentDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<ParentDto>> CreateParentAsync(CreateParentDto dto)
    {
        try
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser is not null)
                return ApiResponse<ParentDto>.FailResponse(ApplicationMessages.DuplicateRecord);

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PhoneNumber = dto.Phone,
                Address = dto.Address,
                City = dto.City,
                State = dto.State,
                Country = dto.Country,
                PostalCode = dto.PostalCode,
                ProfilePictureUrl = dto.ProfilePictureUrl,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, dto.Password ?? "Parent@123");
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return ApiResponse<ParentDto>.FailResponse(string.Join("; ", errors));
            }

            await _userManager.AddToRoleAsync(user, Roles.Parent);

            var parentCode = $"PRN-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}";

            var parent = new Domain.Entities.Parent.Parent
            {
                ParentCode = parentCode,
                UserId = user.Id,
                Occupation = dto.Occupation,
                Relationship = dto.Relationship,
                AlternatePhone = dto.AlternativePhone,
                IsEmergencyContact = true,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Parents.AddAsync(parent);
            await _unitOfWork.SaveChangesAsync();

            if (dto.ChildrenStudentIds?.Any() == true)
            {
                foreach (var studentId in dto.ChildrenStudentIds)
                {
                    var student = await _unitOfWork.Students.GetByIdAsync(studentId);
                    if (student is not null)
                    {
                        student.ParentId = parent.Id;
                        await _unitOfWork.Students.UpdateAsync(student);
                    }
                }
                await _unitOfWork.SaveChangesAsync();
            }

            var created = await _unitOfWork.Parents.GetParentWithDetailsAsync(parent.Id);
            return ApiResponse<ParentDto>.SuccessResponse(MapToDto(created!), ApplicationMessages.CreateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse<ParentDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<ParentDto>> UpdateParentAsync(Guid id, UpdateParentDto dto)
    {
        try
        {
            var parent = await _unitOfWork.Parents.GetParentWithDetailsAsync(id);
            if (parent is null)
                return ApiResponse<ParentDto>.NotFoundResponse(ApplicationMessages.NotFound);

            parent.Occupation = dto.Occupation;
            parent.Relationship = dto.Relationship;
            parent.AlternatePhone = dto.AlternativePhone;
            parent.UpdatedAt = DateTime.UtcNow;

            var user = await _userManager.FindByIdAsync(parent.UserId.ToString());
            if (user is not null)
            {
                user.FirstName = dto.FirstName;
                user.LastName = dto.LastName;
                user.Email = dto.Email;
                user.UserName = dto.Email;
                user.PhoneNumber = dto.Phone;
                user.Address = dto.Address;
                user.City = dto.City;
                user.State = dto.State;
                user.Country = dto.Country;
                user.PostalCode = dto.PostalCode;
                user.ProfilePictureUrl = dto.ProfilePictureUrl;
                user.IsActive = dto.IsActive;
                user.UpdatedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
            }

            await _unitOfWork.Parents.UpdateAsync(parent);
            await _unitOfWork.SaveChangesAsync();

            if (dto.ChildrenStudentIds is not null)
            {
                var existingChildren = parent.Students?.Select(s => s.Id).ToList() ?? new List<Guid>();
                var toRemove = existingChildren.Except(dto.ChildrenStudentIds);
                var toAdd = dto.ChildrenStudentIds.Except(existingChildren);

                foreach (var studentId in toRemove)
                {
                    var student = await _unitOfWork.Students.GetByIdAsync(studentId);
                    if (student is not null)
                    {
                        student.ParentId = null;
                        await _unitOfWork.Students.UpdateAsync(student);
                    }
                }

                foreach (var studentId in toAdd)
                {
                    var student = await _unitOfWork.Students.GetByIdAsync(studentId);
                    if (student is not null)
                    {
                        student.ParentId = parent.Id;
                        await _unitOfWork.Students.UpdateAsync(student);
                    }
                }
                await _unitOfWork.SaveChangesAsync();
            }

            var updated = await _unitOfWork.Parents.GetParentWithDetailsAsync(id);
            return ApiResponse<ParentDto>.SuccessResponse(MapToDto(updated!), ApplicationMessages.UpdateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse<ParentDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse> DeleteParentAsync(Guid id)
    {
        try
        {
            var parent = await _unitOfWork.Parents.GetByIdAsync(id);
            if (parent is null)
                return ApiResponse.FailResponse(ApplicationMessages.NotFound);

            parent.IsDeleted = true;
            parent.DeletedAt = DateTime.UtcNow;
            await _unitOfWork.Parents.UpdateAsync(parent);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.SuccessResponse(ApplicationMessages.DeleteSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse.FailResponse(ex.Message);
        }
    }

    private static ParentDto MapToDto(Domain.Entities.Parent.Parent parent)
    {
        var children = parent.Students?.Select(s => new StudentChildDto
        {
            StudentId = s.Id,
            StudentName = $"{s.User.FirstName} {s.User.LastName}",
            ClassName = s.Section?.ClassRoom?.Name ?? string.Empty,
            SectionName = s.Section?.Name ?? string.Empty,
            AdmissionNumber = s.AdmissionNumber
        }).ToList() ?? new List<StudentChildDto>();

        return new ParentDto
        {
            Id = parent.Id,
            UserId = parent.UserId.ToString(),
            FirstName = parent.User.FirstName,
            LastName = parent.User.LastName,
            Email = parent.User.Email ?? string.Empty,
            Phone = parent.User.PhoneNumber,
            AlternativePhone = parent.AlternatePhone,
            Occupation = parent.Occupation,
            Relationship = parent.Relationship,
            Address = parent.User.Address,
            City = parent.User.City,
            State = parent.User.State,
            Country = parent.User.Country,
            PostalCode = parent.User.PostalCode,
            ProfilePictureUrl = parent.User.ProfilePictureUrl,
            Children = children,
            IsActive = parent.User.IsActive
        };
    }
}
