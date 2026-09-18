using PaperSite.Application.DTOs.Common;
using Microsoft.EntityFrameworkCore;
using PaperSite.Application.Common.Responses;
using PaperSite.Application.DTOs.Order;
using PaperSite.Application.Interfaces;
using PaperSite.Domain.Entities;
using PaperSite.Domain.Enums;
using PaperSite.Infrastructure.Persistence;

namespace PaperSite.Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApplicationDbContext _dbContext;

    public OrderService(IRepository<Order> orderRepository, IRepository<Product> productRepository, IUnitOfWork unitOfWork, ApplicationDbContext dbContext)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _dbContext = dbContext;
    }

    public async Task<BaseResponse<OrderDto>> CreateAsync(Guid userId, CreateOrderRequest request)
    {
        var groupedItems = request.Items
            .GroupBy(x => x.ProductId)
            .Select(x => new CreateOrderItemRequest { ProductId = x.Key, Quantity = x.Sum(i => i.Quantity) })
            .ToList();

        var productIds = groupedItems.Select(x => x.ProductId).ToList();
        var products = await _productRepository.Query(true).Where(x => productIds.Contains(x.Id)).ToListAsync();

        if (products.Count != productIds.Count)
        {
            return BaseResponse<OrderDto>.Failure(" محصولی یافت نشد");
        }

        foreach (var item in groupedItems)
        {
            var product = products.First(x => x.Id == item.ProductId);
            if (product.Stock < item.Quantity)
            {
                return BaseResponse<OrderDto>.Failure($"موجودی محصول '{product.Name}' کافی نیست");
            }
        }

        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Status = OrderStatus.Pending,
            ShippingAddress = request.ShippingAddress,
            ReceiverFullName = request.ReceiverFullName,
            ReceiverPhoneNumber = request.ReceiverPhoneNumber
        };

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        foreach (var item in groupedItems)
        {
            var product = products.First(x => x.Id == item.ProductId);
            var updated = await _dbContext.Products
                .Where(x => x.Id == product.Id && x.Stock >= item.Quantity)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.Stock, x => x.Stock - item.Quantity)
                    .SetProperty(x => x.UpdatedAt, DateTime.UtcNow));

            if (updated == 0)
            {
                await transaction.RollbackAsync();
                return BaseResponse<OrderDto>.Failure($"موجودی محصول '{product.Name}' کافی نیست");
            }

            order.Items.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = item.Quantity,
                TotalPrice = product.Price * item.Quantity
            });
        }

        order.TotalAmount = order.Items.Sum(x => x.TotalPrice);
        await _orderRepository.AddAsync(order);
        await _unitOfWork.SaveChangesAsync();
        await transaction.CommitAsync();

        return BaseResponse<OrderDto>.Success(ToDto(order), "سفارش با موفقیت ثبت شد");
    }

    public async Task<BaseResponse<OrderDto>> GetByIdAsync(Guid userId, Guid orderId, bool isAdmin = false)
    {
        var order = await _orderRepository.Query(true)
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == orderId && (isAdmin || x.UserId == userId));

        return order == null
            ? BaseResponse<OrderDto>.Failure("سفارش یافت نشد")
            : BaseResponse<OrderDto>.Success(ToDto(order), "سفارش با موفقیت بازیابی شد");
    }

    public async Task<BaseResponse<IEnumerable<OrderDto>>> GetUserOrdersAsync(Guid userId)
    {
        var orders = await _orderRepository.Query(true)
            .Include(x => x.Items)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return BaseResponse<IEnumerable<OrderDto>>.Success(orders.Select(ToDto), "سفارش‌ها با موفقیت دریافت شدند");
    }

    public async Task<BaseResponse<IEnumerable<OrderDto>>> GetAllOrdersAsync()
    {
        var orders = await _orderRepository.Query(true)
            .Include(x => x.Items)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return BaseResponse<IEnumerable<OrderDto>>.Success(orders.Select(ToDto), "سفارش‌ها با موفقیت دریافت شدند");
    }

    public async Task<BaseResponse<OrderDto>> ChangeStatusAsync(Guid orderId, ChangeOrderStatusRequest request)
    {
        var order = await _orderRepository.Query().Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == orderId);
        if (order == null)
        {
            return BaseResponse<OrderDto>.Failure("سفارش یافت نشد");
        }

        order.Status = request.Status;
        _orderRepository.Update(order);
        await _unitOfWork.SaveChangesAsync();

        return BaseResponse<OrderDto>.Success(ToDto(order), "وضعیت سفارش با موفقیت تغییر یافت");
    }

    public async Task<BaseResponse<PagedResult<OrderDto>>> GetUserOrdersPagedAsync(Guid userId, PaginationRequest request, CancellationToken cancellationToken = default)
    {
        var page = await _orderRepository.Query(true).Include(x => x.Items).Where(x => x.UserId == userId).OrderByDescending(x => x.CreatedAt).ThenBy(x => x.Id)
            .ToPageAsync(request, ToDto, cancellationToken);
        return BaseResponse<PagedResult<OrderDto>>.Success(page);
    }
    public async Task<BaseResponse<PagedResult<OrderDto>>> GetAllOrdersPagedAsync(PaginationRequest request, CancellationToken cancellationToken = default)
    {
        var page = await _orderRepository.Query(true).Include(x => x.Items).OrderByDescending(x => x.CreatedAt).ThenBy(x => x.Id)
            .ToPageAsync(request, ToDto, cancellationToken);
        return BaseResponse<PagedResult<OrderDto>>.Success(page);
    }
    private static OrderDto ToDto(Order order) => new()
    {
        Id = order.Id,
        UserId = order.UserId,
        Status = order.Status.ToString(),
        TotalAmount = order.TotalAmount,
        ShippingAddress = order.ShippingAddress,
        ReceiverFullName = order.ReceiverFullName,
        ReceiverPhoneNumber = order.ReceiverPhoneNumber,
        CreatedAt = order.CreatedAt,
        Items = order.Items.Select(x => new OrderItemDto
        {
            Id = x.Id,
            ProductId = x.ProductId,
            ProductName = x.ProductName,
            UnitPrice = x.UnitPrice,
            Quantity = x.Quantity,
            TotalPrice = x.TotalPrice
        }).ToList()
    };
}
