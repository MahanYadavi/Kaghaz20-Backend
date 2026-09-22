using System;
using System.Collections.Generic;
using System.Text;

namespace PaperSite.Domain.Entities
{
    public class UserAddress
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string Title { get; set; } = "خانه";

        public string Province { get; set; } = "";

        public string City { get; set; } = "";

        public string FullAddress { get; set; } = "";

        public string PostalCode { get; set; } = "";

        public bool IsDefault { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// حذف نرم؛ سفارش‌های قدیمی به اطلاعات آدرس خود وابسته نیستند (Snapshot)،
        /// اما رفرنس <see cref="Order.AddressId"/> نباید بی‌معنا شود.
        /// </summary>
        public bool IsDeleted { get; set; }

        public User User { get; set; } = null!;
    }
}
