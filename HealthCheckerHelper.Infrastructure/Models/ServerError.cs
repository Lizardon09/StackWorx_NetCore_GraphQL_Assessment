using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace HealthCheckerHelper.Infrastructure.Models
{
    public class ServerError
    {
        public HttpStatusCode? Status { get; set; }
        public string Body { get; set; }
    }
}
