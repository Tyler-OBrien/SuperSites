using System.Text.Json.Serialization;

namespace CloudflareWorkerBundler.Models.Configuration.Storage;

public class EmbeddedStorageConfiguration : IStorageConfiguration
{
    [JsonPropertyName("Type")] public string InstanceType => Type;

    public static string Type => "Embedded";
    public List<string> AllowedFileExtensions { get; set; }
    public long FileSizeLimit { get; set; }

    public bool Validate()
    {
        return true;
    }
}