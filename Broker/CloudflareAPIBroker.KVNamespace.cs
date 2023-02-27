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
            $"/accounts/{accountId}/storage/kv/namespaces/{nameSpace}";

        public async Task<APIResponse> WriteKVPair(string key, byte[] value, string accountId, string nameSpaceId, string apiToken, CancellationToken token)
        {
            var newContent = new ByteArrayContent(value);
            var response = await _httpClient.PutAsync(KVGetRequestUri(accountId, nameSpaceId) + $"/values/{Uri.EscapeDataString(key)}", newContent, token);
            return await HttpExtensions.ProcessHttpResponseAsync(response, $"Put {key} key in KV Namespace {nameSpaceId}");
        }
    }
}
