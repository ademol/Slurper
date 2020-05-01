using System.Runtime.CompilerServices;
using Slurper.Contracts;
using Slurper.Logic;

[assembly: InternalsVisibleTo("SlurperTests")]

namespace Slurper
{
    internal static class Program
    {
        public static IFileSystemLayer FileSystemLayer { get; private set; }

        internal static void Main(string[] args)
        {
            Configure(args);

            FileSystemLayer.CreateTargetLocation();
            FileSystemLayer.SetSourcePaths();

            Searcher.SearchAndCopyFiles();
        }

        private static void Configure(string[] args)
        {
            ConfigurationService.InitSampleConfig();
            ConfigurationService.ProcessArguments(args);
            ConfigurationService.Configure();
            
            FileSystemLayer = ConfigurationService.ChoseFileSystemLayer();
        }
    }
}