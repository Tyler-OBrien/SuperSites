using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudflareWorkerBundler.Broker;
using CloudflareWorkerBundler.Models.Configuration;
using CloudflareWorkerBundler.Models.Configuration.Storage;
using CloudflareWorkerBundler.Services.Router;

namespace CloudflareWorkerBundler.Services.Storage.Storages
{
    public class KvStorage : IGenericStorage
    {
        private readonly KvStorageConfiguration _configuration;


        private readonly ICloudflareApiBroker _apiBroker;


        public KvStorage(KvStorageConfiguration kvStorageConfiguration, ICloudflareApiBroker apiBroker)
        {
            _configuration = kvStorageConfiguration;
            _apiBroker = apiBroker;
        }


        public IStorageConfiguration Configuration => _configuration;



        public async Task<StorageResponse> Write(IRouter router, string fileHash, byte[] value, string fileName)
        {
            var newStorageResponse = new StorageResponse();
            var response = await _apiBroker.WriteKvPair(fileHash, value, _configuration.AccountId, _configuration.NamespaceId,
                _configuration.ApiToken, CancellationToken.None);
            if (response == null || response.Success == false)
            {
                throw new InvalidOperationException($"Failed to upload {response}");
            }

            newStorageResponse.GenerateResponseCode = $"await {router.EnvironmentVariableInsideRequest}{_configuration.BindingName}.get(\"{fileHash}\", {{cacheTtl: 86400, type: \"stream\" }})"; 
            return newStorageResponse;
        }

        public async Task<bool> Delete(string objectName)
        {
            var response = await _apiBroker.DeleteKv(objectName, _configuration.AccountId, _configuration.NamespaceId,
                _configuration.ApiToken, CancellationToken.None);
            if (response == null || response.Success == false)
            {
                throw new InvalidOperationException($"Failed to delete {response}");
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
