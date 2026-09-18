using PaperSite.Application.DTOs.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaperSite.Application.Common.Responses;
using PaperSite.Application.DTOs.Order;
using PaperSite.Application.Interfaces;

namespace PaperSite.API.Controllers;

[Authorize]
public class OrderController : BaseController
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }
    /// <summary>
    /// ایجاد سفارش جدید
    /// </summary>
    /// <param name="request">اطلاعات موردنیاز برای ثبت سفارش</param>
    /// <returns>سفارش ایجادشده به همراه اطلاعات پرداخت و پیگیری</returns>
    [HttpPost]
    [ProducesResponseType(typeof(BaseResponse<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<OrderDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrder(CreateOrderRequest request)
    {
        var result = await _orderService.CreateAsync(CurrentUserId, request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
    /// <summary>
    /// دریافت جزئیات سفارش
    /// </summary>
    /// <param name="id">شناسه سفارش</param>
    /// <returns>اطلاعات کامل سفارش شامل اقلام، مبلغ و وضعیت</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BaseResponse<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<OrderDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderDetails(Guid id)
    {
        var isAdmin = User.IsInRole("Admin");
        var result = await _orderService.GetByIdAsync(CurrentUserId, id, isAdmin);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }
    /// <summary>
    /// دریافت سفارش‌های کاربر
    /// </summary>
    /// <returns>لیست تمامی سفارش‌های ثبت‌شده توسط کاربر جاری</returns>
    [HttpGet]
    [Authorize(Policy = "CustomerPolicy")]
    [ProducesResponseType(typeof(BaseResponse<IEnumerable<OrderDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserOrders()
    {
        return Ok(await _orderService.GetUserOrdersAsync(CurrentUserId));
    }
    /// <summary>
    /// تغییر وضعیت سفارش
    /// </summary>
    /// <param name="id">شناسه سفارش</param>
    /// <param name="request">وضعیت جدید سفارش</param>
    /// <returns>نتیجه عملیات بروزرسانی وضعیت سفارش</returns>
    [HttpPatch("{id:guid}")]
    [Authorize(Policy = "AdminPolicy")]
    [ProducesResponseType(typeof(BaseResponse<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<OrderDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeOrderStatus(Guid id, ChangeOrderStatusRequest request)
    {
        var result = await _orderService.ChangeStatusAsync(id, request);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Returns a page of results. Defaults: PageNumber=1, PageSize=10 (maximum 100).
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "CustomerPolicy")]
    [ProducesResponseType(typeof(BaseResponse<PagedResult<OrderDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserOrdersPaged([FromQuery] PaginationRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _orderService.GetUserOrdersPagedAsync(CurrentUserId, request, cancellationToken));
    }
}
