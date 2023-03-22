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
using CloudflareWorkerBundler.Models.Configuration;
using CloudflareWorkerBundler.Services.Storage;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace CloudflareWorkerBundler;

public static class Program
{

    private const string outputFormat =
        "[{Timestamp:h:mm:ss ff tt}] [{Level:u3}] [{SourceContext}] {Message:lj} {Exception:j}{NewLine}";

    public static LoggingLevelSwitch _logLevelSwitch = new LoggingLevelSwitch(LogEventLevel.Information);



    /// <summary>
    ///     The entry point for the program.
    /// </summary>
    /// <param name="args">The arguments.</param>
    /// <returns>When complete, an integer representing success (0) or failure (non-0).</returns>
    public static async Task<int> Main(string[] args)
    {
        var serviceProvider = await BuildServiceProvider();
        var parser = BuildParser(serviceProvider);

        return await parser.InvokeAsync(args).ConfigureAwait(false);
    }

    private static Parser BuildParser(ServiceProvider serviceProvider)
    {
        var commandLineBuilder = new CommandLineBuilder();

        foreach (var command in serviceProvider.GetServices<Command>()) commandLineBuilder.Command.AddCommand(command);

        return commandLineBuilder.UseDefaults().Build();
    }

    private static async Task<ServiceProvider> BuildServiceProvider()
    {
        var services = new ServiceCollection();


        Log.Logger = new LoggerConfiguration().MinimumLevel
            .ControlledBy(_logLevelSwitch)
            .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Fatal)
            .WriteTo.Console(outputTemplate: outputFormat).Enrich.FromLogContext().CreateLogger();
        Log.Logger.Information("Loaded SeriLog Logger");


        // Load Config
        var baseConfig = await BaseConfiguration.Init();
        if (baseConfig.Verbose)
            _logLevelSwitch.MinimumLevel = LogEventLevel.Verbose;
        services.AddSingleton<IBaseConfiguration>(baseConfig);

        services.AddCliCommands();

        services.AddScoped<ICloudflareApiBroker, CloudflareApiBroker>();
        services.AddHttpClient<ICloudflareApiBroker, CloudflareApiBroker>()
            .SetHandlerLifetime(TimeSpan.FromMinutes(5))
            .AddPolicyHandler(GetRetryPolicy());

        services.AddScoped<IFileBundler, FileBundler>();
        services.AddScoped<IStorageCreatorService, StorageCreatorService>();
        services.AddScoped<IRouterCreatorService, RouterCreatorService>();
        services.AddLogging(builder => builder.AddSerilog());
        return services.BuildServiceProvider();
    }

    static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromMilliseconds(Math.Max(50, retryAttempt * 50)));
    }


}