using CloudflareSuperSites.Services.Storage;

namespace CloudflareSuperSites.Models.Manifest
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
