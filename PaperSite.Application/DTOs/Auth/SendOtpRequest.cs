using System;
using System.Collections.Generic;
using System.Text;

namespace PaperSite.Application.DTOs.Auth
{
    public class SendOtpRequest
    {
        private string _mobile = string.Empty;
        public string mobileNo { get => _mobile; set => _mobile = MobileNumber.Normalize(value); }
    }
}
