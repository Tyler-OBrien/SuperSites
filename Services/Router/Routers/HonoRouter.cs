using CloudflareWorkerBundler.Models.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudflareWorkerBundler.Services.Router.Routers
{
    public class HonoRouter : IRouter
    {
        private readonly ILogger _logger;

        private readonly IBaseConfiguration _baseConfiguration;

        public HonoRouter(IBaseConfiguration baseConfiguration, ILogger<HonoRouter> logger)
        {
            _logger = logger;
            _baseConfiguration = baseConfiguration;
        }
        public string Name => "Hono";

        public const string HonoHeader =
            @"
import { Hono } from 'hono'
const app = new Hono()";

        public const string HonoFooter = @"export default app";

        public string EnvironmentVariableInsideRequest => "c.env.";

        public string RequestVariableInsideRequest => "c.req.";


        public void Begin(StringBuilder stringBuilder, bool fullWorker)
        {
            if (fullWorker)
            {
                stringBuilder.AppendLine(HonoHeader.Trim());
            }

        }

        public void AddRoute(StringBuilder stringBuilder, string relativePath, string fileHash, string responseCode)
        {
            stringBuilder.AppendLine(
                $"app.on(['GET', 'HEAD'],'/{relativePath}', async (c) => {{ {responseCode}  }})");

        }

        public void Add404Route(StringBuilder stringBuilder, string responseCode)
        {
            stringBuilder.AppendLine(
                $"app.on(['GET', 'HEAD'],'*', async (c) => {{ {responseCode} }})");
        }

        public void End(StringBuilder stringBuilder, bool fullWorker)
        {
            if (fullWorker)
            {
                stringBuilder.AppendLine(HonoFooter.Trim());
            }
        }
    }
}
