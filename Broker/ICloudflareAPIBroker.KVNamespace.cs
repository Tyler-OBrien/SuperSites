﻿using CloudflareSuperSites.Models.CloudflareAPI;

namespace CloudflareSuperSites.Broker;

public partial interface ICloudflareApiBroker
{
    Task<ApiResponseBase?> WriteKvPairs(Dictionary<string, byte[]> records, string accountId, string nameSpaceId,
        string apiToken, CancellationToken token);


    Task<ApiResponse<KvResult[], KvResultInfo>?> ListKv(string cursor, string accountId, string nameSpaceId,
        string apiToken,
        CancellationToken token);


    Task<ApiResponseBase?> DeleteKv(List<string> keys, string accountId, string nameSpaceId, string apiToken,
        CancellationToken token);

    Task<string?> GetKv(string key, string accountId, string nameSpaceId, string apiToken,
        CancellationToken token);
}