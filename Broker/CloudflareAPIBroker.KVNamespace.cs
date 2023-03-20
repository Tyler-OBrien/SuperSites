using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<ApiResponseBase> WriteKvPair(string key, byte[] value, string accountId, string nameSpaceId, string apiToken, CancellationToken token)
        {
            
            var newContent = new ByteArrayContent(value);
            var request = new HttpRequestMessage(HttpMethod.Put,
                KvGetRequestUri(accountId, nameSpaceId) + $"/values/{Uri.EscapeDataString(key)}");
            request.Headers.Add("Authorization", $"Bearer {apiToken}");
            request.Content = newContent;
            var response = await _httpClient.SendAsync(request, token);
            return await HttpExtensions.ProcessHttpResponseAsync(response, $"Put {key} key in KV Namespace {nameSpaceId}");
        }

        public async Task<ApiResponse<KvResult[], KvResultInfo>> ListKv(string cursor, string accountId, string nameSpaceId, string apiToken,
            CancellationToken token)
        {
            string cursorQueryString = String.IsNullOrWhiteSpace(cursor) ? string.Empty : $"?cursor={cursor}";
            var request = new HttpRequestMessage(HttpMethod.Get,
                KvGetRequestUri(accountId, nameSpaceId) + $"/keys{cursorQueryString}");
            request.Headers.Add("Authorization", $"Bearer {apiToken}");
            var response = await _httpClient.SendAsync(request, token);
            return await HttpExtensions.ProcessHttpResponseAsyncList<KvResult, KvResultInfo>(response, $"Get KV Keys List");
        }

        public async Task<ApiResponseBase> DeleteKv(string key, string accountId, string nameSpaceId, string apiToken, CancellationToken token)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete,
                KvGetRequestUri(accountId, nameSpaceId) + $"/values/{Uri.EscapeDataString(key)}");
            request.Headers.Add("Authorization", $"Bearer {apiToken}");
            var response = await _httpClient.SendAsync(request, token);
            return await HttpExtensions.ProcessHttpResponseAsync(response, $"Delete {key} key in KV Namespace {nameSpaceId}");
        }
    }
}
