using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace CloudflareWorkerBundler.Models.Configuration.Storage;

[RequiresUnreferencedCode("")]
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public class KvStorageConfiguration : IStorageConfiguration
{
    [ConfigurationEnvironmentVariableProperty("CLOUDFLARE_API_TOKEN")]
    public string ApiToken { get; set; }

    [ConfigurationEnvironmentVariableProperty("ACCOUNTID")]
    public string AccountId { get; set; }

    public string NamespaceId { get; set; }

    public string BindingName { get; set; }

    public int? CacheSeconds { get; set; }


    [JsonInclude]
    [JsonPropertyName("Type")]

    public string InstanceType => Type;

    [JsonIgnore] public string StorageID => NamespaceId;
    public static string Type => "KV";
    public List<string> AllowedFileExtensions { get; set; }
    public List<string> DisallowFileExtensions { get; set; }
    public List<string> IncludePaths { get; set; }
    public List<string> ExcludePaths { get; set; }
    public long FileSizeLimit { get; set; }


    public bool Validate()
    {
        return true;
    }
}