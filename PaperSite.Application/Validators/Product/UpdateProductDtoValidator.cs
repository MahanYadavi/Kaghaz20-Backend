using FluentValidation;
using PaperSite.Application.DTOs.Product;

namespace PaperSite.Application.Validators.Product;

public class UpdateProductDtoValidator : AbstractValidator<UpdateProductDto>
{
    public UpdateProductDtoValidator()
    {
        RuleFor(x => x.Name)
                  .NotEmpty().WithMessage("نام محصول الزامی است.")
                  .MaximumLength(200).WithMessage("نام محصول بیش از حد طولانی است.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("توضیحات محصول بیش از حد طولانی است.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("قیمت باید بزرگ‌تر از صفر باشد.")
            .LessThanOrEqualTo(1_000_000_000)
            .WithMessage("قیمت وارد شده بیش از حد مجاز است.");

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("موجودی نمی‌تواند منفی باشد.")
            .LessThanOrEqualTo(1_000_000)
            .WithMessage("موجودی وارد شده بیش از حد مجاز است.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("انتخاب دسته‌بندی الزامی است.");
    }
}
