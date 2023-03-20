using CloudflareWorkerBundler.Models.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudflareWorkerBundler.Broker;
using CloudflareWorkerBundler.Models.Configuration.Storage;
using CloudflareWorkerBundler.Services.Storage.Storages;

namespace CloudflareWorkerBundler.Services.Storage
{
    public class StorageCreatorService : IStorageCreatorService
    {

        private readonly IBaseConfiguration _baseConfiguration;

        private readonly ICloudflareApiBroker _apiBroker;

        public StorageCreatorService(IBaseConfiguration baseConfiguration, ICloudflareApiBroker apiBroker)
        {
            _baseConfiguration = baseConfiguration;
            _apiBroker = apiBroker;
        }

        public async Task<List<IGenericStorage>> GetStorages()
        {
           List<IGenericStorage> storages = new List<IGenericStorage>();
           foreach (var storageConfiguration in _baseConfiguration.StorageConfigurations)
           {
               if (storageConfiguration is EmbeddedStorageConfiguration embeddedStorageConfiguration)
               {
                   storages.Add(new EmbeddedStorage(embeddedStorageConfiguration));
               }
               else if (storageConfiguration is KvStorageConfiguration kvStorageConfiguration)
               {
                   storages.Add(new KvStorage(kvStorageConfiguration, _apiBroker));
               }
               else if (storageConfiguration is R2StorageConfiguration r2StorageConfiguration)
               {
                   storages.Add(new R2Storage(r2StorageConfiguration, _apiBroker));
               }
           }
           return storages;
        }
    }
}
