using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudflareWorkerBundler.Services.Storage;

namespace CloudflareWorkerBundler.Models.Manifest
{
    public class Deployment
    {
        public DateTime DeploymentTimeUTC { get; set; }

        public string ID { get; set; }

        public List<ManifestFile> Files { get; set; }

        public void AddFile(string fileHash, string filePath, IGenericStorage storage)
        {
            Files.Add(new ManifestFile()
            {
                FileHash = fileHash, FilePath = filePath, StorageId = storage.Configuration.StorageID,
                StorageType = storage.Configuration.InstanceType
            });
        }
    }
}
