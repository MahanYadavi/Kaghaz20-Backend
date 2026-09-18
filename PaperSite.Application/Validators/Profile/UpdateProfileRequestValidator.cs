using FluentValidation;
using PaperSite.Application.DTOs.Profile;

namespace PaperSite.Application.Validators.Profile;

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.FirstName).MaximumLength(100);
        RuleFor(x => x.LastName).MaximumLength(100);
        RuleFor(x => x.UserName).MaximumLength(100);
        RuleFor(x => x.PhoneNumber).MaximumLength(20);
    }
}
