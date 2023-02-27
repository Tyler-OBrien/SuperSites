using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudflareWorkerBundler.Models.CloudflareAPI;

namespace CloudflareWorkerBundler.Broker
{
    public partial interface ICloudflareAPIBroker
    {

        Task<APIResponse> WriteKVPair(string key, byte[] value, string accountId, string nameSpaceId, string apiToken, CancellationToken token);
    }
}
