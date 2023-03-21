using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudflareWorkerBundler.Broker;
using CloudflareWorkerBundler.Models.Configuration;
using CloudflareWorkerBundler.Models.Configuration.Storage;
using CloudflareWorkerBundler.Services.Router;
using Microsoft.Extensions.Logging;

namespace CloudflareWorkerBundler.Services.Storage.Storages
{
    public class KvStorage : IGenericStorage
    {
        private readonly KvStorageConfiguration _configuration;


        private readonly ICloudflareApiBroker _apiBroker;

        private readonly ILogger _logger;

        private readonly IBaseConfiguration _baseConfiguration;


        public KvStorage(KvStorageConfiguration kvStorageConfiguration, ICloudflareApiBroker apiBroker, IBaseConfiguration baseConfiguration, ILogger<KvStorage> logger)
        {
            _configuration = kvStorageConfiguration;
            _apiBroker = apiBroker;
            _logger = logger;
            _baseConfiguration = baseConfiguration;
        }


        public IStorageConfiguration Configuration => _configuration;



        public async Task<StorageResponse> Write(IRouter router, string fileHash, byte[] value, string fileName)
        {
            var newStorageResponse = new StorageResponse();
            if (_baseConfiguration.DryRun == false)
            {
                var response = await _apiBroker.WriteKvPair(fileHash, value, _configuration.AccountId,
                    _configuration.NamespaceId,
                    _configuration.ApiToken, CancellationToken.None);
                if (response == null || response.Success == false)
                {
                    throw new InvalidOperationException($"Failed to upload {response}");
                }
            }

            newStorageResponse.GenerateResponseCode = $"await {router.EnvironmentVariableInsideRequest}{_configuration.BindingName}.get(\"{fileHash}\", {{cacheTtl: 86400, type: \"stream\" }})"; 
            return newStorageResponse;
        }

        public async Task<bool> Delete(string objectName)
        {
            if (_baseConfiguration.DryRun == false)
            {
                var response = await _apiBroker.DeleteKv(objectName, _configuration.AccountId,
                    _configuration.NamespaceId,
                    _configuration.ApiToken, CancellationToken.None);
                if (response == null || response.Success == false)
                {
                    throw new InvalidOperationException($"Failed to delete {response}");
                }
            }

            return true;
        }

        public async Task<List<string>> List()
        {
            List<string> results = new List<string>();
            string cursor = "";
            while (true)
            {
                var tryList = await _apiBroker.ListKv(cursor, _configuration.AccountId, _configuration.NamespaceId,
                    _configuration.ApiToken, CancellationToken.None);
                cursor = tryList.ResultInfo.Cursor;
                results.AddRange(tryList.Result.Select(result => result.Name));
                if (tryList.ResultInfo.Count < results.Count) break;
            }

            return results;
        }
    }
}
