using System;
using System.Collections.Generic;
using System.Text;

namespace PaperSite.Application.DTOs.Auth
{
    public class CompleteRegistrationRequest
    {
        public string RegistrationToken { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string? Email { get; set; }
    }
}
