using System.Text.Json;
using System.Text.Json.Serialization;
using CloudflareSuperSites.Extensions;
using CloudflareSuperSites.Models.Configuration.Storage;

namespace CloudflareSuperSites.Models.Configuration;

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
    [ConfigurationEnvironmentVariableProperty("MAX_MANIFEST_COUNT")]
    public int MaxManifestCount { get; set; }

    [ConfigurationEnvironmentVariableProperty("USE_ETAGS")]
    public bool ETags { get; set; }


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
                        FileSizeLimit = 200000,
                    },
                    new KvStorageConfiguration
                    {
                        FileSizeLimit = 10000000,
                        NamespaceId = "NamespaceId",
                        BindingName = "KV"
                    },
                    new R2StorageConfiguration()
                    {
                        BucketName = "bucket_name",
                        BindingName = "R2",
                        CacheSeconds = 3600,
                    }
                },
                ManifestStorageConfiguration = new R2StorageConfiguration()
                {
                    BucketName = "websitename-default-manifests",
                    BindingName = "I know it's silly, but this still needs one, even if it's not being used."
                },
                MaxManifestCount = 3,

            };
            await File.WriteAllTextAsync(ConfigName,
                JsonSerializer.Serialize(defaultConfig, seralizationConfiguration));
            throw new InvalidOperationException($"Config has been generated, named {ConfigName}, customize it now!");
        }

        var tryLoadConfig = await File.ReadAllTextAsync(ConfigName);
        var baseConfig = JsonSerializer.Deserialize<BaseConfiguration>(tryLoadConfig, seralizationConfiguration);
        baseConfig.ResolveConfigurationDatafromEnvironmentVariables();
        foreach (var storage in baseConfig.StorageConfigurations)
        {
            storage.ResolveConfigurationDatafromEnvironmentVariables();
            storage.Validate();
        }

        if (baseConfig.ManifestStorageConfiguration != null)
        {
            baseConfig.ManifestStorageConfiguration.ResolveConfigurationDatafromEnvironmentVariables();
            baseConfig.ManifestStorageConfiguration.Validate();
        }
        return baseConfig;
    }
}