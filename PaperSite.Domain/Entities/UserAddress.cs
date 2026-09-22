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

        public User User { get; set; } = null!;
    }
}
