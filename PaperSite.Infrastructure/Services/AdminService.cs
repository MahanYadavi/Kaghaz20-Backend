using PaperSite.Application.DTOs.Common;
using Microsoft.EntityFrameworkCore;
using PaperSite.Application.Common.Responses;
using PaperSite.Application.DTOs.Admin;
using PaperSite.Application.Interfaces;
using PaperSite.Domain.Entities;
using PaperSite.Domain.Enums;

namespace PaperSite.Infrastructure.Services;

public class AdminService : IAdminService
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AdminService(IRepository<User> userRepository, IRepository<Order> orderRepository, IRepository<Product> productRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BaseResponse<IEnumerable<UserDto>>> GetAllUsersAsync()
    {
        var users = await _userRepository.Query(true).Include(x => x.Role).OrderByDescending(x => x.CreatedAt).ToListAsync();
        return BaseResponse<IEnumerable<UserDto>>.Success(users.Select(ToDto), "کاربران با موفقیت بازیابی شدند");
    }

    public async Task<BaseResponse<UserDto>> GetUserDetailsAsync(Guid userId)
    {
        var user = await _userRepository.Query(true).Include(x => x.Role).FirstOrDefaultAsync(x => x.Id == userId);
        return user == null
            ? BaseResponse<UserDto>.Failure("کاربر یافت نشد")
            : BaseResponse<UserDto>.Success(ToDto(user), "جزئیات کاربر با موفقیت بازیابی شد");
    }

    public async Task<BaseResponse<bool>> DeleteUserAsync(Guid userId, Guid currentUserId)
    {
        if (userId == currentUserId)
        {
            return BaseResponse<bool>.Failure("Admin cannot delete the current signed-in account");
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return BaseResponse<bool>.Failure("User not found");
        }

        _userRepository.Delete(user);
        await _unitOfWork.SaveChangesAsync();
        return BaseResponse<bool>.Success(true, "User deleted successfully");
    }

    public async Task<BaseResponse<DashboardStatisticsDto>> GetDashboardStatisticsAsync()
    {
        var totalUsers = await _userRepository.Query(true).CountAsync();
        var totalOrders = await _orderRepository.Query(true).CountAsync();
        var totalProducts = await _productRepository.Query(true).CountAsync();
        var totalRevenue = await _orderRepository.Query(true)
            .Where(x => x.Status != OrderStatus.Cancelled)
            .SumAsync(x => x.TotalAmount);

        return BaseResponse<DashboardStatisticsDto>.Success(new DashboardStatisticsDto
        {
            TotalUsers = totalUsers,
            TotalOrders = totalOrders,
            TotalProducts = totalProducts,
            TotalRevenue = totalRevenue
        }, "آمار داشبورد با موفقیت بازیابی شد");
    }

    public async Task<BaseResponse<PagedResult<UserDto>>> GetUsersPagedAsync(PaginationRequest request, CancellationToken cancellationToken = default)
    {
        var page = await _userRepository.Query(true).Include(x => x.Role).OrderByDescending(x => x.CreatedAt).ThenBy(x => x.Id)
            .ToPageAsync(request, ToDto, cancellationToken);
        return BaseResponse<PagedResult<UserDto>>.Success(page);
    }
    private static UserDto ToDto(User user) => new()
    {
        Id = user.Id,
        FirstName = user.FirstName,
        LastName = user.LastName,
        UserName = user.UserName,
        Email = user.Email,
        PhoneNumber = user.PhoneNumber,
        Role = user.Role.Name,
        CreatedAt = user.CreatedAt
    };
}
