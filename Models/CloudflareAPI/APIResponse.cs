using System.Text.Json.Serialization;

namespace CloudflareWorkerBundler.Models.CloudflareAPI;

public class ApiResponseNoPagination<T> : ApiResponse<T, object>
{
    [JsonPropertyName("result")] public T Result { get; set; }


    [JsonPropertyName("success")] public bool Success { get; set; }

    [JsonPropertyName("errors")] public string[] Errors { get; set; }

    [JsonPropertyName("messages")] public string[] Messages { get; set; }
}

public interface ApiResponseBase
{
    [JsonPropertyName("success")] public bool Success { get; set; }

    [JsonPropertyName("errors")] public string[] Errors { get; set; }

    [JsonPropertyName("messages")] public string[] Messages { get; set; }
}

public class ApiResponse<TResult, TResultInfo> : ApiResponseBase
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("result")]
    public TResult Result { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("result_info")]
    public TResultInfo ResultInfo { get; set; }


    [JsonPropertyName("success")] public bool Success { get; set; }

    [JsonPropertyName("errors")] public string[] Errors { get; set; }

    [JsonPropertyName("messages")] public string[] Messages { get; set; }
}

public class ApiResponse
{
    [JsonPropertyName("success")] public bool Success { get; set; }

    [JsonPropertyName("errors")] public string[] Errors { get; set; }

    [JsonPropertyName("messages")] public string[] Messages { get; set; }
}