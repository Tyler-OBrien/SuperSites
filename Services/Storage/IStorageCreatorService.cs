namespace CloudflareWorkerBundler.Services.Storage;

public interface IStorageCreatorService
{
    public Task<List<IGenericStorage>> GetStorages();
}