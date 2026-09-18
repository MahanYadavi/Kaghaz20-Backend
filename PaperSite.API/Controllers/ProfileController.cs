using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaperSite.Application.Common.Responses;
using PaperSite.Application.DTOs.Profile;
using PaperSite.Application.Interfaces;

namespace PaperSite.API.Controllers;

[Authorize]
public class ProfileController : BaseController
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }
    /// <summary>
    /// دریافت اطلاعات پروفایل کاربر جاری
    /// </summary>
    /// <returns>اطلاعات حساب کاربری و پروفایل کاربر</returns>
    [HttpGet]
    [ProducesResponseType(typeof(BaseResponse<ProfileDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProfile()
    {
        return Ok(await _profileService.GetProfileAsync(CurrentUserId));
    }
    /// <summary>
    /// بروزرسانی اطلاعات پروفایل کاربر
    /// </summary>
    /// <param name="request">اطلاعات جدید پروفایل کاربر</param>
    /// <returns>نتیجه عملیات بروزرسانی پروفایل</returns>
    [HttpPut]
    [ProducesResponseType(typeof(BaseResponse<ProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<ProfileDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProfile(UpdateProfileRequest request)
    {
        var result = await _profileService.UpdateProfileAsync(CurrentUserId, request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
    /// <summary>
    /// تغییر رمز عبور کاربر
    /// </summary>
    /// <param name="request">
    /// شامل رمز عبور فعلی و رمز عبور جدید
    /// </param>
    /// <returns>نتیجه عملیات تغییر رمز عبور</returns>
    [HttpPost]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        var result = await _profileService.ChangePasswordAsync(CurrentUserId, request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}
