using System.Security.Cryptography;
using System.Text;
using CloudflareWorkerBundler.Broker;
using CloudflareWorkerBundler.Models.Configuration;
using CloudflareWorkerBundler.Services.Router;
using CloudflareWorkerBundler.Services.Storage;
using Microsoft.AspNetCore.StaticFiles;

namespace CloudflareWorkerBundler.Services.Bundler;

public class FileBundler : IFileBundler
{

    private readonly IBaseConfiguration _baseConfiguration;

    private readonly IStorageCreatorService _storageCreatorService;

    public FileBundler(IBaseConfiguration baseConfiguration, IStorageCreatorService storageCreatorService)
    {
        _baseConfiguration = baseConfiguration;
        _storageCreatorService = storageCreatorService;
    }


    public const string FileVariablePrefix = "file";


    public const string Header =
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

    public const string Footer = @"
Preload();";

    public const string IndexHtml = "index.html";

    public async Task<string> ProduceBundle(IRouter routerToUse, DirectoryInfo directoryToBundle, List<FileInfo> filesToBundle)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"// Bundled by Cloudflare Worker Bundler on {DateTime.UtcNow.ToString()} UTC.");
        sb.AppendLine($"// {filesToBundle.Count} Files found");
        List<string> preloadCodes = new List<string>();
        string responseCode404 = null;

        var storages = await _storageCreatorService.GetStorages();
        sb.AppendLine($"// {storages.Count} Storages Configured.");



        // Write Header first
        sb.AppendLine("// Router Header Below");
        routerToUse.Begin(sb, true);
        sb.AppendLine($"{Environment.NewLine}// Router Header End");


        foreach (var storage in storages.DistinctBy(storage => storage.Configuration.InstanceType))
        {
            if (String.IsNullOrWhiteSpace(storage.Header) == false)
            {
                sb.AppendLine($"// {storage.Configuration.InstanceType} Storage Header Below");
                sb.AppendLine(storage.Header.Trim());
                sb.AppendLine($"{Environment.NewLine}// {storage.Configuration.InstanceType} Storage Header End");
            }
        }

        foreach (var fileInfo in filesToBundle)
        {
            var relativePath = GetRelativePath(fileInfo, directoryToBundle);
            var fileName = fileInfo.Name;
            var getBytes = await File.ReadAllBytesAsync(fileInfo.FullName);
            var fileHash = BitConverter.ToString(SHA256.HashData(getBytes)).Replace("-", "")
                .ToLowerInvariant();
            var trySelectStorage = storages.FirstOrDefault(storage => storage.StoreFile(fileInfo));
            if (trySelectStorage == null)
            {
                // Throw an exception
                throw new InvalidOperationException($"Failed to find storage for file {fileInfo.Name}, length: {fileInfo.Length}, extension: {fileInfo.Extension}");
            }

            var tryPutFile = await trySelectStorage.Write(routerToUse, fileHash, getBytes, fileName);

            

            if (tryPutFile.ResponseHeaders.ContainsKey("ETag") == false)
            {
                tryPutFile.ResponseHeaders.Add("ETag", $"{fileHash}");
            }

            if (String.IsNullOrWhiteSpace(tryPutFile.GlobalCode) == false)
            {
                sb.AppendLine(tryPutFile.GlobalCode);
            }

            var headers = tryPutFile.ResponseHeaders.Select(header =>
                $", '{header.Key.Replace("'", "\\'")}': \'{header.Value.Replace("'", "\\'")}\'");

            var responseCode =
                $"return new Response({tryPutFile.GenerateResponseCode}, {{ status: 200, headers: {{ 'Content-Type': '{GetContentType(fileName)}' {String.Join("", headers)} }}}});";


            routerToUse.AddRoute(sb, relativePath, fileHash, responseCode);
            Console.WriteLine($"Adding route for {relativePath}, using {trySelectStorage.Configuration.InstanceType}");

            if (relativePath.Equals("404.html", StringComparison.OrdinalIgnoreCase))
                responseCode404 = $"return new Response({tryPutFile.GenerateResponseCode}, {{ status: 404, headers: {{ 'Content-Type': '{GetContentType(fileName)}' {String.Join("", headers)} }}}});";

            if (String.IsNullOrWhiteSpace(tryPutFile.PreloadCode) == false)
                preloadCodes.Add(tryPutFile.PreloadCode);

        }

        if (responseCode404 != null)
            routerToUse.Add404Route(sb, responseCode404);

        routerToUse.End(sb, true);
        // Ok now let's warm up all of the blobs :)
        sb.AppendLine("function Preload() {");
        foreach (var preloadCode in preloadCodes)
            sb.AppendLine(preloadCode);

        sb.AppendLine("}");

        sb.AppendLine(Footer.Trim());

        Console.WriteLine($"Finished Bundled {filesToBundle.Count} of which are embedded in the Worker and will be warmed up on worker init!");
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
        if (cleanedFilePath.EndsWith(IndexHtml))
        {
            var lastIndexOf = cleanedFilePath.LastIndexOf(IndexHtml);
            if (lastIndexOf != -1)
                cleanedFilePath = cleanedFilePath.Remove(lastIndexOf, IndexHtml.Length);
        }

        return cleanedFilePath;
    }
}