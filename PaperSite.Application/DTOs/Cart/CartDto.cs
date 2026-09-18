namespace PaperSite.Application.DTOs.Cart;

public class CartDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public IReadOnlyList<CartItemDto> Items { get; set; } = Array.Empty<CartItemDto>();
    public decimal TotalAmount { get; set; }
}
