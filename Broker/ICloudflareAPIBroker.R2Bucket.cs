using CloudflareWorkerBundler.Models.CloudflareAPI;

namespace CloudflareWorkerBundler.Broker;

public partial interface ICloudflareApiBroker
{
    Task<ApiResponseBase> WriteR2(string objectName, byte[] value, string accountId, string bucketName, string apiToken,
        CancellationToken token);

    Task<ApiResponse<KvResult[], KvResultInfo>> ListR2(string cursor, string accountId, string bucketName,
        string apiToken,
        CancellationToken token);

    Task<ApiResponseBase> DeleteR2(string objectName, string accountId, string bucketName, string apiToken,
        CancellationToken token);
}