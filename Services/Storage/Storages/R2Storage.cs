using CloudflareWorkerBundler.Broker;
using CloudflareWorkerBundler.Models.Configuration;
using CloudflareWorkerBundler.Models.Configuration.Storage;
using CloudflareWorkerBundler.Services.Router;
using Microsoft.Extensions.Logging;

namespace CloudflareWorkerBundler.Services.Storage.Storages;

public class R2Storage : IGenericStorage
{
    private readonly ICloudflareApiBroker _apiBroker;

    private readonly IBaseConfiguration _baseConfiguration;

    private readonly R2StorageConfiguration _configuration;

    private readonly ILogger _logger;

    public R2Storage(R2StorageConfiguration storageConfiguration, ICloudflareApiBroker apiBroker,
        IBaseConfiguration baseConfiguration, ILogger<R2Storage> logger)
    {
        _configuration = storageConfiguration;
        _apiBroker = apiBroker;
        _logger = logger;
        _baseConfiguration = baseConfiguration;
    }


    public IStorageConfiguration Configuration => _configuration;

    public string? BindingCode => $"{{ binding = \"{_configuration.BindingName}\", bucket_name = \"{_configuration.BucketName}\", preview_bucket_name = \"{_configuration.BucketName}\" }}";
    public string? BindingInternalName => "r2_buckets";

    public async Task<StorageResponse> Write(IRouter router, string fileHash, byte[] value, string fileName, bool inManifest)
    {
        var newStorageResponse = new StorageResponse();
        if (_baseConfiguration.DryRun == false && inManifest == false)
        {
            var response = await _apiBroker.WriteR2(fileHash, value, _configuration.AccountId,
                _configuration.BucketName,
                _configuration.ApiToken, CancellationToken.None);
            if (response == null || response.Success == false)
            {
                throw new InvalidOperationException($"Failed to upload {response}");
            }

            _logger.LogInformation($"Uploaded {fileName} to R2");
        }

        newStorageResponse.GenerateResponseCode =
            $"(await {router.EnvironmentVariableInsideRequest}{_configuration.BindingName}.get(\"{fileHash}\")).body";
        return newStorageResponse;
    }

    public async Task<bool> Delete(string objectName)
    {
        if (_baseConfiguration.DryRun == false)
        {
            var response = await _apiBroker.DeleteR2(objectName, _configuration.AccountId,
                _configuration.BucketName,
                _configuration.ApiToken, CancellationToken.None);
            if (response == null || response.Success == false)
            {
                throw new InvalidOperationException($"Failed to delete {response}");
            }
        }

        _logger.LogInformation($"Deleted {objectName} from R2");
        return true;
    }

    public async Task<string?> GetFile(string objectName)
    {
        return await _apiBroker.GetR2(objectName, _configuration.AccountId, _configuration.BucketName,
            _configuration.ApiToken, CancellationToken.None);
    }

    public async Task PlainWrite(string objectName, byte[] value)
    {
        var response = await _apiBroker.WriteR2(objectName, value, _configuration.AccountId,
            _configuration.BucketName,
            _configuration.ApiToken, CancellationToken.None);
        if (response == null || response.Success == false)
        {
            throw new InvalidOperationException($"Failed to upload {response}");
        }
        _logger.LogInformation($"Uploaded {objectName} to R2");
    }

    // Untested, was never used.
    public async Task<List<string>> List()
    {
        var results = new List<string>();
        var cursor = "";
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