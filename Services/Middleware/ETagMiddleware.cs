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
        stringBuilder.Append(
            $"\nif ({router.RequestVariableInsideRequest}headers.get(\"If-None-Match\") === \"{fileHash}\") {{   ");
        stringBuilder.Append(
            $"return new Response(null, {{ status: 304, headers: {{ 'Content-Type': '{contentType}' {string.Join("", headers)} }}}});");
        stringBuilder.Append("}\r\n");
    }
}