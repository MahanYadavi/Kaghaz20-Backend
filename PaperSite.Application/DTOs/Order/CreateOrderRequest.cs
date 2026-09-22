namespace PaperSite.Application.DTOs.Order;

public class CreateOrderRequest
{
    public Guid AddressId { get; set; }

    public List<CreateOrderItemRequest> Items { get; set; } = new();
}
