using PaperSite.Application.DTOs.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaperSite.Application.Common.Responses;
using PaperSite.Application.DTOs.Category;
using PaperSite.Application.Interfaces;

namespace PaperSite.API.Controllers;

public class CategoryController : BaseController
{
    private readonly ICategoryService _service;

    public CategoryController(ICategoryService service)
    {
        _service = service;
    }
    /// <summary>
    /// دریافت لیست تمامی دسته‌بندی‌ها
    /// </summary>
    /// <returns>لیست دسته‌بندی‌های موجود در سیستم</returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BaseResponse<IEnumerable<CategoryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }
    /// <summary>
    /// دریافت اطلاعات یک دسته‌بندی
    /// </summary>
    /// <param name="id">شناسه دسته‌بندی</param>
    /// <returns>اطلاعات دسته‌بندی موردنظر</returns>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BaseResponse<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<CategoryDto>), StatusCodes.Status404NotFound)]

    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }
    /// <summary>
    /// ایجاد دسته‌بندی جدید
    /// </summary>
    /// <param name="dto">اطلاعات موردنیاز برای ایجاد دسته‌بندی</param>
    /// <returns>دسته‌بندی ایجادشده</returns>
    [HttpPost]
    [Authorize(Policy = "AdminPolicy")]
    [ProducesResponseType(typeof(BaseResponse<CategoryDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(BaseResponse<CategoryDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(CategoryCreateDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return result.IsSuccess ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result) : BadRequest(result);
    }
    /// <summary>
    /// ویرایش اطلاعات دسته‌بندی
    /// </summary>
    /// <param name="id">شناسه دسته‌بندی</param>
    /// <param name="dto">اطلاعات جدید دسته‌بندی</param>
    /// <returns>نتیجه عملیات بروزرسانی دسته‌بندی</returns>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AdminPolicy")]
    [ProducesResponseType(typeof(BaseResponse<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<CategoryDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, CategoryUpdateDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }
    /// <summary>
    /// حذف دسته‌بندی
    /// </summary>
    /// <param name="id">شناسه دسته‌بندی</param>
    /// <returns>نتیجه عملیات حذف دسته‌بندی</returns>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminPolicy")]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(id);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Returns a page of results. Defaults: PageNumber=1, PageSize=10 (maximum 100).
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BaseResponse<PagedResult<CategoryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaged([FromQuery] PaginationRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _service.GetPagedAsync(request, cancellationToken));
    }
}
