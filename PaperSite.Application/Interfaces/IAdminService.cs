using PaperSite.Application.DTOs.Common;
using PaperSite.Application.Common.Responses;
using PaperSite.Application.DTOs.Admin;

namespace PaperSite.Application.Interfaces;

public interface IAdminService
{
    Task<BaseResponse<IEnumerable<UserDto>>> GetAllUsersAsync();
    Task<BaseResponse<UserDto>> GetUserDetailsAsync(Guid userId);
    Task<BaseResponse<bool>> DeleteUserAsync(Guid userId, Guid currentUserId);
    Task<BaseResponse<DashboardStatisticsDto>> GetDashboardStatisticsAsync();
    Task<BaseResponse<PagedResult<UserDto>>> GetUsersPagedAsync(PaginationRequest request, CancellationToken cancellationToken = default);
}
