using FluentValidation;
using PaperSite.Application.Common;
using PaperSite.Application.DTOs.Address;

namespace PaperSite.Application.Validators.Address;

public class CreateAddressRequestValidator : AbstractValidator<CreateAddressRequest>
{
    public CreateAddressRequestValidator()
    {
        RuleFor(x => x.Title)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("عنوان آدرس الزامی است.")
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage("عنوان آدرس الزامی است.")
            .MaximumLength(50)
            .WithMessage("عنوان آدرس حداکثر ۵۰ کاراکتر است.");

        RuleFor(x => x.Province)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("استان الزامی است.")
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage("استان الزامی است.")
            .MaximumLength(50)
            .WithMessage("استان حداکثر ۵۰ کاراکتر است.");

        RuleFor(x => x.City)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("شهر الزامی است.")
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage("شهر الزامی است.")
            .MaximumLength(60)
            .WithMessage("شهر حداکثر ۶۰ کاراکتر است.");

        RuleFor(x => x.FullAddress)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("نشانی کامل الزامی است.")
            .Must(x => x.Trim().Length >= 10)
            .WithMessage("نشانی کامل باید حداقل ۱۰ کاراکتر باشد.")
            .MaximumLength(400)
            .WithMessage("نشانی کامل حداکثر ۴۰۰ کاراکتر است.");

        RuleFor(x => x.PostalCode)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("کد پستی الزامی است.")
            .Must(x => x.Length == 10 && x.All(char.IsAsciiDigit))
            .WithMessage("کد پستی باید دقیقاً ۱۰ رقم باشد.");
    }
}