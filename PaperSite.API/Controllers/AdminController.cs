using PaperSite.Application.DTOs.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaperSite.Application.Common.Responses;
using PaperSite.Application.DTOs.Admin;
using PaperSite.Application.DTOs.Order;
using PaperSite.Application.Interfaces;

namespace PaperSite.API.Controllers;

[Authorize(Policy = "AdminPolicy")]
public class AdminController : BaseController
{
    private readonly IAdminService _adminService;
    private readonly IOrderService _orderService;

    public AdminController(IAdminService adminService, IOrderService orderService)
    {
        _adminService = adminService;
        _orderService = orderService;
    }
    /// <summary>
    /// دریافت لیست تمامی کاربران
    /// </summary>
    /// <returns>لیست کاربران ثبت‌شده در سیستم</returns>
    [HttpGet]
    [ProducesResponseType(typeof(BaseResponse<IEnumerable<UserDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllUsers()
    {
        return Ok(await _adminService.GetAllUsersAsync());
    }
    /// <summary>
    /// دریافت جزئیات یک کاربر
    /// </summary>
    /// <param name="id">شناسه کاربر</param>
    /// <returns>اطلاعات کامل کاربر موردنظر</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BaseResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<UserDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserDetails(Guid id)
    {
        var result = await _adminService.GetUserDetailsAsync(id);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }
    /// <summary>
    /// حذف کاربر
    /// </summary>
    /// <param name="id">شناسه کاربر</param>
    /// <returns>نتیجه عملیات حذف کاربر</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var result = await _adminService.DeleteUserAsync(id, CurrentUserId);
        if (result.IsSuccess) return Ok(result);
        return string.Equals(result.Message, "User not found", StringComparison.OrdinalIgnoreCase)
            ? NotFound(result)
            : BadRequest(result);
    }
    /// <summary>
    /// دریافت لیست تمامی سفارش‌ها
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(BaseResponse<IEnumerable<OrderDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllOrders()
    {
        return Ok(await _orderService.GetAllOrdersAsync());
    }
    /// <summary>
    /// تغییر وضعیت سفارش
    /// </summary>
    /// <param name="id">شناسه سفارش</param>
    /// <param name="request">اطلاعات وضعیت جدید سفارش</param>
    /// <returns>نتیجه عملیات تغییر وضعیت سفارش</returns>
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(BaseResponse<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<OrderDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeOrderStatus(Guid id, ChangeOrderStatusRequest request)
    {
        var result = await _orderService.ChangeStatusAsync(id, request);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }
    /// <summary>
    /// دریافت آمار و اطلاعات داشبورد
    /// </summary>
    /// <returns>آمار کلی کاربران، سفارش‌ها و سایر اطلاعات داشبورد</returns>
    [HttpGet]
    [ProducesResponseType(typeof(BaseResponse<DashboardStatisticsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DashboardStatistics()
    {
        return Ok(await _adminService.GetDashboardStatisticsAsync());
    }

    /// <summary>
    /// Returns a page of results. Defaults: PageNumber=1, PageSize=10 (maximum 100).
    /// </summary>
    [HttpGet]

    [ProducesResponseType(typeof(BaseResponse<PagedResult<UserDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsersPaged([FromQuery] PaginationRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _adminService.GetUsersPagedAsync(request, cancellationToken));
    }

    /// <summary>
    /// Returns a page of results. Defaults: PageNumber=1, PageSize=10 (maximum 100).
    /// </summary>
    [HttpGet]

    [ProducesResponseType(typeof(BaseResponse<PagedResult<OrderDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrdersPaged([FromQuery] PaginationRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _orderService.GetAllOrdersPagedAsync(request, cancellationToken));
    }
}
