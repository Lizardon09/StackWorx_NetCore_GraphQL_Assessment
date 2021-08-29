using System;
using System.Collections.Generic;
using System.Text;

namespace HealthCheckerHelper.Infrastructure.Models
{
    public class ServerHealth
    {
        public string Name { get; set; }
        public string Status { get; set; }
        public ServerError Error { get; set; }
        public DateTime? LastTimeUp { get; set; }
    }
}
