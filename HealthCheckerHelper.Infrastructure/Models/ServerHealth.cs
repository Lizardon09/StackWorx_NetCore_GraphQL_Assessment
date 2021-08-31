using System;
using System.Collections.Generic;
using System.Text;
using static HealthCheckerHelper.Infrastructure.Models.HealthCheckerEnums;

namespace HealthCheckerHelper.Infrastructure.Models
{
    public class ServerHealth
    {
        public string Name { get; set; }
        public ServerStatus Status { get; set; }
        public ServerError Error { get; set; }
        public DateTime? LastTimeUp { get; set; }
    }
}
