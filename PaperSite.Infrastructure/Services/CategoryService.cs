using Microsoft.EntityFrameworkCore;
using PaperSite.Application.DTOs.Common;
using PaperSite.Application.Common.Responses;
using PaperSite.Application.DTOs.Category;
using PaperSite.Application.Interfaces;
using PaperSite.Domain.Entities;

namespace PaperSite.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly IRepository<Category> _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(IRepository<Category> categoryRepository, IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BaseResponse<CategoryDto>> CreateAsync(CategoryCreateDto request)
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description
        };

        await _categoryRepository.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return BaseResponse<CategoryDto>.Success(ToDto(category), "دسته‌بندی با موفقیت ایجاد شد");
    }

    public async Task<BaseResponse<IEnumerable<CategoryDto>>> GetAllAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return BaseResponse<IEnumerable<CategoryDto>>.Success(categories.Select(ToDto), "دسته‌بندی‌ها با موفقیت بازیابی شدند");
    }

    public async Task<BaseResponse<CategoryDto>> GetByIdAsync(Guid id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        return category == null
            ? BaseResponse<CategoryDto>.Failure("دسته‌بندی یافت نشد")
            : BaseResponse<CategoryDto>.Success(ToDto(category), "دسته‌بندی با موفقیت بازیابی شد");
    }

    public async Task<BaseResponse<CategoryDto>> UpdateAsync(Guid id, CategoryUpdateDto request)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            return BaseResponse<CategoryDto>.Failure("دسته‌بندی یافت نشد");
        }

        category.Name = request.Name;
        category.Description = request.Description;
        _categoryRepository.Update(category);
        await _unitOfWork.SaveChangesAsync();

        return BaseResponse<CategoryDto>.Success(ToDto(category), "دسته‌بندی با موفقیت به‌روزرسانی شد");
    }

    public async Task<BaseResponse<bool>> DeleteAsync(Guid id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            return BaseResponse<bool>.Failure("دسته‌بندی یافت نشد");
        }

        _categoryRepository.Delete(category);
        await _unitOfWork.SaveChangesAsync();
        return BaseResponse<bool>.Success(true, "دسته‌بندی با موفقیت حذف شد");
    }

    public async Task<BaseResponse<PagedResult<CategoryDto>>> GetPagedAsync(PaginationRequest request, CancellationToken cancellationToken = default)
    {
        var page = await _categoryRepository.Query(true).OrderBy(x => x.Name).ThenBy(x => x.Id)
            .ToPageAsync(request, ToDto, cancellationToken);
        return BaseResponse<PagedResult<CategoryDto>>.Success(page);
    }
    private static CategoryDto ToDto(Category category) => new()
    {
        Id = category.Id,
        Name = category.Name,
        Description = category.Description
    };
}
