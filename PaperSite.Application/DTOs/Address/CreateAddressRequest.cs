namespace PaperSite.Application.DTOs.Address;

/// <summary>
/// ورودی ساخت آدرس جدید. مالک آدرس فقط از توکن خوانده می‌شود.
/// </summary>
public class CreateAddressRequest
{
    public string Title { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string FullAddress { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
}