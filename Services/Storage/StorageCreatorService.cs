using CloudflareWorkerBundler.Broker;
using CloudflareWorkerBundler.Models.Configuration;
using CloudflareWorkerBundler.Models.Configuration.Storage;
using CloudflareWorkerBundler.Services.Minio;
using CloudflareWorkerBundler.Services.Storage.Storages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CloudflareWorkerBundler.Services.Storage;

public class StorageCreatorService : IStorageCreatorService
{

    private readonly IBaseConfiguration _baseConfiguration;


    private readonly IServiceProvider _serviceProvider;


    public StorageCreatorService(IBaseConfiguration baseConfiguration, IServiceProvider serviceProvider)
    {
        _baseConfiguration = baseConfiguration;
        _serviceProvider = serviceProvider;
    }

    public List<IGenericStorage> GetStorages()
    {
        var storages = new List<IGenericStorage>();
        foreach (var storageConfiguration in _baseConfiguration.StorageConfigurations)
        {
            storages.Add(CreateSingle(storageConfiguration));
        }

        return storages;
    }

    public IGenericStorage CreateSingle(IStorageConfiguration storageConfiguration)
    {
        if (storageConfiguration is EmbeddedStorageConfiguration embeddedStorageConfiguration)
        {
            return ActivatorUtilities.CreateInstance<EmbeddedStorage>(_serviceProvider, embeddedStorageConfiguration);
        }
        else if (storageConfiguration is KvStorageConfiguration kvStorageConfiguration)
        {
            return ActivatorUtilities.CreateInstance<KvStorage>(_serviceProvider, kvStorageConfiguration);

        }
        else if (storageConfiguration is R2StorageConfiguration r2StorageConfiguration)
        {
            return ActivatorUtilities.CreateInstance<R2Storage>(_serviceProvider, r2StorageConfiguration);
        }

        throw new InvalidOperationException(
            $"Tried to create storage configuration of unknown type: {storageConfiguration.InstanceType}");
    }
}