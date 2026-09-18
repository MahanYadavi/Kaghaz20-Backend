using PaperSite.Application.DTOs.Common;

namespace PaperSite.Application.DTOs.Product;

public class ProductQueryRequest : PaginationRequest
{
    public string? Search { get; set; }
    public Guid? CategoryId { get; set; }
    public string? SortBy { get; set; }
    public bool Descending { get; set; }
}
