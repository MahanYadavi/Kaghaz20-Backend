using PaperSite.Application.Common.Responses;
using PaperSite.Application.DTOs.Common;
using PaperSite.Application.DTOs.Product;

namespace PaperSite.Application.Interfaces;

public interface IProductService
{
    Task<BaseResponse<ProductDto>> CreateAsync(CreateProductDto request);
    Task<BaseResponse<ProductDto>> GetByIdAsync(Guid id);
    Task<BaseResponse<IEnumerable<ProductDto>>> GetAllAsync();
    Task<BaseResponse<PagedResult<ProductDto>>> SearchAsync(ProductQueryRequest request);
    Task<BaseResponse<ProductDto>> UpdateAsync(Guid id, UpdateProductDto request);
    Task<BaseResponse<bool>> DeleteAsync(Guid id);
    Task<BaseResponse<PagedResult<ProductDto>>> GetPagedAsync(PaginationRequest request, CancellationToken cancellationToken = default);
}
