namespace CloudflareSuperSites.Services.Bundler;

public interface IFileBundler
{
    Task<string> ProduceBundle(DirectoryInfo directoryToBundle, List<FileInfo> filesToBundle);
}