using PaperSite.Application.Common.Responses;
using PaperSite.Application.DTOs.Cart;

namespace PaperSite.Application.Interfaces;

public interface ICartService
{
    Task<BaseResponse<CartDto>> AddToCartAsync(Guid userId, AddToCartRequest request);
    Task<BaseResponse<CartDto>> RemoveFromCartAsync(Guid userId, Guid productId);
    Task<BaseResponse<CartDto>> UpdateQuantityAsync(Guid userId, UpdateCartItemQuantityRequest request);
    Task<BaseResponse<bool>> ClearCartAsync(Guid userId);
    Task<BaseResponse<CartDto>> GetCartAsync(Guid userId);
}
