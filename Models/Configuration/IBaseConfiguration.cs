using CloudflareSuperSites.Models.Configuration.Storage;

namespace CloudflareSuperSites.Models.Configuration;

public interface IBaseConfiguration
{
    public string BundleDirectory { get; set; }

    public string OutputLocation { get; set; }

    public string Router { get; set; }
    public bool Verbose { get; set; }
    public bool DryRun { get; set; }

    public IStorageConfiguration? ManifestStorageConfiguration { get; set; }

    public List<IStorageConfiguration> StorageConfigurations { get; set; }

    // Any more then this get pruned, and their assets removed. 
    public int MaxManifestCount { get; set; }

    public bool ETags { get; set; }
}