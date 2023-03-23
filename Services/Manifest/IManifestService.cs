using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudflareWorkerBundler.Models.Manifest;
using CloudflareWorkerBundler.Services.Storage;

namespace CloudflareWorkerBundler.Services.Manifest
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
