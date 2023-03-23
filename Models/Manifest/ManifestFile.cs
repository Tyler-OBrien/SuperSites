using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudflareWorkerBundler.Models.Manifest
{
    public class ManifestFile
    {
        public string FileHash { get; set; }

        public string FilePath { get; set; }

        public string StorageType { get; set; }

        public string StorageId { get; set; }
    }
}
