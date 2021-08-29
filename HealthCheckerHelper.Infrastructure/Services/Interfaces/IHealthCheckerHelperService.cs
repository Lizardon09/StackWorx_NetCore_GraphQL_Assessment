using HealthCheckerHelper.Infrastructure.Models;
using System.Threading.Tasks;

namespace HealthCheckerHelper.Infrastructure.Services.Interfaces
{
    public interface IHealthCheckerHelperService
    {
        Task<ServerHealth> CheckServerStatus(string server);
    }
}
