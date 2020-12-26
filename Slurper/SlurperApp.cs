using System.Threading.Tasks;
using Slurper.Contracts;
using Slurper.Logic;

namespace Slurper
{
    internal class SlurperApp
    {
        public static IOperatingSystemLayer? OperatingSystemLayer;

        private readonly IConfigurationService _configurationService;
        private readonly Searcher _searcher;

        public SlurperApp(Searcher searcher, IConfigurationService configurationService)
        {
            _searcher = searcher;
            _configurationService = configurationService;
        }

        public async Task Run()
        {
            Configure();

            OperatingSystemLayer?.CreateTargetLocation();
            OperatingSystemLayer?.SetSourcePaths();
            await _searcher.SearchAndCopyFiles();
        }

        private void Configure()
        {
            _configurationService.Configure();

            OperatingSystemLayer = _configurationService.ChoseFileSystemLayer();
        }
    }
}
