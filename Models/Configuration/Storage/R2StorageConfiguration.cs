using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using CloudflareSuperSites.Extensions;

namespace CloudflareSuperSites.Models.Configuration.Storage;

[RequiresUnreferencedCode("")]
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public class R2StorageConfiguration : IStorageConfiguration
{
    [ConfigurationEnvironmentVariableProperty("ACCESSKEY")]

    public string AccessKey { get; set; }

    [ConfigurationEnvironmentVariableProperty("SECRETKEY")]

    public string SecretKey { get; set; }

    [ConfigurationEnvironmentVariableProperty("ACCOUNTID")]

    public string AccountId { get; set; }

    public string BucketName { get; set; }

    public string BindingName { get; set; }

    public int? CacheSeconds { get; set; }


    [JsonIgnore] public string StorageID => $"{AccountId}+{BucketName}";


    [JsonPropertyName("Type")] public string InstanceType => Type;
    public static string Type => "R2";

    public List<string> AllowedFileExtensions { get; set; }
    public List<string> DisallowFileExtensions { get; set; }
    public List<string> IncludePaths { get; set; }
    public List<string> ExcludePaths { get; set; }
    public long FileSizeLimit { get; set; }

    public void Validate()
    {
        AccessKey.ThrowIfNullOrEmpty("{Name} cannot be empty in R2StorageConfiguration", nameof(AccessKey));
        SecretKey.ThrowIfNullOrEmpty("{Name} cannot be empty in R2StorageConfiguration", nameof(SecretKey));
        BucketName.ThrowIfNullOrEmpty("{Name} cannot be empty in R2StorageConfiguration", nameof(BucketName));
        AccountId.ThrowIfNullOrEmpty("{Name} cannot be empty in R2StorageConfiguration", nameof(AccountId));
        BindingName.ThrowIfNullOrEmpty("{Name} cannot be empty in R2StorageConfiguration", nameof(BindingName));
    }
}