using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CloudflareWorkerBundler.Models.Configuration.Storage
{
    public class KvStorageConfiguration : IStorageConfiguration
    {
        [JsonInclude] [JsonPropertyName("Type")] public string InstanceType => Type;
        public static string Type => "KV";
        public List<string> AllowedFileExtensions { get; set; }
        public long FileSizeLimit { get; set; }

        [ConfigurationEnvironmentVariableProperty("CloudflareAPIToken")]
        public string ApiToken { get; set; }

        [ConfigurationEnvironmentVariableProperty("AccountID")]
        public string AccountId { get; set; }


        [ConfigurationEnvironmentVariableProperty("NamespaceId")]
        public string NamespaceId { get; set; }

        public string BindingName { get; set; }


        public bool UseCacheApi { get; set; }


        public bool Validate()
        {
            return true;
        }
    }
}
