using PaperSite.Domain.Enums;

namespace PaperSite.Application.DTOs.Order;

public class ChangeOrderStatusRequest
{
    public OrderStatus Status { get; set; }
}
