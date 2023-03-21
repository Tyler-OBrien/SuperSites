using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CloudflareWorkerBundler.Models.Configuration.Storage;

namespace CloudflareWorkerBundler.Models.Configuration
{
    public interface IBaseConfiguration
    {
        public string BundleDirectory { get; set; }

        public string OutputLocation { get; set; }

        public string Router { get; set; }
        public bool Verbose { get; set; }
        public bool DryRun { get; set; }

        public List<IStorageConfiguration> StorageConfigurations { get; set; }

        public bool Validate();
    }


    public interface IStorageConfiguration
    {
        // There's probably a better way to do this, and I am failing to find it.
        [JsonInclude][JsonPropertyName("Type")] public string InstanceType { get; }
        public static string Type { get; }
        public List<string> AllowedFileExtensions { get; set; }
        public long FileSizeLimit { get; set; }
    }
}
