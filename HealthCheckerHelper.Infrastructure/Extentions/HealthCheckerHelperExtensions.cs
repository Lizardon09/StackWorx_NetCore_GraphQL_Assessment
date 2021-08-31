using HealthCheckerHelper.Infrastructure.Services.Interfaces;
using HealthCheckerHelper.Infrastructure.Services.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SeverHelper.Infrastructure.Extentions;
using SeverHelper.Infrastructure.Services.Interfaces;
using System.Linq;

namespace HealthCheckerHelper.Infrastructure.Extentions
{
    public static class HealthCheckerHelperExtensions
    {
        public static void ConfigureHealthCheckerHelper(this IServiceCollection services)
        {
            if (!services.Any(x => x.ServiceType == typeof(IServerHelperService)))
            {
                services.ConfigureServerHelper();
            }
            services.AddTransient<IHealthCheckerHelperService, HealthCheckerHelperService>();
        }
    }
}
