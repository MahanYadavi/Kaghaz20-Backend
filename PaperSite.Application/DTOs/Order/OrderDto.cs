namespace PaperSite.Application.DTOs.Order;

public class OrderDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string ReceiverFullName { get; set; } = string.Empty;
    public string ReceiverPhoneNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public IReadOnlyList<OrderItemDto> Items { get; set; } = Array.Empty<OrderItemDto>();
}
