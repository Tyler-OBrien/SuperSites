using CloudflareWorkerBundler.Broker;
using CloudflareWorkerBundler.Models.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudflareWorkerBundler.Services.Router.Routers;

namespace CloudflareWorkerBundler.Services.Router
{
    public class RouterCreatorService : IRouterCreatorService
    {
        private readonly IBaseConfiguration _baseConfiguration;

        private readonly ICloudflareApiBroker _apiBroker;

        private readonly ILoggerFactory _loggerFactory;



        public RouterCreatorService(IBaseConfiguration baseConfiguration, ICloudflareApiBroker apiBroker, ILoggerFactory loggerFactory)
        {
            _baseConfiguration = baseConfiguration;
            _apiBroker = apiBroker;
            _loggerFactory = loggerFactory;

        }

        public async Task<IRouter> GetRouter()
        {
            // We could do this in Reflection, but meh
            var selectedRouter = _baseConfiguration.Router;
            if (String.IsNullOrWhiteSpace(selectedRouter))
            {
                throw new InvalidOperationException(
                    $"You need to specify a router to use. Your valid options are Hono or Vanilla");
            }
            if (selectedRouter.Equals("Hono", StringComparison.OrdinalIgnoreCase))
            {
                return new HonoRouter(_baseConfiguration, _loggerFactory.CreateLogger<HonoRouter>());

            }
            else if (selectedRouter.Equals("Vanilla"))
            {
                return new VanillaRouter(_baseConfiguration, _loggerFactory.CreateLogger<VanillaRouter>());
            }

            throw new InvalidOperationException(
                $"Couldn't find router {selectedRouter}. Your valid options were Hono or Vanilla");
        }
    }
}
