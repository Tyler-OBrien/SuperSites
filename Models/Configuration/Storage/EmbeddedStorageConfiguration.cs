using System.Text.Json.Serialization;

namespace CloudflareWorkerBundler.Models.Configuration.Storage;

public class EmbeddedStorageConfiguration : IStorageConfiguration
{
    [JsonPropertyName("Type")] public string InstanceType => Type;
    [JsonIgnore] public string StorageID => "Embedded"; // There's only one place to put these...

    public static string Type => "Embedded";
    public List<string> AllowedFileExtensions { get; set; }
    public long FileSizeLimit { get; set; }

    public bool Validate()
    {
        return true;
    }
}