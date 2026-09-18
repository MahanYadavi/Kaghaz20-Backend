using Microsoft.EntityFrameworkCore;
using PaperSite.Application.Common.Responses;
using PaperSite.Application.DTOs.Cart;
using PaperSite.Application.Interfaces;
using PaperSite.Domain.Entities;

namespace PaperSite.Infrastructure.Services;

public class CartService : ICartService
{
    private readonly IRepository<Cart> _cartRepository;
    private readonly IRepository<CartItem> _cartItemRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CartService(IRepository<Cart> cartRepository, IRepository<CartItem> cartItemRepository, IRepository<Product> productRepository, IUnitOfWork unitOfWork)
    {
        _cartRepository = cartRepository;
        _cartItemRepository = cartItemRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BaseResponse<CartDto>> AddToCartAsync(Guid userId, AddToCartRequest request)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId);
        if (product == null)
        {
            return BaseResponse<CartDto>.Failure("محصول یافت نشد");
        }

        if (product.Stock < request.Quantity)
        {
            return BaseResponse<CartDto>.Failure("مقدار درخواستی بیش از موجودی انبار است");
        }

        var cart = await GetOrCreateCartAsync(userId);
        var item = cart.Items.FirstOrDefault(x => x.ProductId == request.ProductId);
        if (item == null)
        {
            cart.Items.Add(new CartItem
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                Product = product,
                Quantity = request.Quantity
            });
        }
        else
        {
            if (product.Stock < item.Quantity + request.Quantity)
            {
                return BaseResponse<CartDto>.Failure("مقدار درخواستی بیش از موجودی انبار است");
            }

            item.Quantity += request.Quantity;
            _cartItemRepository.Update(item);
        }

        _cartRepository.Update(cart);
        await _unitOfWork.SaveChangesAsync();

        return BaseResponse<CartDto>.Success(ToDto(cart), "محصول با موفقیت به سبد خرید اضافه شد");
    }

    public async Task<BaseResponse<CartDto>> RemoveFromCartAsync(Guid userId, Guid productId)
    {
        var cart = await GetCartEntityAsync(userId);
        if (cart == null)
        {
            return BaseResponse<CartDto>.Failure("سبد خرید یافت نشد");
        }

        var item = cart.Items.FirstOrDefault(x => x.ProductId == productId);
        if (item == null)
        {
            return BaseResponse<CartDto>.Failure("آیتم سبد خرید یافت نشد");
        }

        _cartItemRepository.Delete(item);
        await _unitOfWork.SaveChangesAsync();

        cart.Items.Remove(item);
        return BaseResponse<CartDto>.Success(ToDto(cart), "محصول با موفقیت از سبد خرید حذف شد");
    }

    public async Task<BaseResponse<CartDto>> UpdateQuantityAsync(Guid userId, UpdateCartItemQuantityRequest request)
    {
        var cart = await GetCartEntityAsync(userId);
        if (cart == null)
        {
            return BaseResponse<CartDto>.Failure("سبد خرید یافت نشد");
        }

        var item = cart.Items.FirstOrDefault(x => x.ProductId == request.ProductId);
        if (item == null)
        {
            return BaseResponse<CartDto>.Failure("آیتم سبد خرید یافت نشد");
        }

        if (item.Product.Stock < request.Quantity)
        {
            return BaseResponse<CartDto>.Failure("مقدار درخواستی بیش از موجودی انبار است");
        }

        item.Quantity = request.Quantity;
        _cartItemRepository.Update(item);
        await _unitOfWork.SaveChangesAsync();

        return BaseResponse<CartDto>.Success(ToDto(cart), "تعداد آیتم سبد خرید با موفقیت به‌روزرسانی شد");
    }

    public async Task<BaseResponse<bool>> ClearCartAsync(Guid userId)
    {
        var cart = await GetCartEntityAsync(userId);
        if (cart == null)
        {
            return BaseResponse<bool>.Success(true, "سبد خرید خالی است");
        }

        foreach (var item in cart.Items.ToList())
        {
            _cartItemRepository.Delete(item);
        }

        await _unitOfWork.SaveChangesAsync();
        return BaseResponse<bool>.Success(true, "سبد خرید با موفقیت خالی شد");
    }

    public async Task<BaseResponse<CartDto>> GetCartAsync(Guid userId)
    {
        var cart = await GetOrCreateCartAsync(userId);
        await _unitOfWork.SaveChangesAsync();
        return BaseResponse<CartDto>.Success(ToDto(cart), "سبد خرید با موفقیت دریافت شد");
    }

    private async Task<Cart> GetOrCreateCartAsync(Guid userId)
    {
        var cart = await GetCartEntityAsync(userId);
        if (cart != null)
        {
            return cart;
        }

        cart = new Cart
        {
            Id = Guid.NewGuid(),
            UserId = userId
        };
        await _cartRepository.AddAsync(cart);
        return cart;
    }

    private async Task<Cart?> GetCartEntityAsync(Guid userId)
    {
        return await _cartRepository.Query()
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x => x.UserId == userId);
    }

    private static CartDto ToDto(Cart cart) => new()
    {
        Id = cart.Id,
        UserId = cart.UserId,
        Items = cart.Items.Where(x => !x.IsDeleted).Select(x => new CartItemDto
        {
            ProductId = x.ProductId,
            ProductName = x.Product?.Name ?? string.Empty,
            UnitPrice = x.Product?.Price ?? 0,
            Quantity = x.Quantity,
            TotalPrice = (x.Product?.Price ?? 0) * x.Quantity
        }).ToList(),
        TotalAmount = cart.Items.Where(x => !x.IsDeleted).Sum(x => (x.Product?.Price ?? 0) * x.Quantity)
    };
}
