using System.CommandLine;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
namespace CloudflareWorkerBundler.Extensions
{
    //https://endjin.com/blog/2020/09/simple-pattern-for-using-system-commandline-with-dependency-injection
    public static class CliCommandCollectionExtensions
    {
        public static IServiceCollection AddCliCommands(this IServiceCollection services)
        {
            var commandType = typeof(Command);

            var commands =
                Assembly.GetExecutingAssembly()
                    .GetExportedTypes()
                    .Where(x => commandType.IsAssignableFrom(x));

            foreach (var command in commands) services.AddSingleton(commandType, command);


            return services;
        }
    }
}
