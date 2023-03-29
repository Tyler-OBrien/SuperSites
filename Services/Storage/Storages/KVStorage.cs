using CloudflareSuperSites.Broker;
using CloudflareSuperSites.Models.Configuration;
using CloudflareSuperSites.Models.Configuration.Storage;
using CloudflareSuperSites.Services.Router;
using Microsoft.Extensions.Logging;

namespace CloudflareSuperSites.Services.Storage.Storages;

public class KvStorage : IGenericStorage
{
    public const int WORKERS_KV_BULK_MAX = 10000;

    private readonly ICloudflareApiBroker _apiBroker;

    private readonly IBaseConfiguration _baseConfiguration;
    private readonly KvStorageConfiguration _configuration;

    public readonly HashSet<string> _deleteCache;

    private readonly ILogger _logger;

    private readonly Dictionary<string, byte[]> _writeCache;

    private long _writeCacheLength;


    public KvStorage(KvStorageConfiguration kvStorageConfiguration, ICloudflareApiBroker apiBroker,
        IBaseConfiguration baseConfiguration, ILogger<KvStorage> logger)
    {
        _configuration = kvStorageConfiguration;
        _apiBroker = apiBroker;
        _logger = logger;
        _baseConfiguration = baseConfiguration;
        _deleteCache = new HashSet<string>();
        _writeCache = new Dictionary<string, byte[]>();
    }

    public string? BindingCode => $"{{ binding = \"{_configuration.BindingName}\", id = \"{_configuration.NamespaceId}\", preview_id = \"{_configuration.NamespaceId}\" }}";
    public string? BindingInternalName => "kv_namespaces";

    public IStorageConfiguration Configuration => _configuration;


    public async Task<StorageResponse> Write(IRouter router, string fileHash, FileStream value, string fileName, bool inManifest)
    {
        var newStorageResponse = new StorageResponse();
        if (_baseConfiguration.DryRun == false && inManifest == false)
        {
            if (_writeCacheLength + value.Length > CloudflareApiBroker.CLOUDFLARE_API_SIZE_LIMIT ||
                _writeCache.Count + 1 > WORKERS_KV_BULK_MAX) // lol how unlikely is that second condition
                await FlushWrites();

            // The files here shouldn't be any larger then a 25 MB at most, otherwise they wouldn't even fit into the Embedded storage...
            var memoryStream = new MemoryStream();
            await value.CopyToAsync(memoryStream);
            _writeCache.Add(fileHash, memoryStream.ToArray());
            _writeCacheLength += value.Length;
        }

        newStorageResponse.GenerateResponseCode =
            $"await {router.EnvironmentVariableInsideRequest}{_configuration.BindingName}.get(\"{fileHash}\", {{cacheTtl: 86400, type: \"stream\" }})";
        return newStorageResponse;
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

    public async Task<string?> GetFile(string key)
    {
        return await _apiBroker.GetKv(key, _configuration.AccountId, _configuration.NamespaceId,
            _configuration.ApiToken, CancellationToken.None);
    }

    public async Task PlainWrite(string objectName, byte[] value)
    {
        var writePairs = new Dictionary<string, byte[]>
        {
            { objectName, value }
        };
        var response = await _apiBroker.WriteKvPairs(writePairs, _configuration.AccountId,
            _configuration.NamespaceId,
            _configuration.ApiToken, CancellationToken.None);
        if (response == null || response.Success == false)
        {
            throw new InvalidOperationException($"Failed to upload {response}");
        }
    }

    public async Task FinalizeChanges()
    {
        _logger.LogInformation("FinalizeChanges Called for KV, flushing Writes & Deletes to KV");
        await FlushDeletes();
        await FlushWrites();
    }

    public async Task FlushWrites()
    {
        _logger.LogInformation(
            $"Flushing Writes to KV, we have reached {_writeCacheLength} length & {_writeCache.Count} items");
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
}