using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaperSite.Application.Common.Responses;
using PaperSite.Application.DTOs.Cart;
using PaperSite.Application.Interfaces;

namespace PaperSite.API.Controllers;

[Authorize]
public class CartController : BaseController
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }
    /// <summary>
    /// افزودن محصول به سبد خرید
    /// </summary>
    /// <param name="request">اطلاعات محصول و تعداد موردنظر برای افزودن به سبد خرید</param>
    /// <returns>نتیجه عملیات افزودن محصول به سبد خرید</returns>
    [HttpPost]
    [ProducesResponseType(typeof(BaseResponse<CartDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<CartDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddToCart(AddToCartRequest request)
    {
        var result = await _cartService.AddToCartAsync(CurrentUserId, request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
    /// <summary>
    /// حذف محصول از سبد خرید
    /// </summary>
    /// <param name="productId">شناسه محصول موردنظر</param>
    /// <returns>نتیجه عملیات حذف محصول از سبد خرید</returns>
    [HttpDelete("{productId:guid}")]
    [ProducesResponseType(typeof(BaseResponse<CartDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<CartDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveFromCart(Guid productId)
    {
        var result = await _cartService.RemoveFromCartAsync(CurrentUserId, productId);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }
    /// <summary>
    /// بروزرسانی تعداد یک محصول در سبد خرید
    /// </summary>
    /// <param name="request">اطلاعات محصول و تعداد جدید</param>
    /// <returns>نتیجه عملیات بروزرسانی سبد خرید</returns>
    [HttpPut]
    [ProducesResponseType(typeof(BaseResponse<CartDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<CartDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateQuantity(UpdateCartItemQuantityRequest request)
    {
        var result = await _cartService.UpdateQuantityAsync(CurrentUserId, request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
    /// <summary>
    /// حذف تمامی اقلام سبد خرید
    /// </summary>
    /// <returns>نتیجه عملیات پاک‌سازی سبد خرید</returns>
    [HttpDelete]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ClearCart()
    {
        return Ok(await _cartService.ClearCartAsync(CurrentUserId));
    }
    /// <summary>
    /// دریافت اطلاعات سبد خرید کاربر
    /// </summary>
    /// <returns>لیست اقلام موجود در سبد خرید به همراه جمع مبالغ</returns>
    [HttpGet]
    [ProducesResponseType(typeof(BaseResponse<CartDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCart()
    {
        return Ok(await _cartService.GetCartAsync(CurrentUserId));
    }
}
