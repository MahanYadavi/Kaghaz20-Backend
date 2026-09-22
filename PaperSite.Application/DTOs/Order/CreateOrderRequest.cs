namespace PaperSite.Application.DTOs.Order;

public class CreateOrderRequest
{
    public Guid AddressId { get; set; }

    public List<CreateOrderItemRequest> Items { get; set; } = new();

    /// <summary>
    /// روش ارسال: <c>express</c> (پیک موتوری تهران) یا <c>tipax</c> (تیپاکس).
    /// </summary>
    public string ShippingMethod { get; set; } = string.Empty;
}
