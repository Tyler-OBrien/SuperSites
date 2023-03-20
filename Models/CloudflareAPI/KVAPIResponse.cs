using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CloudflareWorkerBundler.Models.CloudflareAPI
{
    public partial class KvapiResponse : ApiResponse<KvResult, KvResultInfo>
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("result")]
        public KvResult[] Result { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("errors")]
        public object[] Errors { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("messages")]
        public object[] Messages { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("result_info")]
        public KvResultInfo ResultInfo { get; set; }
    }

    public partial class KvResult
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    public partial class KvResultInfo
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("count")]
        public long? Count { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("cursor")]
        public string Cursor { get; set; }
    }
}
