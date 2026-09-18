using Microsoft.EntityFrameworkCore;
using PaperSite.Application.DTOs.Common;

namespace PaperSite.Infrastructure.Services;

internal static class PaginationExtensions
{
    public static async Task<PagedResult<TDto>> ToPageAsync<TEntity, TDto>(
        this IOrderedQueryable<TEntity> query, PaginationRequest request,
        Func<TEntity, TDto> map, CancellationToken cancellationToken = default)
    {
        var (pageNumber, pageSize) = request.Normalize();
        var totalCount = await query.CountAsync(cancellationToken);
        var offset = ((long)pageNumber - 1) * pageSize;
        // Avoid overflowing SQL's integer offset for very large page numbers.
        var items = offset >= totalCount
            ? new List<TDto>()
            : (await query.Skip((int)offset).Take(pageSize).ToListAsync(cancellationToken))
                .Select(map).ToList();
        return new PagedResult<TDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}