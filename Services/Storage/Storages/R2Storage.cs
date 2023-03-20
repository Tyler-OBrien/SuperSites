using CloudflareWorkerBundler.Broker;
using CloudflareWorkerBundler.Models.Configuration;
using CloudflareWorkerBundler.Models.Configuration.Storage;
using CloudflareWorkerBundler.Services.Router;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudflareWorkerBundler.Services.Storage.Storages
{
    internal class R2Storage : IGenericStorage
    {

        private readonly R2StorageConfiguration _configuration;

        private readonly ICloudflareApiBroker _apiBroker;

        public R2Storage(R2StorageConfiguration storageConfiguration, ICloudflareApiBroker apiBroker)
        {
            _configuration = storageConfiguration;
            _apiBroker = apiBroker;
        }


        public IStorageConfiguration Configuration => _configuration;


        public async Task<StorageResponse> Write(IRouter router, string fileHash, byte[] value, string fileName)
        {
            var newStorageResponse = new StorageResponse();
            var response = await _apiBroker.WriteR2(fileHash, value, _configuration.AccountId, _configuration.BucketName,
                _configuration.ApiToken, CancellationToken.None);
            if (response == null || response.Success == false)
            {
                throw new InvalidOperationException($"Failed to upload {response}");
            }

            newStorageResponse.GenerateResponseCode = $"(await {router.EnvironmentVariableInsideRequest}{_configuration.BindingName}.get(\"{fileHash}\")).body";
            return newStorageResponse;
        }

        public async Task<bool> Delete(string objectName)
        {
            var response = await _apiBroker.DeleteR2(objectName, _configuration.AccountId, _configuration.BucketName,
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
                var tryList = await _apiBroker.ListR2(cursor, _configuration.AccountId, _configuration.BucketName,
                    _configuration.ApiToken, CancellationToken.None);
                cursor = tryList.ResultInfo.Cursor;
                results.AddRange(tryList.Result.Select(result => result.Name));
                if (tryList.ResultInfo.Count < results.Count) break;
            }

            return results;
        }
    }
}
