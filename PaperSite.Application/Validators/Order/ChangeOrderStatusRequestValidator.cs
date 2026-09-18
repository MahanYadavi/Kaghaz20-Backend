using FluentValidation;
using PaperSite.Application.DTOs.Order;
using PaperSite.Domain.Enums;

namespace PaperSite.Application.Validators.Order;

public class ChangeOrderStatusRequestValidator : AbstractValidator<ChangeOrderStatusRequest>
{
    public ChangeOrderStatusRequestValidator()
    {
        RuleFor(x => x.Status).IsInEnum().NotEqual(OrderStatus.Pending);
    }
}
