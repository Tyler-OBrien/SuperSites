using System.Net;
using System.Net.Http.Json;
using CloudflareWorkerBundler.Extensions;
using CloudflareWorkerBundler.Models.CloudflareAPI;

namespace CloudflareWorkerBundler.Broker;

public partial class CloudflareApiBroker
{
    public async Task<ApiResponseBase?> WriteKvPairs(Dictionary<string, byte[]> records, string accountId,
        string nameSpaceId, string apiToken, CancellationToken token)
    {
        var items = records.Select(recordKvp => new WriteKvBulkItem
            { Base64 = true, Key = recordKvp.Key, Value = Convert.ToBase64String(recordKvp.Value) }).ToList();

        var newContent = JsonContent.Create(items);
        var request = new HttpRequestMessage(HttpMethod.Put,
            KvGetRequestUri(accountId, nameSpaceId) + "/bulk");
        request.Headers.Add("Authorization", $"Bearer {apiToken}");
        request.Content = newContent;
        var response = await _httpClient.SendAsync(request, token);
        return await response.ProcessHttpResponseAsync($"Put {records.Count} keys in KV Namespace {nameSpaceId}",
            _logger);
    }

    // Untested as this was never used
    public async Task<ApiResponse<KvResult[], KvResultInfo>?> ListKv(string cursor, string accountId, string nameSpaceId,
        string apiToken,
        CancellationToken token)
    {
        var cursorQueryString = string.IsNullOrWhiteSpace(cursor) ? string.Empty : $"?cursor={cursor}";
        var request = new HttpRequestMessage(HttpMethod.Get,
            KvGetRequestUri(accountId, nameSpaceId) + $"/keys{cursorQueryString}");
        request.Headers.Add("Authorization", $"Bearer {apiToken}");
        var response = await _httpClient.SendAsync(request, token);
        return await response.ProcessHttpResponseAsyncList<KvResult, KvResultInfo>("Get KV Keys List", _logger);
    }

    public async Task<ApiResponseBase?> DeleteKv(List<string> key, string accountId, string nameSpaceId, string apiToken,
        CancellationToken token)
    {
        var newContent = JsonContent.Create(key);
        var request = new HttpRequestMessage(HttpMethod.Delete,
            KvGetRequestUri(accountId, nameSpaceId) + "/bulk");
        request.Content = newContent;
        request.Headers.Add("Authorization", $"Bearer {apiToken}");
        var response = await _httpClient.SendAsync(request, token);
        return await response.ProcessHttpResponseAsync($"Delete {key.Count} keys in KV Namespace {nameSpaceId}",
            _logger);
    }

    public async Task<string?> GetKv(string key, string accountId, string nameSpaceId, string apiToken,
        CancellationToken token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get,
            KvGetRequestUri(accountId, nameSpaceId) + "/values/" + Uri.EscapeDataString(key));
        request.Headers.Add("Authorization", $"Bearer {apiToken}");
        var response = await _httpClient.SendAsync(request, token);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadAsStringAsync();
        // I've gotta think about how to do this better... but for now we can just use this to parse the bad result
        await response.ProcessHttpResponseAsync($"Get {key} keys in KV Namespace {nameSpaceId}",
            _logger);
        return null;
    }

    public string KvGetRequestUri(string accountId, string nameSpace)
    {
        return $"{BasePath}/accounts/{accountId}/storage/kv/namespaces/{nameSpace}";
    }
}