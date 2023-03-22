using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CloudflareWorkerBundler.Models.CloudflareAPI
{

    public class WriteKvBulkItem
    {
        [JsonPropertyName("base64")]
        public bool Base64 { get; set; }
        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }
    }
}
