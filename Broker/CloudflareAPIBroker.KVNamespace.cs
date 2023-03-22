using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using CloudflareWorkerBundler.Extensions;
using CloudflareWorkerBundler.Models.CloudflareAPI;

namespace CloudflareWorkerBundler.Broker
{
    public partial class CloudflareApiBroker
    {
        public string KvGetRequestUri(string accountId, string nameSpace) =>
            $"{BasePath}/accounts/{accountId}/storage/kv/namespaces/{nameSpace}";

        public async Task<ApiResponseBase> WriteKvPairs(Dictionary<string, byte[]> records, string accountId, string nameSpaceId, string apiToken, CancellationToken token)
        {
            List<WriteKvBulkItem> items = records.Select(recordKvp => new WriteKvBulkItem()
                { Base64 = true, Key = recordKvp.Key, Value = Convert.ToBase64String(recordKvp.Value) }).ToList();

            var newContent = JsonContent.Create(items);
            var request = new HttpRequestMessage(HttpMethod.Put,
                KvGetRequestUri(accountId, nameSpaceId) + $"/bulk");
            request.Headers.Add("Authorization", $"Bearer {apiToken}");
            request.Content = newContent;
            var response = await _httpClient.SendAsync(request, token);
            return await HttpExtensions.ProcessHttpResponseAsync(response, $"Put {records.Count} keys in KV Namespace {nameSpaceId}", _logger);
        }

        public async Task<ApiResponse<KvResult[], KvResultInfo>> ListKv(string cursor, string accountId, string nameSpaceId, string apiToken,
            CancellationToken token)
        {
            string cursorQueryString = String.IsNullOrWhiteSpace(cursor) ? string.Empty : $"?cursor={cursor}";
            var request = new HttpRequestMessage(HttpMethod.Get,
                KvGetRequestUri(accountId, nameSpaceId) + $"/keys{cursorQueryString}");
            request.Headers.Add("Authorization", $"Bearer {apiToken}");
            var response = await _httpClient.SendAsync(request, token);
            return await HttpExtensions.ProcessHttpResponseAsyncList<KvResult, KvResultInfo>(response, $"Get KV Keys List", _logger);
        }

        public async Task<ApiResponseBase> DeleteKv(List<string> key, string accountId, string nameSpaceId, string apiToken, CancellationToken token)
        {
            var newContent = JsonContent.Create(key);
            var request = new HttpRequestMessage(HttpMethod.Delete,
                KvGetRequestUri(accountId, nameSpaceId) + $"/bulk");
            request.Content = newContent;
            request.Headers.Add("Authorization", $"Bearer {apiToken}");
            var response = await _httpClient.SendAsync(request, token);
            return await HttpExtensions.ProcessHttpResponseAsync(response, $"Delete {key.Count} keys in KV Namespace {nameSpaceId}", _logger);
        }
    }
}
