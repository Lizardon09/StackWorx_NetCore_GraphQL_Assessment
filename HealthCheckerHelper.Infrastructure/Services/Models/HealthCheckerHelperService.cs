using HealthCheckerHelper.Infrastructure.Models;
using HealthCheckerHelper.Infrastructure.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using SeverHelper.Infrastructure.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public HealthCheckerHelperService(IServerHelperService serverHelperService, IMemoryCache memoryCache)
        {
            _serverHelperService = serverHelperService;
            _memoryCache = memoryCache;                     //We intentiionally do not have cache options for entrye expiry times because we want cache to persist
        }

        public async Task<ServerHealth> CheckServerStatus(string server)
        {
            try
            {
                var response = await _serverHelperService.ServerStatus(server);
                var severhealth = this.ConstructNewServerHealth(response, server);
                _memoryCache.Remove(server);
                _memoryCache.Set(server, severhealth);
                Trace.WriteLine("Server health" + severhealth);
                return severhealth;
            }
            catch (Exception ex)
            {
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
        }

        public void StartCheckingServer(string server, int seconds)
        {
            RecurringTaskHelper.StartRecurringTask(() => CheckServerStatus(server), seconds, server);
        }

        public void StartContinuousCheckingServers(List<string> serverurls, int seconds)
        {
            serverurls.ForEach(server => {
                StartCheckingServer(server, seconds);
            });
        }
    }
}
