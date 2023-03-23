using System.Text;
using CloudflareWorkerBundler.Models.Configuration;
using CloudflareWorkerBundler.Models.Middleware;
using CloudflareWorkerBundler.Services.Router;

namespace CloudflareWorkerBundler.Services.Middleware;

public class ETagMiddleware : IBaseMiddleware
{
    public string Name => "ETag";
    public ExecutionOrder Order => ExecutionOrder.Start;

    public void AddCode(IRouter router, StringBuilder stringBuilder, string fileName, string contentType,
        string fileHash, StorageResponse storageResponse, List<string> headers)
    {
        // We do .endsWith for now because of Cloudflare's default behavior with converting everything to a weak etag. Not sure the best approach for this, but I think this works ok for now...
        stringBuilder.Append(
            $"\nif ({router.RequestVariableInsideRequest}headers.get(\"If-None-Match\")?.endsWith(`\"{fileHash}\"`)) {{   ");
        stringBuilder.Append(
            $"return new Response(null, {{ status: 304, headers: {{ 'Content-Type': '{contentType}' {string.Join("", headers)} }}}});");
        stringBuilder.Append("}\r\n");
    }
}