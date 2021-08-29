using System.Net.Http;
using System.Threading.Tasks;

namespace SeverHelper.Infrastructure.Services.Interfaces
{
    public interface IServerHelperService
    {
        Task<HttpResponseMessage> ServerStatus(string server);
    }
}
