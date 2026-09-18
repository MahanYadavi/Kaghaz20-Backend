using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaperSite.Application.Common.Responses;
using PaperSite.Application.DTOs.Common;
using PaperSite.Application.DTOs.Product;
using PaperSite.Application.Interfaces;

namespace PaperSite.API.Controllers;

public class ProductController : BaseController
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }
    /// <summary>
    /// ایجاد محصول جدید
    /// </summary>
    /// <param name="request">اطلاعات موردنیاز برای ایجاد محصول</param>
    /// <returns>محصول ایجادشده</returns>
    [HttpPost]
    [Authorize(Policy = "AdminPolicy")]
    [ProducesResponseType(typeof(BaseResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<ProductDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(CreateProductDto request)
    {
        var result = await _productService.CreateAsync(request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
    /// <summary>
    /// دریافت لیست تمامی محصولات
    /// </summary>
    /// <returns>لیست محصولات موجود در سیستم</returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BaseResponse<IEnumerable<ProductDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _productService.GetAllAsync());
    }
    /// <summary>
    /// جستجو و فیلتر محصولات
    /// </summary>
    /// <param name="request">
    /// پارامترهای جستجو شامل نام محصول، دسته‌بندی، محدوده قیمت، صفحه‌بندی و سایر فیلترها
    /// </param>
    /// <returns>لیست محصولات منطبق با معیارهای جستجو</returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BaseResponse<PagedResult<ProductDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] ProductQueryRequest request)
    {
        return Ok(await _productService.SearchAsync(request));
    }
    /// <summary>
    /// دریافت اطلاعات یک محصول
    /// </summary>
    /// <param name="id">شناسه محصول</param>
    /// <returns>اطلاعات کامل محصول</returns>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BaseResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<ProductDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _productService.GetByIdAsync(id);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }
    /// <summary>
    /// بروزرسانی اطلاعات محصول
    /// </summary>
    /// <param name="id">شناسه محصول</param>
    /// <param name="request">اطلاعات جدید محصول</param>
    /// <returns>نتیجه عملیات بروزرسانی محصول</returns>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AdminPolicy")]
    [ProducesResponseType(typeof(BaseResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<ProductDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, UpdateProductDto request)
    {
        var result = await _productService.UpdateAsync(id, request);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }
    /// <summary>
    /// حذف محصول
    /// </summary>
    /// <param name="id">شناسه محصول</param>
    /// <returns>نتیجه عملیات حذف محصول</returns>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminPolicy")]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _productService.DeleteAsync(id);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Returns a page of results. Defaults: PageNumber=1, PageSize=10 (maximum 100).
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BaseResponse<PagedResult<ProductDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaged([FromQuery] PaginationRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _productService.GetPagedAsync(request, cancellationToken));
    }
}
