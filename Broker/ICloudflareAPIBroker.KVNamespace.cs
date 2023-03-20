using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudflareWorkerBundler.Models.CloudflareAPI;

namespace CloudflareWorkerBundler.Broker
{
    public partial interface ICloudflareApiBroker
    {

        Task<ApiResponseBase> WriteKvPair(string key, byte[] value, string accountId, string nameSpaceId, string apiToken, CancellationToken token);


        Task<ApiResponse<KvResult[], KvResultInfo>> ListKv(string cursor, string accountId, string nameSpaceId,
            string apiToken,
            CancellationToken token);


        Task<ApiResponseBase> DeleteKv(string key, string accountId, string nameSpaceId, string apiToken,
            CancellationToken token);


    }
}
