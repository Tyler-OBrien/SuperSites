using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CloudflareWorkerBundler.Models.CloudflareAPI
{

    public class APIResponseNoPagination<T> : APIResponse<T, object>
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

    public class APIResponseBase
    {

        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("errors")]
        public string[] Errors { get; set; }

        [JsonPropertyName("messages")]
        public string[] Messages { get; set; }
    }


    public class APIResponse<TResult, TResultInfo> : APIResponseBase
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("result")]
        public TResult Result { get; set; }


        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("errors")]
        public string[] Errors { get; set; }

        [JsonPropertyName("messages")]
        public string[] Messages { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("result_info")]
        public TResultInfo ResultInfo { get; set; }
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
