using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Slurper.Logic;
using Slurper.Providers;

[assembly: InternalsVisibleTo("SlurperTests")]

namespace Slurper
{
    internal static class Program
    {
        internal static void Main(string[] args)
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            using var serviceProvider = services.BuildServiceProvider();
            Task.WaitAll(serviceProvider.GetService<SlurperApp>().Run(args));
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<SlurperApp>()
                .AddScoped<Searcher>()
                .AddScoped<FileRipper>()
                .AddSingleton<OperatingSystemLayerWindows>()
                .AddSingleton<OperatingSystemLayerLinux>()
                .AddSingleton<IConfigurationService, ConfigurationService>()
                .AddLogging(c => c.AddConsole());
        }
    }
}
