using System.Text;
using CloudflareSuperSites.Models.Configuration;
using Microsoft.Extensions.Logging;

namespace CloudflareSuperSites.Services.Router.Routers;

public class HonoRouter : IRouter
{
    public const string HonoHeader =
        @"
import { Hono } from 'hono'
const app = new Hono()";

    public const string HonoFooter = @"export default app";

    public const string HonoCacheImport = "import { cache } from 'hono/cache'";

    private readonly IBaseConfiguration _baseConfiguration;
    private readonly ILogger _logger;

    public HonoRouter(IBaseConfiguration baseConfiguration, ILogger<HonoRouter> logger)
    {
        _logger = logger;
        _baseConfiguration = baseConfiguration;
    }

    public string Name => "Hono";

    public string EnvironmentVariableInsideRequest => "c.env.";

    public string RequestVariableInsideRequest => "c.req.";

    public string ContextVariableInsideRequest => "c.executionCtx.";


    public string? Route404 { get; set; } = null;


    public void Begin(StringBuilder stringBuilder, bool fullWorker, bool useCache)
    {
        if (fullWorker)
        {
            stringBuilder.AppendLine(HonoHeader.Trim());
        }

        if (useCache)
        {
            stringBuilder.AppendLine(HonoCacheImport);
        }
    }

    public void AddRoute(StringBuilder stringBuilder, string relativePath, string fileHash, string responseCode, int? cacheSeconds, string deploymentId)
    {
        string cacheStr = "";
        if (cacheSeconds is > 0)
        {
            cacheStr = $" , cache({{ cacheName: '{deploymentId}', cacheControl: 'max-age={cacheSeconds}', }})";
        }

        stringBuilder.AppendLine(
            $"app.on(['GET', 'HEAD'],'/{relativePath}'{cacheStr}, async (c) => {{ {responseCode} return response;  }})");
    }

    public void Add404Route(StringBuilder stringBuilder, string fileHash, string responseCode, int? cacheSeconds, string deploymentId)
    {
        string cacheStr = "";
        if (cacheSeconds is > 0)
        {
            cacheStr = $" , cache({{ cacheName: '{deploymentId}', cacheControl: 'max-age={cacheSeconds}', }})";
        }
        Route404 = (
            $"app.on(['GET', 'HEAD'],'*'{cacheStr}, async (c) => {{ {responseCode} return response; }})");
    }

    public void End(StringBuilder stringBuilder, bool fullWorker, bool useCache)
    {
        if (Route404 != null)
        {
            stringBuilder.AppendLine(Route404);
        }
        if (fullWorker)
        {
            stringBuilder.AppendLine(HonoFooter.Trim());
        }
    }
}