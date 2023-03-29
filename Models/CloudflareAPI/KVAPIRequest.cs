using System.Text.Json.Serialization;

namespace CloudflareSuperSites.Models.CloudflareAPI;

public class WriteKvBulkItem
{
    [JsonPropertyName("base64")] public bool Base64 { get; set; }

    [JsonPropertyName("key")] public string Key { get; set; }

    [JsonPropertyName("value")] public string Value { get; set; }
}