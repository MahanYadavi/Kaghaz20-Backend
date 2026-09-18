using System.Text.Json.Serialization;

namespace PaperSite.Application.DTOs.Common;

public class PagedResult<T>
{
    [JsonPropertyName("items")]
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
    [JsonPropertyName("pagenumber")]
    public int PageNumber { get; set; }
    [JsonPropertyName("pagesize")]
    public int PageSize { get; set; }
    [JsonPropertyName("totalcount")]
    public int TotalCount { get; set; }
    [JsonPropertyName("haspreviouspage")]
    public bool HasPreviousPage => PageNumber > 1;
    [JsonPropertyName("hasnextpage")]
    public bool HasNextPage => PageNumber < TotalPages;
    [JsonPropertyName("totalpages")]
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
