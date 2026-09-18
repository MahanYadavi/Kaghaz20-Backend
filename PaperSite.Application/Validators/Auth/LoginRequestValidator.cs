using FluentValidation;
using PaperSite.Application.DTOs.Auth;

namespace PaperSite.Application.Validators.Auth;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("لطفا ایمیل را وارد کنید")
            .EmailAddress().WithMessage("فرمت ایمیل معتبر نیست");
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("لطفا رمز عبور را وارد کنید");
    }
}
