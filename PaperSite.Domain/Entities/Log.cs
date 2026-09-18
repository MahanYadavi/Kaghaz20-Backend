using PaperSite.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaperSite.Domain.Entities
{
    public class Log:BaseEntity
    {
        public string controllerName { get; set; } = string.Empty;
        public string requestJson { get; set; } = string.Empty;
        public string responseJson { get; set; } = string.Empty;
        public string persianDate { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public int statusCode { get; set; }
        public string? ipAddress {  get; set; }
        public long ExecutionTimeMs { get; set; }
        public string? userId {  get; set; }
    }
}
