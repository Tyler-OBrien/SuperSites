using CloudflareWorkerBundler.Broker;
using CloudflareWorkerBundler.Models.Configuration;
using CloudflareWorkerBundler.Models.Configuration.Storage;
using CloudflareWorkerBundler.Services.Minio;
using CloudflareWorkerBundler.Services.Router;
using Microsoft.Extensions.Logging;

namespace CloudflareWorkerBundler.Services.Storage.Storages;

public class R2Storage : IGenericStorage
{
    private readonly IMinioService _minioService;

    private readonly IBaseConfiguration _baseConfiguration;

    private readonly R2StorageConfiguration _configuration;

    private readonly ILogger _logger;

    public R2Storage(R2StorageConfiguration storageConfiguration, IMinioService minioService,
        IBaseConfiguration baseConfiguration, ILogger<R2Storage> logger)
    {
        _configuration = storageConfiguration;
        _minioService = minioService;
        _logger = logger;
        _baseConfiguration = baseConfiguration;
    }


    public IStorageConfiguration Configuration => _configuration;

    public string? BindingCode => $"{{ binding = \"{_configuration.BindingName}\", bucket_name = \"{_configuration.BucketName}\", preview_bucket_name = \"{_configuration.BucketName}\" }}";
    public string? BindingInternalName => "r2_buckets";

    public async Task<StorageResponse> Write(IRouter router, string fileHash, FileStream value, string fileName, bool inManifest)
    {
        var newStorageResponse = new StorageResponse();
        if (_baseConfiguration.DryRun == false && inManifest == false)
        {
            await _minioService.UploadFile(fileHash, value, _configuration.AccountId,
                _configuration.AccessKey, _configuration.SecretKey, _configuration.BucketName, CancellationToken.None);

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
            await _minioService.DeleteFile(objectName, _configuration.AccountId,
                _configuration.AccessKey, _configuration.SecretKey, _configuration.BucketName, CancellationToken.None);
        }

        _logger.LogInformation($"Deleted {objectName} from R2");
        return true;
    }

    public async Task<string?> GetFile(string objectName)
    {
        return await _minioService.GetFile(objectName, _configuration.AccountId,
            _configuration.AccessKey, _configuration.SecretKey, _configuration.BucketName, CancellationToken.None);
    }

    public async Task PlainWrite(string objectName, byte[] value)
    {
        await _minioService.UploadFile(objectName, new MemoryStream(value), _configuration.AccountId,
            _configuration.AccessKey, _configuration.SecretKey, _configuration.BucketName, CancellationToken.None);

        _logger.LogInformation($"Uploaded {objectName} to R2");
    }
}