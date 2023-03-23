using CloudflareWorkerBundler.Models.Configuration.Storage;

namespace CloudflareWorkerBundler.Services.Storage;

public interface IStorageCreatorService
{
    /// <summary>
    /// Returned all of the storages set up configuration. This operation does not do any actions on the actual storages, just creates all of the binding objects.
    /// </summary>
    /// <returns></returns>
    public List<IGenericStorage> GetStorages();

    /// <summary>
    /// Return a single storage from a configuration. This operation does not do any actions on the actual storages, just creates all of the binding objects.
    /// </summary>
    /// <param name="storageConfiguration"></param>
    /// <returns></returns>
    public IGenericStorage CreateSingle(IStorageConfiguration storageConfiguration);
}