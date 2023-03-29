using CloudflareSuperSites.Models.Configuration;
using CloudflareSuperSites.Models.Configuration.Storage;
using CloudflareSuperSites.Services.Router;
using Microsoft.Extensions.Logging;

namespace CloudflareSuperSites.Services.Storage.Storages;

public class EmbeddedStorage : IGenericStorage
{
    public const string FILE_VARIABLE_PREFIX = "file";

    private readonly IBaseConfiguration _baseConfiguration;

    private readonly EmbeddedStorageConfiguration _configuration;

    private readonly ILogger _logger;

    private readonly HashSet<string> hashesUsed = new();

    public EmbeddedStorage(EmbeddedStorageConfiguration storageConfiguration, IBaseConfiguration baseConfiguration,
        ILogger<EmbeddedStorage> logger)
    {
        _configuration = storageConfiguration;
        _logger = logger;
        _baseConfiguration = baseConfiguration;
    }

    public string Header =>
        @"
//https://stackoverflow.com/questions/27980612/converting-base64-to-blob-in-javascript
function b64toBlob(base64) {
    
    var byteString = atob(base64);
    var ab = new ArrayBuffer(byteString.length);
    var ia = new Uint8Array(ab);
    
    for (var i = 0; i < byteString.length; i++) {
        ia[i] = byteString.charCodeAt(i);
    }
    return new Blob([ab]);
}
";


    public string? BindingCode => null;
    public string? BindingInternalName => null;

    public IStorageConfiguration Configuration => _configuration;


    public async Task<StorageResponse> Write(IRouter router, string fileHash, FileStream value, string fileName, bool inManifest)
    {
        var newStorageResponse = new StorageResponse();
        if (hashesUsed.Contains(fileHash) == false)
        {
            // The files here shouldn't be any larger then a 5 MB at most, otherwise they wouldn't even fit into the Embedded storage...
            var memoryStream = new MemoryStream();
            await value.CopyToAsync(memoryStream);

            newStorageResponse.GlobalCode =
                $"let {FILE_VARIABLE_PREFIX}{fileHash} = \"{Convert.ToBase64String(memoryStream.ToArray())}\"";

            newStorageResponse.PreloadCode =
                $"    globalThis.{FILE_VARIABLE_PREFIX}{fileHash}blob = b64toBlob({FILE_VARIABLE_PREFIX}{fileHash});";
            hashesUsed.Add(fileHash);
        }

        newStorageResponse.GenerateResponseCode = $"globalThis.{FILE_VARIABLE_PREFIX}{fileHash}blob";
        return newStorageResponse;
    }

    public async Task<bool> Delete(string objectName)
    {
        return true;
    }


    public async Task<string?> GetFile(string objectName)
    {
        throw new NotImplementedException("You can't get a file from an embedded storage.");
    }

    public async Task PlainWrite(string objectName, byte[] value)
    {
        throw new NotImplementedException("You can't do plain writes to embedded storage.");
    }
}