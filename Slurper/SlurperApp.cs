using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Slurper.Contracts;
using Slurper.Logic;

namespace Slurper
{
    internal class SlurperApp
    {
        public static IOperatingSystemLayer OperatingSystemLayer;

        private readonly IConfigurationService _configurationService;
        private readonly ILogger<SlurperApp> _logger;
        private readonly Searcher _searcher;

        public SlurperApp(ILogger<SlurperApp> logger, Searcher searcher, IConfigurationService configurationService)
        {
            _logger = logger;
            _searcher = searcher;
            _configurationService = configurationService;
        }

        public async Task Run(string[] args)
        {
            Configure(args);

            OperatingSystemLayer.CreateTargetLocation();
            OperatingSystemLayer.SetSourcePaths();


           _searcher.SearchAndCopyFiles();
        }

        private void Configure(string[] args)
        {
            _configurationService.InitSampleConfig();
            _configurationService.ProcessArguments(args);
            _configurationService.Configure();

            OperatingSystemLayer = _configurationService.ChoseFileSystemLayer();
        }
    }
}
