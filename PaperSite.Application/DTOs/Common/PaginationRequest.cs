namespace PaperSite.Application.DTOs.Common;

public class PaginationRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public (int PageNumber, int PageSize) Normalize() =>
        (Math.Max(1, PageNumber), PageSize is < 1 or > 100 ? 10 : PageSize);
}