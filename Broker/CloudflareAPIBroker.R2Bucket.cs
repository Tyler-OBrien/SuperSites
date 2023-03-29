using System.Net;
using CloudflareSuperSites.Extensions;
using CloudflareSuperSites.Models.CloudflareAPI;

namespace CloudflareSuperSites.Broker;

// This is all undocumented superrr scary API, ripped from Wrangler
// THIS IS THE OLD IMPLEMENTATION, NOW USING minio

public partial class CloudflareApiBroker
{
    public async Task<ApiResponseBase?> WriteR2(string objectName, byte[] value, string accountId, string bucketName,
        string apiToken,
        CancellationToken token)
    {
        var newContent = new ByteArrayContent(value);
        var request = new HttpRequestMessage(HttpMethod.Put,
            R2GetRequestUri(accountId, bucketName) + $"/objects/{Uri.EscapeDataString(objectName)}");
        request.Headers.Add("Authorization", $"Bearer {apiToken}");
        request.Content = newContent;
        var response = await _httpClient.SendAsync(request, token);
        return await response.ProcessHttpResponseAsync($"Put {objectName} in  R2 Bucket {bucketName} for account {accountId}", _logger);
    }

    // Untested as this was never used
    public async Task<ApiResponse<KvResult[], KvResultInfo>?> ListR2(string cursor, string accountId, string bucketName,
        string apiToken,
        CancellationToken token)
    {
        var cursorQueryString = string.IsNullOrWhiteSpace(cursor) ? string.Empty : $"?cursor={cursor}";
        var request = new HttpRequestMessage(HttpMethod.Get,
            R2GetRequestUri(accountId, bucketName) + $"/objects/{cursorQueryString}");
        request.Headers.Add("Authorization", $"Bearer {apiToken}");
        var response = await _httpClient.SendAsync(request, token);
        return await response.ProcessHttpResponseAsyncList<KvResult, KvResultInfo>($"Get R2 bucket List {bucketName} for account {accountId}",
            _logger);
    }


    public async Task<ApiResponseBase?> DeleteR2(string objectName, string accountId, string bucketName, string apiToken,
        CancellationToken token)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete,
            R2GetRequestUri(accountId, bucketName) + $"/objects/{Uri.EscapeDataString(objectName)}");
        request.Headers.Add("Authorization", $"Bearer {apiToken}");
        var response = await _httpClient.SendAsync(request, token);
        return await response.ProcessHttpResponseAsync($"Delete {objectName} in  R2 Bucket {bucketName} for account {accountId}", _logger);
    }


    public async Task<string?> GetR2(string objectName, string accountId, string bucketName, string apiToken,
        CancellationToken token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get,
            R2GetRequestUri(accountId, bucketName) + $"/objects/{Uri.EscapeDataString(objectName)}");
        request.Headers.Add("Authorization", $"Bearer {apiToken}");
        var response = await _httpClient.SendAsync(request, token);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadAsStringAsync(token);
        // I've gotta think about how to do this better... but for now we can just use this to parse the bad result
        await response.ProcessHttpResponseAsync($"Get {objectName} object in R2 Bucket {bucketName} for account {accountId}",
            _logger);
        return null;
    }

    public string R2GetRequestUri(string accountId, string bucketName)
    {
        return $"{BasePath}/accounts/{accountId}/r2/buckets/{bucketName}";
    }
}