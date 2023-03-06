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
    public partial class CloudflareAPIBroker
    {
        public string KVGetRequestUri(string accountId, string nameSpace) =>
            $"{BASE_PATH}/accounts/{accountId}/storage/kv/namespaces/{nameSpace}";

        public async Task<APIResponseBase> WriteKVPair(string key, byte[] value, string accountId, string nameSpaceId, string apiToken, CancellationToken token)
        {
            
            var newContent = new ByteArrayContent(value);
            var request = new HttpRequestMessage(HttpMethod.Put,
                KVGetRequestUri(accountId, nameSpaceId) + $"/values/{Uri.EscapeDataString(key)}");
            request.Headers.Add("Authorization", $"Bearer {apiToken}");
            request.Content = newContent;
            var response = await _httpClient.SendAsync(request, token);
            return await HttpExtensions.ProcessHttpResponseAsync(response, $"Put {key} key in KV Namespace {nameSpaceId}");
        }
    }
}
