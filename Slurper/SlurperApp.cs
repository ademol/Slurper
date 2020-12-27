using System.Linq;
using System.Threading.Tasks;
using Slurper.Logic;
using Slurper.OperatingSystemLayers;

namespace Slurper
{
    internal class SlurperApp
    {
        private readonly IConfigurationService _configurationService;
        private readonly Searcher _searcher;

        public SlurperApp(Searcher searcher, IConfigurationService configurationService)
        {
            _searcher = searcher;
            _configurationService = configurationService;
        }

        public async Task Run()
        {

            _configurationService.Configure();

            var operatingSystemLayer = OperatingSystemLayerFactory.Create();
            operatingSystemLayer.CreateTargetLocation();
            ConfigurationService.PathList = operatingSystemLayer.GetSourcePaths().ToList();

            await _searcher.SearchAndCopyFiles();
        }
    }
}
