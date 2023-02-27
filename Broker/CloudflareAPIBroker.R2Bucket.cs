﻿using CloudflareWorkerBundler.Extensions;
using CloudflareWorkerBundler.Models.CloudflareAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudflareWorkerBundler.Broker
{
    // This is all undocumented superrr scary API, ripped from Wrangler
    public partial class CloudflareAPIBroker
    {
        public string R2GetRequestUri(string accountId, string bucketName) =>
            $"{BASE_PATH}/accounts/{accountId}/r2/buckets/{bucketName}";
        public async Task<APIResponse> WriteR2(string objectName, byte[] value, string accountId, string bucketName, string apiToken,
            CancellationToken token)
        {

            var newContent = new ByteArrayContent(value);
            var request = new HttpRequestMessage(HttpMethod.Put,
                R2GetRequestUri(accountId, bucketName) + $"/objects/{Uri.EscapeDataString(objectName)}");
            request.Headers.Add("Authorization", $"Bearer {apiToken}");
            request.Content = newContent;
            var response = await _httpClient.SendAsync(request, token);
            return await HttpExtensions.ProcessHttpResponseAsync(response, $"Put {objectName} in  R2 Bucket {bucketName}");
        }
    }
}
