using CloudflareWorkerBundler.Extensions;
using CloudflareWorkerBundler.Models.CloudflareAPI;

namespace CloudflareWorkerBundler.Broker;

// This is all undocumented superrr scary API, ripped from Wrangler
public partial class CloudflareApiBroker
{
    public async Task<ApiResponseBase> WriteR2(string objectName, byte[] value, string accountId, string bucketName,
        string apiToken,
        CancellationToken token)
    {
        var newContent = new ByteArrayContent(value);
        var request = new HttpRequestMessage(HttpMethod.Put,
            R2GetRequestUri(accountId, bucketName) + $"/objects/{Uri.EscapeDataString(objectName)}");
        request.Headers.Add("Authorization", $"Bearer {apiToken}");
        request.Content = newContent;
        var response = await _httpClient.SendAsync(request, token);
        return await response.ProcessHttpResponseAsync($"Put {objectName} in  R2 Bucket {bucketName}", _logger);
    }

    public async Task<ApiResponse<KvResult[], KvResultInfo>> ListR2(string cursor, string accountId, string bucketName,
        string apiToken,
        CancellationToken token)
    {
        var cursorQueryString = string.IsNullOrWhiteSpace(cursor) ? string.Empty : $"?cursor={cursor}";
        var request = new HttpRequestMessage(HttpMethod.Get,
            R2GetRequestUri(accountId, bucketName) + $"/objects/{cursorQueryString}");
        request.Headers.Add("Authorization", $"Bearer {apiToken}");
        var response = await _httpClient.SendAsync(request, token);
        return await response.ProcessHttpResponseAsyncList<KvResult, KvResultInfo>($"Get R2 bucket List {bucketName}",
            _logger);
    }


    public async Task<ApiResponseBase> DeleteR2(string objectName, string accountId, string bucketName, string apiToken,
        CancellationToken token)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete,
            R2GetRequestUri(accountId, bucketName) + $"/objects/{Uri.EscapeDataString(objectName)}");
        request.Headers.Add("Authorization", $"Bearer {apiToken}");
        var response = await _httpClient.SendAsync(request, token);
        return await response.ProcessHttpResponseAsync($"Delete {objectName} in  R2 Bucket {bucketName}", _logger);
    }

    public string R2GetRequestUri(string accountId, string bucketName)
    {
        return $"{BasePath}/accounts/{accountId}/r2/buckets/{bucketName}";
    }
}