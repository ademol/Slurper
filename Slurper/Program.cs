using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Slurper.Logic;
using Slurper.Output;
using Slurper.Providers;

[assembly: InternalsVisibleTo("SlurperTests")]

namespace Slurper
{
    internal static class Program
    {
        private static LogLevel _minLogLevel = LogLevel.Warning;

        internal static void Main(string[] args)
        {
            ProcessArguments(args);

            var services = new ServiceCollection();
            ConfigureServices(services);
            using var serviceProvider = services.BuildServiceProvider();
            Task.WaitAll(serviceProvider.GetService<SlurperApp>().Run());
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<SlurperApp>()
                .AddScoped<Searcher>()
                .AddScoped<FileRipper>()
                .AddSingleton<OperatingSystemLayerWindows>()
                .AddSingleton<OperatingSystemLayerLinux>()
                .AddSingleton<IConfigurationService, ConfigurationService>()
                .AddLogging(c => c.AddConsole().SetMinimumLevel(_minLogLevel));
        }

        private static void ProcessArguments(string[] args)
        {
            var charArguments = string.Join("", args);
            foreach (var c in charArguments)
            {
                switch (c)
                {
                    case 'h':
                        DisplayMessages.Help();
                        Environment.Exit(0);
                        break;
                    case 'v':
                        _minLogLevel = LogLevel.Debug;
                        break;
                    case 'd':
                        ConfigurationService.DryRun = true;
                        break;
                    case 't':
                        _minLogLevel = LogLevel.Trace;
                        break;
                    case '/':
                        break;
                    case '-':
                        break;
                    case 'g':
                        ConfigurationService.GenerateSampleConfig();
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("option [{0}] not supported", c);
                        DisplayMessages.Help();
                        Environment.Exit(0);
                        break;
                }
            }
        }
    }
}
