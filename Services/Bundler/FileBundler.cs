using System.Security.Cryptography;
using System.Text;
using CloudflareWorkerBundler.Broker;
using CloudflareWorkerBundler.Services.Router;
using Microsoft.AspNetCore.StaticFiles;

namespace CloudflareWorkerBundler.Services.Bundler;

public class FileBundler : IFileBundler
{
    private readonly ICloudflareAPIBroker _apiBroker;

    public FileBundler(ICloudflareAPIBroker apiBroker)
    {
        _apiBroker = apiBroker;
    }


    public const string FILE_VARIABLE_PREFIX = "file";


    public const string HEADER =
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

    public const string FOOTER = @"
WarmUpBlobs();";

    public const string INDEX_HTML = "index.html";

    public async Task<string> ProduceBundle(IRouter routerToUse, DirectoryInfo directoryToBundle, List<FileInfo> filesToBundle, string[] fileExtensionsToBundle, long bundleFileSizeLimit, string KVNamespace, long KVFileSizeLimit, string R2BucketName, string APIToken, string accountId)
    {
        var sb = new StringBuilder();
        var hashsUsed = new HashSet<string>();

        bool hasKVOrR2 = String.IsNullOrWhiteSpace(KVNamespace) == false || String.IsNullOrWhiteSpace(R2BucketName);

        if (String.IsNullOrWhiteSpace(APIToken) && hasKVOrR2)
        {
            Console.Write("You have R2 or KV Specified, but no API Token. Will treat this as you want to bundle everything into the worker.");
            hasKVOrR2 = false;
        }


        // Write Header first
        routerToUse.Begin(sb, true);
        sb.AppendLine(HEADER.Trim());

        foreach (var file in filesToBundle)
        {

            if (hasKVOrR2 == false || (fileExtensionsToBundle.Any(fileExtension =>
                    file.Extension.EndsWith(fileExtension, StringComparison.OrdinalIgnoreCase)) && file.Length < bundleFileSizeLimit))
            {
                var relativePath = GetRelativePath(file, directoryToBundle);
                var getBytes = await File.ReadAllBytesAsync(file.FullName);
                var fileHash = BitConverter.ToString(SHA256.HashData(getBytes)).Replace("-", "")
                    .ToLowerInvariant();
                if (hashsUsed.Contains(fileHash) == false)
                {
                    hashsUsed.Add(fileHash);
                    sb.AppendLine($"let {FILE_VARIABLE_PREFIX}{fileHash} = \"{Convert.ToBase64String(getBytes)}\"");
                }


                var responseCode =
                    $"return new Response(globalThis.{FILE_VARIABLE_PREFIX}{fileHash}blob, {{ status: 200, headers: {{ 'Content-Type': '{GetContentType(file.Name)}' }}}});";

                routerToUse.AddRoute(sb, relativePath, fileHash, responseCode);
                Console.WriteLine($"Adding route for {relativePath}, embedding in worker..");
            }
            else
            {
                var relativePath = GetRelativePath(file, directoryToBundle);
                var getBytes = await File.ReadAllBytesAsync(file.FullName);
                var fileHash = BitConverter.ToString(SHA256.HashData(getBytes)).Replace("-", "")
                    .ToLowerInvariant();
                bool r2 = String.IsNullOrWhiteSpace(R2BucketName) == false;
                // Upload first..

                string getResponseBodyCode = "";

                if (r2 && file.Length > KVFileSizeLimit)
                {
                    if (await _apiBroker.WriteR2(relativePath, getBytes, accountId, R2BucketName, APIToken,
                            CancellationToken.None) == null)
                    {
                        // Error, already printed, just abort..
                        throw new InvalidOperationException("Failed to upload r2 asset..");
                    }
                    getResponseBodyCode = $"(await {routerToUse.EnvironmentVariableInsideRequest}R2.get(\"{relativePath}\")).body";
                    Console.WriteLine($"Adding route for {relativePath}, uploaded to R2..");
                }
                else // KV!!
                {
                    if (await _apiBroker.WriteKVPair(relativePath, getBytes, accountId, KVNamespace, APIToken,
                            CancellationToken.None) == null)
                    {
                        // Error, already printed, just abort..
                        throw new InvalidOperationException("Failed to upload KV asset..");
                    }
                    getResponseBodyCode = $"await {routerToUse.EnvironmentVariableInsideRequest}KV.get(\"{relativePath}\", {{cacheTtl: 86400, type: \"stream\" }})";
                    Console.WriteLine($"Adding route for {relativePath}, uploaded to KV..");
                }

                var responseCode =
                    $"return new Response({getResponseBodyCode}, {{ status: 200, headers: {{ 'Content-Type': '{GetContentType(file.Name)}' }}}});";

                routerToUse.AddRoute(sb, relativePath, fileHash, responseCode);

            }
        }

        routerToUse.End(sb, true);
        // If this isn't an asset we should bundle, we will need to fetch it from KV later. For now, we'll just ignore.
        // Ok now let's warm up all of the blobs :)
        sb.AppendLine("function WarmUpBlobs() {");
        foreach (var hashToWarm in hashsUsed)
            sb.AppendLine(
                $"    globalThis.{FILE_VARIABLE_PREFIX}{hashToWarm}blob = b64toBlob({FILE_VARIABLE_PREFIX}{hashToWarm});");

        sb.AppendLine("}");

        sb.AppendLine(FOOTER.Trim());

        Console.WriteLine($"Finished Bundled {filesToBundle.Count}, {hashsUsed.Count} of which are embedded in the Worker and will be warmed up on worker init!");
        return sb.ToString();
    }

    public string GetContentType(string fileName)
    {
        var provider = new FileExtensionContentTypeProvider();
        string contentType;
        if (!provider.TryGetContentType(fileName, out contentType)) contentType = "application/octet-stream";
        return contentType;
    }

    public string GetRelativePath(FileInfo file, DirectoryInfo baseDirectory)
    {
        var cleanedFilePath = file.FullName.Replace(baseDirectory.FullName, "").TrimStart('\\').Replace("\\", "/");
        if (cleanedFilePath.EndsWith(INDEX_HTML))
        {
            var lastIndexOf = cleanedFilePath.LastIndexOf(INDEX_HTML);
            if (lastIndexOf != -1)
                cleanedFilePath = cleanedFilePath.Remove(lastIndexOf, INDEX_HTML.Length);
        }

        return cleanedFilePath;
    }
}