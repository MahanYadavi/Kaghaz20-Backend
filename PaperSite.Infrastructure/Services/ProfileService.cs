using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PaperSite.Application.Common.Responses;
using PaperSite.Application.DTOs.Profile;
using PaperSite.Application.Interfaces;
using PaperSite.Domain.Entities;

namespace PaperSite.Infrastructure.Services;

public class ProfileService : IProfileService
{
    private readonly IRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly PasswordHasher<User> _passwordHasher = new();

    public ProfileService(IRepository<User> userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BaseResponse<ProfileDto>> GetProfileAsync(Guid userId)
    {
        var user = await _userRepository.Query(true).Include(x => x.Role).FirstOrDefaultAsync(x => x.Id == userId);
        return user == null
            ? BaseResponse<ProfileDto>.Failure("User not found")
            : BaseResponse<ProfileDto>.Success(ToDto(user), "Profile retrieved successfully");
    }

    public async Task<BaseResponse<ProfileDto>> UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
    {
        var user = await _userRepository.Query().Include(x => x.Role).FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
        {
            return BaseResponse<ProfileDto>.Failure("User not found");
        }



        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.UserName = request.UserName;
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return BaseResponse<ProfileDto>.Success(ToDto(user), "طلاعات حساب کاربری با موفقیت به‌روزرسانی شد");
    }

    public async Task<BaseResponse<bool>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return BaseResponse<bool>.Failure("کاربر یافت نشد");
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword);
        if (result == PasswordVerificationResult.Failed)
        {
            return BaseResponse<bool>.Failure("گذرواژه فعلی نادرست است");
        }

        user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return BaseResponse<bool>.Success(true, "گذرواژه با موفقیت تغییر یافت");
    }

    private static ProfileDto ToDto(User user) => new()
    {
        Id = user.Id,
        FirstName = user.FirstName,
        LastName = user.LastName,
        UserName = user.UserName,
        Email = user.Email,
        PhoneNumber = user.PhoneNumber,
        Role = user.Role.Name
    };
}
