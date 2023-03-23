﻿using CloudflareWorkerBundler.Models.Configuration;
using CloudflareWorkerBundler.Models.Configuration.Storage;
using CloudflareWorkerBundler.Services.Router;

namespace CloudflareWorkerBundler.Services.Storage;

public interface IGenericStorage
{
    /// <summary>
    ///     Optional Header, placed at start of script
    /// </summary>
    public string Header => "";

    // Wrangler.toml code to create a binding. Null if none/non-supported
    public string? BindingCode { get; }

    // of wrangler.toml binding array
    public string? BindingInternalName { get; }

    // Base Configuration
    public IStorageConfiguration Configuration { get; }

    // Write File to storage and return response
    public Task<StorageResponse> Write(IRouter router, string fileHash, byte[] value, string fileName, bool inManifest);

    // Delete a file, used in cleanup
    public Task<bool> Delete(string objectName);

    /// <summary>
    ///     Used to decide when to store a file. By default, uses Configured File Size Limit and AllowedFileExtensions. Can be
    ///     overiden.
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public virtual bool StoreFile(FileInfo file)
    {
        if (Configuration.FileSizeLimit != 0 && file.Length > Configuration.FileSizeLimit) return false;
        if (Configuration.AllowedFileExtensions != null && Configuration.AllowedFileExtensions.Any(extension =>
                file.Extension.EndsWith(extension, StringComparison.OrdinalIgnoreCase)) == false) return false;
        return true;
    }

    /// <summary>
    /// Get File. Only used by Manifest, so we only care about strings/simple uses.
    /// </summary>
    /// <param name="objectName"></param>
    /// <returns>NULL if not exists. Otherwise, string of object.</returns>
    public Task<string?> GetFile(string objectName);

    /// <summary>
    /// Plain Write, used for Manifest. No batching or special stuff, just a straight write.
    /// </summary>
    /// <param name="objectName"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public Task PlainWrite(string objectName, byte[] value);

    // Return a list of all files
    public Task<List<string>> List();

    // Called at the end of the bundler, required for some Storages like KV which do things in bulk behind the scenes
    public async Task FinalizeChanges()
    {
    }
}