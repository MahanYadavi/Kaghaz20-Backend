using PaperSite.Domain.Common;
using PaperSite.Domain.Enums;

namespace PaperSite.Domain.Entities;

public class Payment : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public long GatewayOrderId { get; set; }
    public decimal AmountToman { get; set; }
    public long AmountRial { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? RefId { get; set; }
    public long? SaleReferenceId { get; set; }
    public string? ResCode { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? VerifiedAt { get; set; }
}
