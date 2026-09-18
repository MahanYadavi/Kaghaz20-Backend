namespace PaperSite.Application.DTOs.Cart;

public class UpdateCartItemQuantityRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
