using CloudflareSuperSites.Models.CloudflareAPI;

namespace CloudflareSuperSites.Broker;

// THIS IS THE OLD IMPLEMENTATION, NOW USING minio
public partial interface ICloudflareApiBroker
{
    Task<ApiResponseBase?> WriteR2(string objectName, byte[] value, string accountId, string bucketName, string apiToken,
        CancellationToken token);

    // Not tested as it was never used.
    Task<ApiResponse<KvResult[], KvResultInfo>?> ListR2(string cursor, string accountId, string bucketName,
        string apiToken,
        CancellationToken token);

    Task<ApiResponseBase?> DeleteR2(string objectName, string accountId, string bucketName, string apiToken,
        CancellationToken token);

    Task<string?> GetR2(string objectName, string accountId, string bucketName, string apiToken,
        CancellationToken token);
}