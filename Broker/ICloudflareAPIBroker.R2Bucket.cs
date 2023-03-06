using CloudflareWorkerBundler.Models.CloudflareAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudflareWorkerBundler.Broker
{
    public partial interface ICloudflareAPIBroker
    {
        Task<APIResponseBase> WriteR2(string objectName, byte[] value, string accountId, string bucketName, string apiToken, CancellationToken token);

    }
}
