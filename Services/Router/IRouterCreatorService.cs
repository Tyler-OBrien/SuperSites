namespace CloudflareSuperSites.Services.Router;

public interface IRouterCreatorService
{
    public Task<IRouter> GetRouter();
}