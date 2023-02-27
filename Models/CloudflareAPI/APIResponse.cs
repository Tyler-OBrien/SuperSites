using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CloudflareWorkerBundler.Models.CloudflareAPI
{

    public class APIResponse<T>
    {
        [JsonPropertyName("result")]
        public T Result { get; set; }


        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("errors")]
        public string[] Errors { get; set; }

        [JsonPropertyName("messages")]
        public string[] Messages { get; set; }
    }

    public class APIResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("errors")]
        public string[] Errors { get; set; }

        [JsonPropertyName("messages")]
        public string[] Messages { get; set; }
    }
}
