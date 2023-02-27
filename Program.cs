using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using CloudflareWorkerBundler.Broker;
using CloudflareWorkerBundler.Services.Bundler;
using CloudflareWorkerBundler.Services.Router;
using Microsoft.Extensions.DependencyInjection;
using Polly.Extensions.Http;
using Polly;
using CloudflareWorkerBundler.Extensions;

namespace CloudflareWorkerBundler;

public static class Program
{
    /// <summary>
    ///     The entry point for the program.
    /// </summary>
    /// <param name="args">The arguments.</param>
    /// <returns>When complete, an integer representing success (0) or failure (non-0).</returns>
    public static async Task<int> Main(string[] args)
    {
        var serviceProvider = BuildServiceProvider();
        var parser = BuildParser(serviceProvider);

        return await parser.InvokeAsync(args).ConfigureAwait(false);
    }

    private static Parser BuildParser(ServiceProvider serviceProvider)
    {
        var commandLineBuilder = new CommandLineBuilder();

        foreach (var command in serviceProvider.GetServices<Command>()) commandLineBuilder.Command.AddCommand(command);

        return commandLineBuilder.UseDefaults().Build();
    }

    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();


        services.AddCliCommands();

        services.AddScoped<ICloudflareAPIBroker, CloudflareAPIBroker>();
        services.AddHttpClient<ICloudflareAPIBroker, CloudflareAPIBroker>()
            .SetHandlerLifetime(TimeSpan.FromMinutes(5))
            .AddPolicyHandler(GetRetryPolicy());

        services.AddScoped<IFileBundler, FileBundler>();

        return services.BuildServiceProvider();
    }

    static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromMilliseconds(Math.Max(50, retryAttempt * 50)));
    }


}