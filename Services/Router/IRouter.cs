using System.Text;

namespace CloudflareWorkerBundler.Services.Router;

public interface IRouter
{
    public string Name { get; }
    public string EnvironmentVariableInsideRequest { get; }

    public string RequestVariableInsideRequest { get; }
    public string ContextVariableInsideRequest { get; }

    /// <summary>
    /// Called at the very start, use this to prepare anything internally, like appending import headers, see hono
    /// </summary>
    /// <param name="stringBuilder"></param>
    /// <param name="fullWorker"></param>
    /// <param name="useCache"></param>
    public void Begin(StringBuilder stringBuilder, bool fullWorker, bool useCache);
    /// <summary>
    /// Add the route, either now or at the End, either is fine.
    /// The Response Code is the middleware + the actual response, as a variable "response". The Router's job is to return that.
    /// </summary>
    /// <param name="stringBuilder"></param>
    /// <param name="relativePath"></param>
    /// <param name="fileHash"></param>
    /// <param name="responseCode"></param>
    /// <param name="cacheSeconds"></param>
    /// <param name="deploymentId"></param>
    public void AddRoute(StringBuilder stringBuilder, string relativePath, string fileHash, string responseCode, int? cacheSeconds, string deploymentId);
    public void Add404Route(StringBuilder stringBuilder, string fileHash, string responseCode, int? cacheSeconds, string deploymentId);
    /// <summary>
    /// The end. If your router needs to/wants to, you could keep all of the routes in memory and append them all now.
    /// </summary>
    /// <param name="stringBuilder"></param>
    /// <param name="fullWorker"></param>
    /// <param name="useCache"></param>
    public void End(StringBuilder stringBuilder, bool fullWorker, bool useCache);
}