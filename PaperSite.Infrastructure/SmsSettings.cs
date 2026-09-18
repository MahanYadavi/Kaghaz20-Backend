using System;
using System.Collections.Generic;
using System.Text;

namespace PaperSite.Infrastructure
{
    public class SmsSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string TemplateId { get; set; } = string.Empty;
        public string PaymentSuccessTemplateId { get; set; } = string.Empty;

    }
}
