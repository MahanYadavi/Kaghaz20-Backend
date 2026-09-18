using PaperSite.Application.Common.Responses;
using PaperSite.Application.DTOs.Profile;

namespace PaperSite.Application.Interfaces;

public interface IProfileService
{
    Task<BaseResponse<ProfileDto>> GetProfileAsync(Guid userId);
    Task<BaseResponse<ProfileDto>> UpdateProfileAsync(Guid userId, UpdateProfileRequest request);
    Task<BaseResponse<bool>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
}
