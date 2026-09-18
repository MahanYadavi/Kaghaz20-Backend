using FluentValidation;
using PaperSite.Application.DTOs.Cart;

namespace PaperSite.Application.Validators.Cart;

public class AddToCartRequestValidator : AbstractValidator<AddToCartRequest>
{
    public AddToCartRequestValidator()
    {
        RuleFor(x => x.ProductId)
    .NotEmpty()
    .WithMessage("انتخاب محصول الزامی است.");

        RuleFor(x => x.Quantity)
               .GreaterThan(0)
               .WithMessage("تعداد باید بزرگ‌تر از صفر باشد.")
               .LessThanOrEqualTo(1000)
               .WithMessage("تعداد وارد شده بیش از حد مجاز است.");
    }
}
