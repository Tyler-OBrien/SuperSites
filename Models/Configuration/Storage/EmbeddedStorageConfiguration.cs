using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CloudflareWorkerBundler.Models.Configuration.Storage
{
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
}
