using System.CommandLine;
using CloudflareSuperSites.Models.Configuration;
using CloudflareSuperSites.Services.Bundler;

namespace CloudflareSuperSites.Commands;

public class BundleCommand : Command
{
    private readonly IBaseConfiguration _baseConfiguration;

    private readonly IFileBundler _fileBundler;


    public BundleCommand(IFileBundler fileBundler, IBaseConfiguration baseConfiguration) : base("bundle",
        "Bundle Local Directory")
    {
        _fileBundler = fileBundler;
        _baseConfiguration = baseConfiguration;
        var setDirectory = new Option<DirectoryInfo>("--directory")
        {
            Name = "--directory",
            Description = "Set Directory to use",
            IsRequired = false
        };


        AddOption(setDirectory);


        this.SetHandler(ExecuteCommand, setDirectory);
    }


    public async Task ExecuteCommand(DirectoryInfo directory)
    {
        var outputFileName = _baseConfiguration.OutputLocation ?? "worker.js";
        if (directory == null)
        {
            if (string.IsNullOrWhiteSpace(_baseConfiguration.BundleDirectory) == false)
                directory = new DirectoryInfo(_baseConfiguration.BundleDirectory);
            else directory = new DirectoryInfo(Environment.CurrentDirectory);
        }
        // We force all paths to include the slash for normalization, otherwise you can get different behavior depending on how you specify the directory
        if (directory.FullName.EndsWith("/", StringComparison.CurrentCultureIgnoreCase) == false)
        {
            directory = new DirectoryInfo(directory.FullName + "/");
        }


        // Gather all files
        var getFileInfo = new List<FileInfo>();

        foreach (var file in directory.GetFiles("*", SearchOption.AllDirectories))
        {
            if (directory.ToString().Equals(Environment.CurrentDirectory))
            {
                if (file.Name.Equals(outputFileName)) continue;
            }

            if (file.Name.EndsWith(".ignore") == false)
            {
                getFileInfo.Add(file);
            }
        }


        var outputFile = await _fileBundler.ProduceBundle(directory, getFileInfo);

        await File.WriteAllTextAsync(outputFileName, outputFile);
    }
}