using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudflareWorkerBundler.Broker
{
    public partial class CloudflareApiBroker : ICloudflareApiBroker
    {
        private readonly HttpClient _httpClient;

        public const string BasePath = "/client/v4";

        public CloudflareApiBroker(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://api.cloudflare.com");
        }
    }
}
