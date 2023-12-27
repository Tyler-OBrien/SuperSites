using System.Text.Json;
using CloudflareSuperSites.Extensions;
using CloudflareSuperSites.Models;
using System.Text.Json.Serialization;
using CloudflareSuperSites.Models.Configuration;
using CloudflareSuperSites.Models.Manifest;
using CloudflareSuperSites.Services.Storage;
using Microsoft.Extensions.Logging;

namespace CloudflareSuperSites.Services.Manifest
{
    public class ManifestService : IManifestService
    {
        private readonly IBaseConfiguration _baseConfiguration;

        private readonly ILogger _logger;

        private readonly IStorageCreatorService _storageCreatorService;

        public ManifestService(IBaseConfiguration baseConfiguration, IStorageCreatorService storageCreatorService, ILogger<ManifestService> logger)
        {
            _baseConfiguration = baseConfiguration;
            _logger = logger;
            _storageCreatorService = storageCreatorService;
        }

        public IGenericStorage BackingStorage { get; set; }

        public BundlerManifest Manifest { get; set; }
        public async Task<bool> LoadManifest()
        {
            if (_baseConfiguration.ManifestStorageConfiguration == null ||
                String.IsNullOrWhiteSpace(_baseConfiguration.ManifestStorageConfiguration.InstanceType))
            {
                _logger.LogInformation($"No Storage is specified for the Manifest. Aborting loading. This will slowly build up old assets in your storages, as we do not know what to delete safely.");
                return false;
            }
            var seralizationConfiguration = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                AllowTrailingCommas = true,
                TypeInfoResolver = SerializableRequestJsonContext.Default,
            };



            BackingStorage = _storageCreatorService.CreateSingle(_baseConfiguration.ManifestStorageConfiguration);
            var getManifest = await BackingStorage.GetFile("Manifest");
            if (getManifest == null)
            {
                Manifest = new BundlerManifest();
                Manifest.Deployments = new List<Deployment>();
                Manifest.LastDeployUTC = DateTime.UtcNow;
                Manifest.ManifestVersion = "1";
            }
            else
            {
                Manifest = JsonSerializer.Deserialize<BundlerManifest>(getManifest, seralizationConfiguration);
            }
            return true;
        }

        public async Task<bool> SaveManifest()
        {
            if (Manifest == null) throw new InvalidOperationException("The Manifest is not loaded yet...");
            await BackingStorage.PlainWrite("Manifest", System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(Manifest)));
            return true;
        }

        public bool IsFileUploaded(IGenericStorage storage, string fileHash)
        {
            if (Manifest == null) throw new InvalidOperationException("The Manifest is not loaded yet...");
            // If the file is already uploaded by a deployment for that same storage type and Id...
            return Manifest.Deployments.Any(deployment =>
                deployment.Files.Any(file => file.FileHash.Equals(fileHash, StringComparison.Ordinal) && file.StorageType == storage.Configuration.InstanceType && file.StorageId == storage.Configuration.StorageID));
        }

        public async Task CleanUpFiles(List<IGenericStorage> storages)
        {
            if (Manifest == null) throw new InvalidOperationException("The Manifest is not loaded yet...");
            if (Manifest.Deployments.Count < _baseConfiguration.MaxManifestCount) return;

            var findDeploymentsToCleanUp = Manifest.Deployments
                .OrderByDescending(deployment => deployment.DeploymentTimeUTC)
                .Skip(_baseConfiguration.MaxManifestCount).ToList();
            if (findDeploymentsToCleanUp.Any() == false)
            {
                _logger.LogInformation($"Found no deployments out of the {Manifest.Deployments.Count} to clean up");
                return;
            }
            _logger.LogInformation($"Cleaning up {findDeploymentsToCleanUp.Count} deployments... ");

            foreach (var deploymentToCleanup in findDeploymentsToCleanUp)
            {

                var filesToCleanup = deploymentToCleanup.Files.Where(fileToCleanup =>
                    Manifest.Deployments.Where(Deployment => Deployment != deploymentToCleanup).Any(deployment =>
                        deployment.Files.Any(file => file.FileHash.Equals(fileToCleanup.FileHash, StringComparison.Ordinal))) == false).ToList();
                _logger.LogInformation($"Cleaning up deployment: {deploymentToCleanup.ID}, which was created at {deploymentToCleanup.DeploymentTimeUTC}, {filesToCleanup.Count} files to clean up.");
                foreach (var file in filesToCleanup)
                {
                    var tryFindStorage = storages.FirstOrDefault(storage =>
                        storage.Configuration.InstanceType == file.StorageType &&
                        storage.Configuration.StorageID == file.StorageId);
                    if (tryFindStorage == null)
                    {
                        _logger.LogInformation($"We can't find the storage to delete {file.FileHash} from deployment {deploymentToCleanup.ID}, assuming storage is gone.");
                        continue;
                    }

                    await tryFindStorage.Delete(file.FileHash);
                }

                foreach (var storage in storages)
                    await storage.FinalizeChanges();

                _logger.LogInformation($"Finished Cleaning up deployment: {deploymentToCleanup.ID}, removing deployment...");
                Manifest.Deployments.Remove(deploymentToCleanup);
            }
            _logger.LogInformation($"Finished cleaning up all {findDeploymentsToCleanUp.Count} deployments...");
            await SaveManifest();
        }
    }
}
