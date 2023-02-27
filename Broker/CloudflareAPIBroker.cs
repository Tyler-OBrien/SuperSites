using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudflareWorkerBundler.Broker
{
    public partial class CloudflareAPIBroker : ICloudflareAPIBroker
    {
        private readonly HttpClient _httpClient;

        public CloudflareAPIBroker(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://api.cloudflare.com/client/v4");
        }
    }
}
