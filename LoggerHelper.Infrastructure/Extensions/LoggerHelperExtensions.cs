using LoggerHelper.Infrastructure.Models;
using LoggerHelper.Infrastructure.Models.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace LoggerHelper.Infrastructure.Extensions
{
    public static class LoggerHelperExtensions
    {
        public static void ConfigureLoggerService(this IServiceCollection services)
        {
            services.AddSingleton<ILoggerManager, LoggerManager>();
        }
    }
}
