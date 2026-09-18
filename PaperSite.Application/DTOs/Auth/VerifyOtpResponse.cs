using System;
using System.Collections.Generic;
using System.Text;

namespace PaperSite.Application.DTOs.Auth
{
    public class VerifyOtpResponse
    {
        public bool IsRegistered { get; set; }

        public string? AccessToken { get; set; }

        public string? RegistrationToken { get; set; }

        public Guid? UserId { get; set; }

        public string? Role { get; set; }

        public bool RequiresProfileCompletion { get; set; }
    }
}
