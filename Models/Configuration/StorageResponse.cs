using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudflareWorkerBundler.Models.Configuration
{
    public class StorageResponse
    {
        public string GenerateResponseCode { get; set; }

        public string GlobalCode { get; set; }

        public string PreloadCode { get; set; }

        public Dictionary<string, string> ResponseHeaders { get; set; } = new Dictionary<string, string>();

    }
}
