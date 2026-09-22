using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaperSite.Application.Common.Responses;
using PaperSite.Application.DTOs.Address;
using PaperSite.Application.Interfaces;

namespace PaperSite.API.Controllers;

[Authorize]
public class AddressController : BaseController
{
    private readonly IAddressService _addressService;

    public AddressController(IAddressService addressService)
    {
        _addressService = addressService;
    }

    /// <summary>
    /// دریافت آدرس‌های کاربر جاری
    /// </summary>
    /// <remarks>آدرس پیش‌فرض اول، سپس جدیدترین. اگر آدرسی نباشد لیست خالی برگردانده می‌شود.</remarks>
    [HttpGet]
    [ProducesResponseType(typeof(BaseResponse<IEnumerable<AddressDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyAddresses(CancellationToken cancellationToken)
    {
        return Ok(await _addressService.GetMyAddressesAsync(CurrentUserId, cancellationToken));
    }

    /// <summary>
    /// افزودن آدرس جدید
    /// </summary>
    /// <param name="request">اطلاعات آدرس جدید</param>
    /// <param name="cancellationToken">توکن لغو درخواست</param>
    /// <remarks>مالک آدرس فقط از توکن خوانده می‌شود. اولین آدرس کاربر پیش‌فرض می‌شود.</remarks>
    [HttpPost]
    [ProducesResponseType(typeof(BaseResponse<AddressDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<AddressDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(CreateAddressRequest request, CancellationToken cancellationToken)
    {
        var result = await _addressService.CreateAsync(CurrentUserId, request, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// ویرایش آدرس
    /// </summary>
    /// <param name="id">شناسه آدرس</param>
    /// <param name="request">اطلاعات جدید آدرس</param>
    /// <param name="cancellationToken">توکن لغو درخواست</param>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(BaseResponse<AddressDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<AddressDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, UpdateAddressRequest request, CancellationToken cancellationToken)
    {
        var result = await _addressService.UpdateAsync(CurrentUserId, id, request, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// حذف آدرس (حذف نرم)
    /// </summary>
    /// <param name="id">شناسه آدرس</param>
    /// <param name="cancellationToken">توکن لغو درخواست</param>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _addressService.DeleteAsync(CurrentUserId, id, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// انتخاب آدرس پیش‌فرض
    /// </summary>
    /// <param name="id">شناسه آدرس</param>
    /// <param name="cancellationToken">توکن لغو درخواست</param>
    [HttpPost("{id:guid}")]
    [ProducesResponseType(typeof(BaseResponse<AddressDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<AddressDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetDefault(Guid id, CancellationToken cancellationToken)
    {
        var result = await _addressService.SetDefaultAsync(CurrentUserId, id, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}