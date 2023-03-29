using CloudflareSuperSites.Models.Manifest;
using CloudflareSuperSites.Services.Storage;

namespace CloudflareSuperSites.Services.Manifest
{
    public interface IManifestService
    {
        public IGenericStorage BackingStorage { get; set; }
        public BundlerManifest Manifest { get; set; }
        public Task<bool> LoadManifest();

        public Task<bool> SaveManifest();

        public bool IsFileUploaded(IGenericStorage storage, string fileHash);

        public Task CleanUpFiles(List<IGenericStorage> storages);

    }
}
