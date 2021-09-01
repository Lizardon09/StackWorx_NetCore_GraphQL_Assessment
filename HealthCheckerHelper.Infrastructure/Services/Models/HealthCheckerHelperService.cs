using HealthCheckerHelper.Infrastructure.Models;
using HealthCheckerHelper.Infrastructure.Services.Interfaces;
using LoggerHelper.Infrastructure.Models.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using SeverHelper.Infrastructure.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using TaskHelper.Infrastructure.Models;
using static HealthCheckerHelper.Infrastructure.Models.HealthCheckerEnums;

namespace HealthCheckerHelper.Infrastructure.Services.Models
{
    public class HealthCheckerHelperService : IHealthCheckerHelperService
    {
        private IServerHelperService _serverHelperService;
        private IMemoryCache _memoryCache;
        private ILoggerManager _logger;

        public HealthCheckerHelperService(IServerHelperService serverHelperService, IMemoryCache memoryCache, ILoggerManager logger)
        {
            _serverHelperService = serverHelperService;
            _memoryCache = memoryCache;                     //We intentiionally do not have cache options for entrye expiry times because we want cache to persist
            _logger = logger;                               //All sever healths proccessed, as well as their checks being stopped or start are logged
        }

        public async Task<ServerHealth> CheckServerStatus(string server)
        {
            try
            {
                var response = await _serverHelperService.ServerStatus(server);
                var severhealth = this.ConstructNewServerHealth(response, server);
                _memoryCache.Remove(server);
                _memoryCache.Set(server, severhealth);
                LogServerHealth(severhealth);
                return severhealth;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                var severhealth = new ServerHealth()
                {
                    Name = server,
                    Status = ServerStatus.UNKNOWN,
                    Error = new ServerError()
                    {
                        Status = 000,
                        Body = ex.Message
                    },
                    LastTimeUp = null
                };
                LogServerHealth(severhealth);
                return severhealth;
            }
        }

        public void FlushCache()
        {
            _memoryCache.Dispose();
        }

        public ServerHealth GetCachedServerStatus(string server)
        {
            if (_memoryCache.TryGetValue(server, out ServerHealth severhealth))
            {
                return severhealth;
            }
            return null;
        }

        private ServerHealth ConstructNewServerHealth(HttpResponseMessage response, string server, DateTime? lastTimeUp = null)
        {
            ServerHealth severhealth;
            if (response.IsSuccessStatusCode)
            {
                severhealth = new ServerHealth()
                {
                    Name = server,
                    Status = ServerStatus.ONLINE,
                    Error = null,
                    LastTimeUp = DateTime.Now
                };
            }
            else
            {
                severhealth = new ServerHealth()
                {
                    Name = server,
                    Status = ServerStatus.DOWN,
                    Error = new ServerError()
                    {
                        Status = response.StatusCode,
                        Body = response.ReasonPhrase
                    },
                    LastTimeUp = lastTimeUp

                };
            }
            return severhealth;
        }

        public void StopCheckingServer(string server)
        {
            RecurringTaskHelper.StopRecurringTask(server);
            _logger.LogInfo("Stopped checking server: " + server);
        }

        public void StartCheckingServer(string server, int seconds)
        {
            RecurringTaskHelper.StartRecurringTask(() => CheckServerStatus(server), seconds, server);
            _logger.LogInfo("Started checking server: " + server);
        }

        public void StartContinuousCheckingServers(List<string> serverurls, int seconds)
        {
            serverurls.ForEach(server => {
                StartCheckingServer(server, seconds);
            });
        }

        public void LogServerHealth(ServerHealth serverhealth)
        {
            var message = "Server checked: " +
                            serverhealth.Name + "; Status: " +
                            serverhealth.Status + "; LastTimeUp: " +
                            serverhealth.LastTimeUp;
            _logger.LogInfo(message);
        }
    }
}
