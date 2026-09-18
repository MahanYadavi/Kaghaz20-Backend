namespace PaperSite.Application.DTOs.Order;

public class CreateOrderRequest
{
    public string ShippingAddress { get; set; } = string.Empty;
    public string ReceiverFullName { get; set; } = string.Empty;
    public string ReceiverPhoneNumber { get; set; } = string.Empty;
    public List<CreateOrderItemRequest> Items { get; set; } = new();
}
