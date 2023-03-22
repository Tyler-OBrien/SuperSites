using CloudflareWorkerBundler.Broker;
using CloudflareWorkerBundler.Models.Configuration;
using CloudflareWorkerBundler.Services.Router.Routers;
using Microsoft.Extensions.Logging;

namespace CloudflareWorkerBundler.Services.Router;

public class RouterCreatorService : IRouterCreatorService
{
    private readonly ICloudflareApiBroker _apiBroker;
    private readonly IBaseConfiguration _baseConfiguration;

    private readonly ILoggerFactory _loggerFactory;


    public RouterCreatorService(IBaseConfiguration baseConfiguration, ICloudflareApiBroker apiBroker,
        ILoggerFactory loggerFactory)
    {
        _baseConfiguration = baseConfiguration;
        _apiBroker = apiBroker;
        _loggerFactory = loggerFactory;
    }

    public async Task<IRouter> GetRouter()
    {
        // We could do this in Reflection, but meh
        var selectedRouter = _baseConfiguration.Router;
        if (string.IsNullOrWhiteSpace(selectedRouter))
        {
            throw new InvalidOperationException(
                "You need to specify a router to use. Your valid options are Hono or Vanilla");
        }

        if (selectedRouter.Equals("Hono", StringComparison.OrdinalIgnoreCase))
        {
            return new HonoRouter(_baseConfiguration, _loggerFactory.CreateLogger<HonoRouter>());
        }

        if (selectedRouter.Equals("Vanilla"))
        {
            return new VanillaRouter(_baseConfiguration, _loggerFactory.CreateLogger<VanillaRouter>());
        }

        throw new InvalidOperationException(
            $"Couldn't find router {selectedRouter}. Your valid options were Hono or Vanilla");
    }
}