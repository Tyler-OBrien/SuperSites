using CloudflareWorkerBundler.Models.CloudflareAPI;

namespace CloudflareWorkerBundler.Broker;

public partial interface ICloudflareApiBroker
{
    Task<ApiResponseBase> WriteKvPairs(Dictionary<string, byte[]> records, string accountId, string nameSpaceId,
        string apiToken, CancellationToken token);


    Task<ApiResponse<KvResult[], KvResultInfo>> ListKv(string cursor, string accountId, string nameSpaceId,
        string apiToken,
        CancellationToken token);


    Task<ApiResponseBase> DeleteKv(List<string> keys, string accountId, string nameSpaceId, string apiToken,
        CancellationToken token);
}