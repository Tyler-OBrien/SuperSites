using Microsoft.Extensions.Logging;

namespace CloudflareWorkerBundler.Broker;

public partial class CloudflareApiBroker : ICloudflareApiBroker
{
    public const long CLOUDFLARE_API_SIZE_LIMIT = 104857600;

    public const string BasePath = "/client/v4";

    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;

    public CloudflareApiBroker(HttpClient httpClient, ILogger<CloudflareApiBroker> logger)
    {
        _logger = logger;
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://api.cloudflare.com");
    }
}