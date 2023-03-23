using CloudflareWorkerBundler.Broker;
using CloudflareWorkerBundler.Models.Configuration;
using CloudflareWorkerBundler.Models.Configuration.Storage;
using CloudflareWorkerBundler.Services.Storage.Storages;
using Microsoft.Extensions.Logging;

namespace CloudflareWorkerBundler.Services.Storage;

public class StorageCreatorService : IStorageCreatorService
{
    private readonly ICloudflareApiBroker _apiBroker;

    private readonly IBaseConfiguration _baseConfiguration;

    private readonly ILoggerFactory _loggerFactory;


    public StorageCreatorService(IBaseConfiguration baseConfiguration, ICloudflareApiBroker apiBroker,
        ILoggerFactory loggerFactory)
    {
        _baseConfiguration = baseConfiguration;
        _apiBroker = apiBroker;
        _loggerFactory = loggerFactory;
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
            return new EmbeddedStorage(embeddedStorageConfiguration, _baseConfiguration,
                _loggerFactory.CreateLogger<EmbeddedStorage>());
        }

        if (storageConfiguration is KvStorageConfiguration kvStorageConfiguration)
        {
            return new KvStorage(kvStorageConfiguration, _apiBroker, _baseConfiguration,
                _loggerFactory.CreateLogger<KvStorage>());
        }

        if (storageConfiguration is R2StorageConfiguration r2StorageConfiguration)
        {
            return new R2Storage(r2StorageConfiguration, _apiBroker, _baseConfiguration,
                _loggerFactory.CreateLogger<R2Storage>());
        }

        throw new InvalidOperationException(
            $"Tried to create storage configuration of unknown type: {storageConfiguration.InstanceType}");
    }
}