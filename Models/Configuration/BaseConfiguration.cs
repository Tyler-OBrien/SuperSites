using System.Text.Json;
using System.Text.Json.Serialization;
using CloudflareWorkerBundler.Extensions;
using CloudflareWorkerBundler.Models.Configuration.Storage;

namespace CloudflareWorkerBundler.Models.Configuration;

public class BaseConfiguration : IBaseConfiguration
{
    public const string ConfigName = "bundleconfig.json";

    [ConfigurationEnvironmentVariableProperty("BUNDLE_DIRECTORY")]

    public string BundleDirectory { get; set; }


    [ConfigurationEnvironmentVariableProperty("OUTPUT_LOCATION")]
    public string OutputLocation { get; set; }

    [ConfigurationEnvironmentVariableProperty("ROUTER")]
    public string Router { get; set; }


    [ConfigurationEnvironmentVariableProperty("VERBOSE")]
    public bool Verbose { get; set; }

    [ConfigurationEnvironmentVariableProperty("DRY_RUN")]
    public bool DryRun { get; set; }

    public List<IStorageConfiguration> StorageConfigurations { get; set; }

    // Any more then this get pruned, and their assets removed. 
    public int MaxManifestCount { get; set; }

    public IStorageConfiguration ManifestStorageConfiguration { get; set; }

    public static async Task<BaseConfiguration> Init()
    {
        var seralizationConfiguration = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new TypeDiscriminatorConverter() },
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            AllowTrailingCommas = true
        };

        if (File.Exists(ConfigName) == false)
        {
            // Generate Default Example Config..
            var defaultConfig = new BaseConfiguration
            {
                BundleDirectory = "",
                OutputLocation = "",
                Verbose = false,
                Router = "Vanilla",
                StorageConfigurations = new List<IStorageConfiguration>
                {
                    new EmbeddedStorageConfiguration
                    {
                        AllowedFileExtensions = new List<string>
                        {
                            "html", "css", "js"
                        },
                        FileSizeLimit = 200000
                    },
                    new KvStorageConfiguration
                    {
                        FileSizeLimit = 10000000,
                        NamespaceId = "NamespaceId",
                        BindingName = "KV"
                    },
                    new R2StorageConfiguration()
                    {
                        BucketName = "Bucket_Name",
                        BindingName = "R2"
                    }
                },
                ManifestStorageConfiguration = new R2StorageConfiguration()
                {
                    BucketName = "WebsiteName-Manifests",
                    BindingName = "MANIFEST"
                },
                MaxManifestCount = 3,

            };
            await File.WriteAllTextAsync(ConfigName,
                JsonSerializer.Serialize(defaultConfig, seralizationConfiguration));
        }

        var tryLoadConfig = await File.ReadAllTextAsync(ConfigName);
        var baseConfig = JsonSerializer.Deserialize<BaseConfiguration>(tryLoadConfig, seralizationConfiguration);
        baseConfig.ResolveConfigurationDatafromEnvironmentVariables();
        return baseConfig;
    }
}