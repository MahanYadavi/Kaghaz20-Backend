namespace PaperSite.Application.DTOs.Payment;

public class MellatCallbackRequest
{
    public string? RefId { get; set; }
    public string? ResCode { get; set; }
    public long SaleOrderId { get; set; }
    public long SaleReferenceId { get; set; }
    public string? CardHolderPan { get; set; }
}
