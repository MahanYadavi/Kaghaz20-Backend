namespace PaperSite.Application.DTOs.Address;

/// <summary>
/// آدرسی که برای کاربر جاری برگردانده می‌شود.
/// </summary>
public class AddressDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string FullAddress { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}