using Microsoft.Extensions.DependencyInjection;
using SeverHelper.Infrastructure.Services.Interfaces;
using SeverHelper.Infrastructure.Services.Models;
using System.Linq;
using System.Net.Http;

namespace SeverHelper.Infrastructure.Extentions
{
    public static class ServerHelperExtensions
    {
        public static void ConfigureServerHelper(this IServiceCollection services)
        {
            if (!services.Any(x => x.ServiceType == typeof(IHttpClientFactory)))
            {
                services.AddHttpClient();
            }

            services.AddTransient<IServerHelperService, ServerHelperService>();
        }
    }
}
