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

        private readonly Dictionary<string, byte[]> _writeCache;

        private long _writeCacheLength = 0;

        public const int WORKERS_KV_BULK_MAX = 10000;

        public readonly HashSet<string> _deleteCache;

        private readonly ICloudflareApiBroker _apiBroker;

        private readonly ILogger _logger;

        private readonly IBaseConfiguration _baseConfiguration;


        public KvStorage(KvStorageConfiguration kvStorageConfiguration, ICloudflareApiBroker apiBroker, IBaseConfiguration baseConfiguration, ILogger<KvStorage> logger)
        {
            _configuration = kvStorageConfiguration;
            _apiBroker = apiBroker;
            _logger = logger;
            _baseConfiguration = baseConfiguration;
            _deleteCache = new HashSet<string>();
            _writeCache = new Dictionary<string, byte[]>();
        }


        public IStorageConfiguration Configuration => _configuration;



        public async Task<StorageResponse> Write(IRouter router, string fileHash, byte[] value, string fileName)
        {
            var newStorageResponse = new StorageResponse();
            if (_baseConfiguration.DryRun == false)
            {
                if (_writeCacheLength + value.Length > CloudflareApiBroker.CLOUDFLARE_API_SIZE_LIMIT || _writeCache.Count + 1 > WORKERS_KV_BULK_MAX) // lol how unlikely is that second condition
                    await FlushWrites();
                _writeCache.Add(fileHash, value);
                _writeCacheLength += value.Length;
            } 

            newStorageResponse.GenerateResponseCode = $"await {router.EnvironmentVariableInsideRequest}{_configuration.BindingName}.get(\"{fileHash}\", {{cacheTtl: 86400, type: \"stream\" }})"; 
            return newStorageResponse;
        }

        public async Task FlushWrites()
        {
            _logger.LogInformation($"Flushing Writes to KV, we have reached {_writeCacheLength} length & {_writeCache.Count} items");
            if (_writeCache.Any() == false) return;
            var response = await _apiBroker.WriteKvPairs(_writeCache, _configuration.AccountId,
                _configuration.NamespaceId,
                _configuration.ApiToken, CancellationToken.None);
            if (response == null || response.Success == false)
            {
                throw new InvalidOperationException($"Failed to upload {response}");
            }
            _logger.LogInformation($"Uploaded {_writeCache.Count} items in bulk.");
            _writeCache.Clear();
            _writeCacheLength = 0;
        }

        public async Task<bool> Delete(string objectName)
        {
            if (_baseConfiguration.DryRun == false)
            {
                if (_deleteCache.Count + 1 > WORKERS_KV_BULK_MAX)
                    await FlushWrites();
                _deleteCache.Add(objectName);
            }

            return true;
        }

        public async Task FlushDeletes()
        {
            _logger.LogInformation($"Flushing Deletes to KV, we have reached {_deleteCache.Count} items");
            if (_deleteCache.Any() == false) return;
            var response = await _apiBroker.DeleteKv(_deleteCache.ToList(), _configuration.AccountId,
                _configuration.NamespaceId,
                _configuration.ApiToken, CancellationToken.None);
            if (response == null || response.Success == false)
            {
                throw new InvalidOperationException($"Failed to delete {response}");
            }
            _logger.LogInformation($"Deleted {_deleteCache.Count} items in bulk.");
            _deleteCache.Clear();
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

        

        public async Task FinalizeChanges()
        {
            _logger.LogInformation($"FinalizeChanges Called for KV, flushing Writes & Deletes to KV");
            await FlushDeletes();
            await FlushWrites();
        }
    }
}
