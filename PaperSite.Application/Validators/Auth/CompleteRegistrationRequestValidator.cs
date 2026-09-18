using FluentValidation;
using PaperSite.Application.DTOs.Auth;

namespace PaperSite.Application.Validators.Auth;

public class CompleteRegistrationRequestValidator:AbstractValidator<CompleteRegistrationRequest>
{
    public CompleteRegistrationRequestValidator()
    {
        RuleFor(x => x.RegistrationToken)
            .NotEmpty()
            .WithMessage("توکن ثبت‌نام الزامی است.");

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("نام الزامی است.")
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("نام خانوادگی الزامی است.")
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .EmailAddress()
            .WithMessage("فرمت ایمیل معتبر نیست.")
            .MaximumLength(255)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}