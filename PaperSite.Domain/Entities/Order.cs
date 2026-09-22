using PaperSite.Domain.Common;
using PaperSite.Domain.Enums;

namespace PaperSite.Domain.Entities;

public class Order : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal TotalAmount { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string ReceiverFullName { get; set; } = string.Empty;
    public string ReceiverPhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// روش ارسال انتخاب‌شده در لحظه ثبت سفارش.
    /// </summary>
    public ShippingMethod ShippingMethod { get; set; } = ShippingMethod.Tipax;

    /// <summary>
    /// فقط برای رفرنس؛ اطلاعات آدرس به صورت Snapshot در <see cref="ShippingAddress"/> نگه‌داری می‌شود.
    /// </summary>
    public Guid? AddressId { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
