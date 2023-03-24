using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using CloudflareWorkerBundler.Models.Configuration;
using CloudflareWorkerBundler.Models.Manifest;
using CloudflareWorkerBundler.Models.Middleware;
using CloudflareWorkerBundler.Services.Manifest;
using CloudflareWorkerBundler.Services.Middleware;
using CloudflareWorkerBundler.Services.Router;
using CloudflareWorkerBundler.Services.Storage;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;

namespace CloudflareWorkerBundler.Services.Bundler;

public class FileBundler : IFileBundler
{
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

    private readonly IBaseConfiguration _baseConfiguration;
    private readonly ILogger _logger;
    private readonly IRouterCreatorService _routerCreatorService;
    private readonly IStorageCreatorService _storageCreatorService;
    private readonly IManifestService _manifestService;

    public FileBundler(IBaseConfiguration baseConfiguration, IStorageCreatorService storageCreatorService,
        IRouterCreatorService routerCreatorService, IManifestService manifestService, ILogger<FileBundler> logger)
    {
        _baseConfiguration = baseConfiguration;
        _storageCreatorService = storageCreatorService;
        _routerCreatorService = routerCreatorService;
        _manifestService = manifestService;
        _logger = logger;
    }

    public async Task<string> ProduceBundle(DirectoryInfo directoryToBundle, List<FileInfo> filesToBundle)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"// Bundled by Cloudflare Worker Bundler on {DateTime.UtcNow.ToString()} UTC.");
        stringBuilder.AppendLine($"// {filesToBundle.Count} Files found");
        var preloadCodes = new List<string>();
        string responseCode404 = null;

        var usingManifest = await _manifestService.LoadManifest();
        stringBuilder.AppendLine($"// We are {(usingManifest ? "" : "not")} using a manifest.");
        Deployment newDeployment = null;
        if (usingManifest)
        {
            newDeployment = _manifestService.Manifest.CreateNewDeployment();
            stringBuilder.AppendLine($"// This is deployment {newDeployment.ID} at {newDeployment.DeploymentTimeUTC} UTC");
        }

        var routerToUse = await _routerCreatorService.GetRouter();
        stringBuilder.AppendLine($"// Router using: {routerToUse.Name}");

        var storages = _storageCreatorService.GetStorages();
        stringBuilder.AppendLine($"// {storages.Count} Storages Configured.");

        var middlewares = new List<IBaseMiddleware> { new ETagMiddleware() };
        stringBuilder.AppendLine($"// {middlewares.Count} Middlewares configured");


        // Write Header first
        stringBuilder.AppendLine("// Router Header Below");
        routerToUse.Begin(stringBuilder, true);
        stringBuilder.AppendLine($"{Environment.NewLine}// Router Header End");


        foreach (var storage in storages.DistinctBy(storage => storage.Configuration.InstanceType))
        {
            if (string.IsNullOrWhiteSpace(storage.Header) == false)
            {
                stringBuilder.AppendLine($"// {storage.Configuration.InstanceType} Storage Header Below");
                stringBuilder.AppendLine(storage.Header.Trim());
                stringBuilder.AppendLine(
                    $"{Environment.NewLine}// {storage.Configuration.InstanceType} Storage Header End");
            }
        }

        var routerStringBuilder = new StringBuilder();
        foreach (var fileInfo in filesToBundle)
        {
            var relativePath = GetRelativePath(fileInfo, directoryToBundle);
            var fileName = fileInfo.Name;
            var getBytes = await File.ReadAllBytesAsync(fileInfo.FullName);
            var fileHash = BitConverter.ToString(SHA256.HashData(getBytes)).Replace("-", "")
                .ToLowerInvariant();
            var contentType = GetContentType(fileName);
            var trySelectStorage = storages.FirstOrDefault(storage => storage.StoreFile(fileInfo));
            if (trySelectStorage == null)
            {
                // Throw an exception
                throw new InvalidOperationException(
                    $"Failed to find storage for file {fileInfo.Name}, length: {fileInfo.Length}, extension: {fileInfo.Extension}");
            }

            bool inManifest = usingManifest && _manifestService.IsFileUploaded(trySelectStorage, fileHash);


            var tryPutFile = await trySelectStorage.Write(routerToUse, fileHash, getBytes, fileName, inManifest);

            if (usingManifest)
            {
                newDeployment?.AddFile(fileHash, relativePath, trySelectStorage);
            }

            if (tryPutFile.ResponseHeaders.ContainsKey("ETag") == false)
            {
                tryPutFile.ResponseHeaders.Add("ETag", $"W/\"{fileHash}\"");
            }

            if (string.IsNullOrWhiteSpace(tryPutFile.GlobalCode) == false)
            {
                stringBuilder.AppendLine(tryPutFile.GlobalCode);
            }


            var headers = tryPutFile.ResponseHeaders.Select(header =>
                $", '{header.Key.Replace("'", "\\'")}': \'{header.Value.Replace("'", "\\'")}\'").ToList();

            var responseCode =
                $"return new Response({tryPutFile.GenerateResponseCode}, {{ status: 200, headers: {{ 'Content-Type': '{contentType}' {string.Join("", headers)} }}}});";

            var responseMiddlewareStringBuilder = new StringBuilder();
            // Middleware is WIP, still thinking about how exactly it should interact with responses
            foreach (var baseMiddleware in middlewares.Where(middleware => middleware.Order == ExecutionOrder.Start))
            {
                baseMiddleware.AddCode(routerToUse, responseMiddlewareStringBuilder, fileName, contentType, fileHash,
                    tryPutFile, headers);
            }

            responseMiddlewareStringBuilder.AppendLine(responseCode);

            responseCode = responseMiddlewareStringBuilder.ToString();


            routerToUse.AddRoute(routerStringBuilder, relativePath, fileHash, responseCode);
            _logger.LogInformation(
                $"Adding route for {relativePath}, using {trySelectStorage.Configuration.InstanceType}");

            if (relativePath.Equals("404.html", StringComparison.OrdinalIgnoreCase))
                responseCode404 =
                    $"return new Response({tryPutFile.GenerateResponseCode}, {{ status: 404, headers: {{ 'Content-Type': '{contentType}' {string.Join("", headers)} }}}});";

            if (string.IsNullOrWhiteSpace(tryPutFile.PreloadCode) == false)
                preloadCodes.Add(tryPutFile.PreloadCode);
        }

        foreach (var storage in storages)
            await storage.FinalizeChanges();


        if (responseCode404 != null)
            routerToUse.Add404Route(routerStringBuilder, responseCode404);

        routerToUse.End(routerStringBuilder, true);

        stringBuilder.Append(routerStringBuilder);
        // Ok now let's warm up all of the blobs :)
        stringBuilder.AppendLine("function Preload() {");
        foreach (var preloadCode in preloadCodes)
            stringBuilder.AppendLine(preloadCode);

        stringBuilder.AppendLine("}");

        stringBuilder.AppendLine(Footer.Trim());

        _logger.LogInformation($"Finished Bundling {filesToBundle.Count} files.");
        if (usingManifest)
        {
            _logger.LogInformation("Uploading Manifest...");
            await _manifestService.SaveManifest();
            _logger.LogInformation($"Uploaded new Manifest, manifest now contains {_manifestService.Manifest.Deployments.Count} Deployments");
            // Clean up time!
            await _manifestService.CleanUpFiles(storages);
        }
        var storageInfoStringBuilder = new StringBuilder();

        var findStoragesToBind = storages
            .DistinctBy(storage => storage.Configuration.InstanceType + storage.Configuration.StorageID)
            .Where(storage => storage.BindingCode != null).ToList();
        if (findStoragesToBind.Any())
        {
            var groupByType = findStoragesToBind.GroupBy(storage => storage.BindingInternalName).ToList();
            storageInfoStringBuilder.AppendLine(
                $"--- Please Create the following bindings in your wrangler.toml if you haven't already:");
            foreach (var type in groupByType)
            {
                storageInfoStringBuilder.AppendLine($"{type.Key} = [");
                storageInfoStringBuilder.AppendLine(String.Join($",{Environment.NewLine}", type.Select(storage => storage.BindingCode)));
                storageInfoStringBuilder.AppendLine($"]");
            }
            _logger.LogInformation(storageInfoStringBuilder.ToString());
        }
        

        return stringBuilder.ToString();
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