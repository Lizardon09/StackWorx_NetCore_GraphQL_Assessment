using HealthCheckerHelper.Infrastructure.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthCheckerHelper.Infrastructure.Services.Interfaces
{
    public interface IHealthCheckerHelperService
    {
        Task<ServerHealth> CheckServerStatus(string server);
        ServerHealth GetCachedServerStatus(string server);
        void StopCheckingServer(string server);
        void StartCheckingServer(string server, int seconds);
        void StartContinuousCheckingServers(List<string> serverurls, int seconds);
        void FlushCache();
    }
}
