using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CloudflareWorkerBundler.Extensions;
using CloudflareWorkerBundler.Models.Configuration.Storage;

namespace CloudflareWorkerBundler.Models.Configuration
{
    public class BaseConfiguration : IBaseConfiguration
    {
        [ConfigurationEnvironmentVariableProperty("BUNDLE_DIRECTORY")]

        public string BundleDirectory { get; set; }


        [ConfigurationEnvironmentVariableProperty(("OUTPUT_LOCATION"))]
        public string OutputLocation { get; set; }

        [ConfigurationEnvironmentVariableProperty("Router")]
        public string Router { get; set; }



        [ConfigurationEnvironmentVariableProperty(("VERBOSE"))]
        public bool Verbose { get; set; }
        [ConfigurationEnvironmentVariableProperty(("DRY_RUN"))]
        public bool DryRun { get; set; }
        public List<IStorageConfiguration> StorageConfigurations { get; set; }
        public bool Validate()
        {
            return true;
        }

        public const string ConfigName = "bundleconfig.json";
        public static async Task<BaseConfiguration> Init()
        {
            var seralizationConfiguration = new JsonSerializerOptions()
            {
                WriteIndented = true,
                Converters = { new TypeDiscriminatorConverter() },
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                AllowTrailingCommas = true,
            };

            if (File.Exists(ConfigName) == false)
            {
                
                // Generate Default Example Config..
                var defaultConfig = new BaseConfiguration()
                {
                    BundleDirectory = "",
                    OutputLocation = "",
                    Verbose = false,
                    StorageConfigurations = new List<IStorageConfiguration>
                    {
                        new EmbeddedStorageConfiguration()
                        {
                            AllowedFileExtensions = new List<string>()
                            {
                                "html", "css", "js"
                            },
                            FileSizeLimit = 200000,
                        },
                        new KvStorageConfiguration()
                        {
                            FileSizeLimit = 10000000
                        },
                        new R2StorageConfiguration()
                        {
                        }
                    }

                };
                await File.WriteAllTextAsync(ConfigName, JsonSerializer.Serialize(defaultConfig, seralizationConfiguration));
            }

            var tryLoadConfig = await File.ReadAllTextAsync(ConfigName);
            var baseConfig = System.Text.Json.JsonSerializer.Deserialize<BaseConfiguration>(tryLoadConfig, seralizationConfiguration);
            baseConfig.ResolveConfigurationDatafromEnvironmentVariables();
            return baseConfig;
        }
    }
}
