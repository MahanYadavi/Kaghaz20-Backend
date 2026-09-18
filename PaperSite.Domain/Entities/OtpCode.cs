using PaperSite.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaperSite.Domain.Entities
{
    public class OtpCode:BaseEntity
    {
        public string PhoneNumber { get; set; } = string.Empty;

        public string CodeHash { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }

        public DateTime? UsedAt { get; set; }

        public int FailedAttempts { get; set; }
    }
}
