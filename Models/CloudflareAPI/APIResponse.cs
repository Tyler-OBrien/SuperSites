using System.Text.Json.Serialization;

namespace CloudflareWorkerBundler.Models.CloudflareAPI;


public interface ApiResponseBase
{
    [JsonPropertyName("success")] public bool Success { get; set; }

    [JsonPropertyName("errors")] public APIMessage[] Errors { get; set; }

    [JsonPropertyName("messages")] public APIMessage[] Messages { get; set; }
}

public class ApiResponse<TResult, TResultInfo> : ApiResponseBase
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("result")]
    public TResult? Result { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("result_info")]
    public TResultInfo? ResultInfo { get; set; }


    [JsonPropertyName("success")] public bool Success { get; set; }

    [JsonPropertyName("errors")] public APIMessage[] Errors { get; set; } 

    [JsonPropertyName("messages")] public APIMessage[] Messages { get; set; }
}

public partial class APIMessage
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("code")]
    public long? Code { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("message")]
    public string Message { get; set; }
}


public class ApiResponse
{
    [JsonPropertyName("success")] public bool Success { get; set; }

    [JsonPropertyName("errors")] public APIMessage[] Errors { get; set; }

    [JsonPropertyName("messages")] public APIMessage[] Messages { get; set; }
}