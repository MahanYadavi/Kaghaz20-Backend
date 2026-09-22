using System;
using System.Collections.Generic;
using System.Text;

namespace PaperSite.Application.DTOs.Address
{
    public class createAddressDto
    {
        public string Title { get; set; } = "";

        public string Province { get; set; } = "";

        public string City { get; set; } = "";

        public string FullAddress { get; set; } = "";

        public string PostalCode { get; set; } = "";
    }
}
