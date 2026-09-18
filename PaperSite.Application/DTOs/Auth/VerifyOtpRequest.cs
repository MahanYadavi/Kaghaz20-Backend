using System;
using System.Collections.Generic;
using System.Text;

namespace PaperSite.Application.DTOs.Auth
{
    public class VerifyOtpRequest
    {
        private string _mobile = string.Empty;
        public string Mobile { get => _mobile; set => _mobile = MobileNumber.Normalize(value); }
        public string Code { get; set; } = string.Empty;
    }
}
