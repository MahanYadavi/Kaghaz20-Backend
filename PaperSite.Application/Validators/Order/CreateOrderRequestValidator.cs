using FluentValidation;
using PaperSite.Application.DTOs.Order;

namespace PaperSite.Application.Validators.Order;

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.ShippingAddress)
            .NotEmpty().WithMessage("آدرس ارسال الزامی است.")
            .MaximumLength(1000).WithMessage("آدرس ارسال بیش از حد طولانی است.");

        RuleFor(x => x.ReceiverFullName)
            .NotEmpty().WithMessage("نام گیرنده الزامی است.")
            .MaximumLength(200).WithMessage("نام گیرنده بیش از حد طولانی است.");

        RuleFor(x => x.ReceiverPhoneNumber)
            .NotEmpty().WithMessage("شماره موبایل گیرنده الزامی است.")
            .Matches(@"^09\d{9}$").WithMessage("شماره موبایل گیرنده معتبر نیست.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("سفارش باید حداقل یک آیتم داشته باشد.")
            .Must(x => x.Count <= 50)
            .WithMessage("تعداد آیتم‌های سفارش بیش از حد مجاز است.");

        RuleForEach(x => x.Items)
            .SetValidator(new CreateOrderItemRequestValidator());
    }
}

public class CreateOrderItemRequestValidator : AbstractValidator<CreateOrderItemRequest>
{
    public CreateOrderItemRequestValidator()
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
