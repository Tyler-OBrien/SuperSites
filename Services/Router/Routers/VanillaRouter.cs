using System.Text;
using CloudflareWorkerBundler.Models.Configuration;
using Microsoft.Extensions.Logging;

namespace CloudflareWorkerBundler.Services.Router.Routers;

public class VanillaRouter : IRouter
{
    public const string VanillaHeader =
        @"export default {
  async fetch(request, env) {
   const requestedUrl = new URL(request.url);
let path = requestedUrl.pathname.split(""/"").slice(1);

";

    public const string VanillaFooter = @" }
}";

    private readonly IBaseConfiguration _baseConfiguration;
    private readonly ILogger _logger;

    private VanillaRouterRoute Route404;

    private readonly List<VanillaRouterRoute> Routes = new();

    public VanillaRouter(IBaseConfiguration baseConfiguration, ILogger<VanillaRouter> logger)
    {
        _logger = logger;
        _baseConfiguration = baseConfiguration;
    }

    public string Name => "Vanilla";


    public string EnvironmentVariableInsideRequest => "env.";

    public string RequestVariableInsideRequest => "request.";


    public void Begin(StringBuilder stringBuilder, bool fullWorker)
    {
    }

    public void AddRoute(StringBuilder stringBuilder, string relativePath, string fileHash, string responseCode)
    {
        Routes.Add(new VanillaRouterRoute { Path = relativePath, FileHash = fileHash, ResponseCode = responseCode });
    }

    public void Add404Route(StringBuilder stringBuilder, string responseCode)
    {
        Route404 = new VanillaRouterRoute { ResponseCode = responseCode };
    }

    public void End(StringBuilder stringBuilder, bool fullWorker)
    {
        _logger.LogInformation("-- Vanilla Router --");
        var tree = new VanillaRouterTree(Routes);
        var rootNode = tree.GetRoot();
        stringBuilder.AppendLine(VanillaHeader.Trim());
        TraverseAndAdd(stringBuilder, rootNode);
        stringBuilder.AppendLine(Route404?.ResponseCode);
        stringBuilder.AppendLine(VanillaFooter.Trim());
        _logger.LogInformation("-- Vanilla Router --");
    }

    private void TraverseAndAdd(StringBuilder builder, VanillaRouterTree.Node node, int depth = 0)
    {
        var first = true;
        // Root Node! Should Exist if there's an index.html! / Taken care of by earlier pre-processing
        if (depth == 0 && node.Route != null)
        {
            _logger.LogInformation($"{node.Segment}");
            builder.AppendLine($"if (path[{depth}] === \"{node.Segment}\") {{");
            builder.Append(node.Route.ResponseCode);
            builder.Append("}");
            first = false;
        }

        foreach (var child in node.Children)
        {
            var indentStr = string.Concat(Enumerable.Repeat("   ", depth));
            _logger.LogInformation($"{indentStr}{child.Segment}");

            builder.AppendLine(
                $"{indentStr}{(first == false ? "else" : "")} if (path[{depth}] === \"{child.Segment}\") {{");
            TraverseAndAdd(builder, child, depth + 1);
            if (child.Route != null)
            {
                builder.Append(indentStr + child.Route.ResponseCode);
            }

            builder.Append("}");
            first = false;
        }
    }
}

internal class VanillaRouterTree
{
    private readonly Node _root;

    public VanillaRouterTree(IEnumerable<VanillaRouterRoute> routes)
    {
        _root = new Node(null, null);

        foreach (var route in routes)
        {
            var current = _root;

            foreach (var segment in route.Path.Split('/').Where(s => !string.IsNullOrWhiteSpace(s)))
            {
                current = current.Children.FirstOrDefault(c => c.Segment == segment) ?? current.AddChild(segment);
            }

            current.Route = route;
        }
    }

    public Node GetRoot()
    {
        return _root;
    }

    public class Node
    {
        public Node(string segment, VanillaRouterRoute route)
        {
            Segment = segment;
            Route = route;
            Children = new List<Node>();
        }

        public string Segment { get; }
        public VanillaRouterRoute Route { get; set; }
        public List<Node> Children { get; }

        public Node AddChild(string segment)
        {
            var child = new Node(segment, null);
            Children.Add(child);
            return child;
        }
    }
}

internal class VanillaRouterRoute
{
    public string Path { get; set; }
    public string FileHash { get; set; }
    public string ResponseCode { get; set; }
}