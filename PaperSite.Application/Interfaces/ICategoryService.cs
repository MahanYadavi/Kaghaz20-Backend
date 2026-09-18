using PaperSite.Application.DTOs.Common;
using PaperSite.Application.Common.Responses;
using PaperSite.Application.DTOs.Category;

namespace PaperSite.Application.Interfaces;

public interface ICategoryService
{
    Task<BaseResponse<IEnumerable<CategoryDto>>> GetAllAsync();
    Task<BaseResponse<CategoryDto>> GetByIdAsync(Guid id);
    Task<BaseResponse<CategoryDto>> CreateAsync(CategoryCreateDto request);
    Task<BaseResponse<CategoryDto>> UpdateAsync(Guid id, CategoryUpdateDto request);
    Task<BaseResponse<bool>> DeleteAsync(Guid id);
    Task<BaseResponse<PagedResult<CategoryDto>>> GetPagedAsync(PaginationRequest request, CancellationToken cancellationToken = default);
}
