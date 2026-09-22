using FluentValidation;
using PaperSite.Application.DTOs.Address;

namespace PaperSite.Application.Validators.Address;

public class UpdateAddressRequestValidator : AbstractValidator<UpdateAddressRequest>
{
    public UpdateAddressRequestValidator()
    {
        Include(new CreateAddressRequestValidator());
    }
}