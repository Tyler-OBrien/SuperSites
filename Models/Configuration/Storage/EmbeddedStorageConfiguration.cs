using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace CloudflareWorkerBundler.Models.Configuration.Storage;

[RequiresUnreferencedCode("")]
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public class EmbeddedStorageConfiguration : IStorageConfiguration
{
    [JsonPropertyName("Type")] public string InstanceType => Type;
    [JsonIgnore] public string StorageID => "Embedded"; // There's only one place to put these...

    public static string Type => "Embedded";
    public List<string> AllowedFileExtensions { get; set; }
    public List<string> DisallowFileExtensions { get; set; }
    public List<string> IncludePaths { get; set; }
    public List<string> ExcludePaths { get; set; }
    public long FileSizeLimit { get; set; }

    public int? CacheSeconds
    {
        get => null;
        set
        {

        }
    }

    public void Validate()
    {
    }
}