using CloudflareSuperSites.Models.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CloudflareSuperSites.Models.CloudflareAPI;
using CloudflareSuperSites.Models.Configuration.Storage;
using CloudflareSuperSites.Models.Manifest;

namespace CloudflareSuperSites.Models
{
    [JsonSerializable(typeof(BaseConfiguration))]
    [JsonSerializable(typeof(EmbeddedStorageConfiguration))]
    [JsonSerializable(typeof(IStorageConfiguration))]
    [JsonSerializable(typeof(KvStorageConfiguration))]
    [JsonSerializable(typeof(R2StorageConfiguration))]




    [JsonSerializable(typeof(object))]
    [JsonSerializable(typeof(BundlerManifest))]
    [JsonSerializable(typeof(ApiResponse))]
    [JsonSerializable(typeof(KvResultInfo))]
    [JsonSerializable(typeof(KvResult))]
    [JsonSerializable(typeof(WriteKvBulkItem))]
    [JsonSerializable(typeof(APIMessage))]
    [JsonSerializable(typeof(ApiResponse<object[], object>))]
    [JsonSerializable(typeof(ApiResponse<KvResult[], KvResultInfo>))]
    public partial class SerializableRequestJsonContext : JsonSerializerContext
    {
    }
}
