using System.CommandLine;
using CloudflareSuperSites.Models.Configuration;
using CloudflareSuperSites.Services.Manifest;
using CloudflareSuperSites.Services.Storage;
using Microsoft.Extensions.Logging;

namespace CloudflareSuperSites.Commands;

public class CleanupCommand : Command
{
    private readonly IBaseConfiguration _baseConfiguration;
    private readonly IStorageCreatorService _storageCreatorService;
    private readonly ILogger _logger;
    private readonly IManifestService _manifestService;

    public CleanupCommand(IManifestService manifestService, IStorageCreatorService storageCreator, IBaseConfiguration baseConfiguration, ILogger<CleanupCommand> logger) : base("cleanup",
        "Clean up old files, from the manifest!")
    {
        _baseConfiguration = baseConfiguration;
        _storageCreatorService = storageCreator;
        _manifestService = manifestService;
        _logger = logger;

        this.SetHandler(ExecuteCommand);
    }


    public async Task ExecuteCommand()
    {
        var manifest = await _manifestService.LoadManifest();
        if (manifest == null)
        {
            _logger.LogCritical("You don't have manifests enabled! /* KABOOM */");
            return;
        }

        var getStorages = _storageCreatorService.GetStorages();
        

        await _manifestService.CleanUpFiles(getStorages);
        _logger.LogInformation("Done Cleaning up!");
    }
}