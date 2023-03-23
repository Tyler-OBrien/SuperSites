using System.Text.Json.Serialization;

namespace CloudflareWorkerBundler.Models.Configuration.Storage;

public class R2StorageConfiguration : IStorageConfiguration
{
    [ConfigurationEnvironmentVariableProperty("CloudflareAPIToken")]

    public string ApiToken { get; set; }

    [ConfigurationEnvironmentVariableProperty("AccountID")]

    public string AccountId { get; set; }

    [ConfigurationEnvironmentVariableProperty("BucketName")]
    public string BucketName { get; set; }

    public string BindingName { get; set; }

    [JsonIgnore] public string StorageID => $"{AccountId}+{BucketName}";


    [JsonPropertyName("Type")] public string InstanceType => Type;
    public static string Type => "R2";

    public List<string> AllowedFileExtensions { get; set; }
    public long FileSizeLimit { get; set; }
}