using SeverHelper.Infrastructure.Services.Interfaces;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SeverHelper.Infrastructure.Services.Models
{
    public class ServerHelperService : IServerHelperService
    {
        private IHttpClientFactory _httpClientFactory;
        private HttpClient _serviceHttpClient;

        public ServerHelperService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _serviceHttpClient = _httpClientFactory.CreateClient();
        }

        public async Task<HttpResponseMessage> ServerStatus(string server)
        {
            try
            {
                var response = await _serviceHttpClient.GetAsync(server);
                return response;
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage()
                {
                    StatusCode = 000,
                    ReasonPhrase = ex.Message
                };
            }
        }
    }
}
