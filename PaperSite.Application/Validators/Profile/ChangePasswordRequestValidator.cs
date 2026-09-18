using FluentValidation;
using PaperSite.Application.DTOs.Profile;

namespace PaperSite.Application.Validators.Profile;

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty()
            .WithMessage("رمز عبور فعلی الزامی است.");

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithMessage("رمز عبور جدید الزامی است.")
            .MinimumLength(8)
            .WithMessage("رمز عبور جدید باید حداقل ۸ کاراکتر باشد.")
            .MaximumLength(100)
            .WithMessage("رمز عبور جدید نمی‌تواند بیشتر از ۱۰۰ کاراکتر باشد.")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$")
            .WithMessage("رمز عبور جدید باید شامل حداقل یک حرف کوچک، یک حرف بزرگ و یک عدد باشد."); 
    }
}
