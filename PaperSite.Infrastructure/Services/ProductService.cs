using Microsoft.EntityFrameworkCore;
using PaperSite.Application.Common.Responses;
using PaperSite.Application.DTOs.Common;
using PaperSite.Application.DTOs.Product;
using PaperSite.Application.Interfaces;
using PaperSite.Domain.Entities;

namespace PaperSite.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<Category> _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(IRepository<Product> productRepository, IRepository<Category> categoryRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BaseResponse<ProductDto>> CreateAsync(CreateProductDto request)
    {
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
        if (category == null)
        {
            return BaseResponse<ProductDto>.Failure("دسته بندی یافت نشد");
        }

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Stock = request.Stock,
            CategoryId = request.CategoryId,
            Category = category
        };

        await _productRepository.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return BaseResponse<ProductDto>.Success(ToDto(product), "محضول با موفقیت ساخته شد");
    }

    public async Task<BaseResponse<IEnumerable<ProductDto>>> GetAllAsync()
    {
        var products = await _productRepository.Query(true).Include(x => x.Category).ToListAsync();
        return BaseResponse<IEnumerable<ProductDto>>.Success(products.Select(ToDto), "محصولات با موفقیت بازیابی شدند");
    }

    public async Task<BaseResponse<PagedResult<ProductDto>>> SearchAsync(ProductQueryRequest request)
    {

        var query = _productRepository.Query(true).Include(x => x.Category).AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x => x.Name.Contains(search) || (x.Description != null && x.Description.Contains(search)));
        }

        if (request.CategoryId.HasValue)
        {
            query = query.Where(x => x.CategoryId == request.CategoryId.Value);
        }

        var orderedQuery = request.SortBy?.Trim().ToLowerInvariant() switch
        {
            "price" => request.Descending ? query.OrderByDescending(x => x.Price) : query.OrderBy(x => x.Price),
            "stock" => request.Descending ? query.OrderByDescending(x => x.Stock) : query.OrderBy(x => x.Stock),
            "createdat" => request.Descending ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
            _ => request.Descending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name)
        };

        var page = await orderedQuery.ThenBy(x => x.Id).ToPageAsync(request, ToDto);
        return BaseResponse<PagedResult<ProductDto>>.Success(page, "محصولات با موفقیت بازیابی شدند");
    }
    public async Task<BaseResponse<ProductDto>> GetByIdAsync(Guid id)
    {
        var product = await _productRepository.Query(true).Include(x => x.Category).FirstOrDefaultAsync(x => x.Id == id);
        return product == null
            ? BaseResponse<ProductDto>.Failure("محصولی یافت نشد")
            : BaseResponse<ProductDto>.Success(ToDto(product), "محصول با موفقیت بازیابی شدند");
    }

    public async Task<BaseResponse<ProductDto>> UpdateAsync(Guid id, UpdateProductDto request)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
        {
            return BaseResponse<ProductDto>.Failure("محصولی یافت نشد");
        }

        var categoryExists = await _categoryRepository.GetByIdAsync(request.CategoryId);
        if (categoryExists == null)
        {
            return BaseResponse<ProductDto>.Failure("دسته بندی یافت نشد");
        }

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.Stock = request.Stock;
        product.CategoryId = request.CategoryId;
        _productRepository.Update(product);
        await _unitOfWork.SaveChangesAsync();

        product.Category = categoryExists;
        return BaseResponse<ProductDto>.Success(ToDto(product), "محصول با موفقیت به روزرسانی شد");
    }

    public async Task<BaseResponse<bool>> DeleteAsync(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
        {
            return BaseResponse<bool>.Failure("محصولی یافت نشد");
        }

        _productRepository.Delete(product);
        await _unitOfWork.SaveChangesAsync();
        return BaseResponse<bool>.Success(true, "محصول با موفقیت حذف شد");
    }

    public async Task<BaseResponse<PagedResult<ProductDto>>> GetPagedAsync(PaginationRequest request, CancellationToken cancellationToken = default)
    {
        var page = await _productRepository.Query(true).Include(x => x.Category).OrderBy(x => x.Name).ThenBy(x => x.Id)
            .ToPageAsync(request, ToDto, cancellationToken);
        return BaseResponse<PagedResult<ProductDto>>.Success(page);
    }
    private static ProductDto ToDto(Product product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        Description = product.Description,
        Price = product.Price,
        Stock = product.Stock,
        CategoryId = product.CategoryId,
        CategoryName = product.Category?.Name
    };
}
