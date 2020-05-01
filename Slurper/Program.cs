using System;
using System.Runtime.CompilerServices;
using Slurper.Contracts;
using Slurper.Logic;
using Slurper.Providers;

[assembly: InternalsVisibleTo("SlurperTests")]

namespace Slurper
{
    internal static class Program
    {
        public static IFileSystemLayer FileSystemLayer { get; private set; }

        internal static void Main(string[] args)
        {
            Configuration.InitSampleConfig();

            Configuration.ProcessArguments(args);

            FileSystemLayer = ChoseFileSystemLayer();

            FileSystemLayer.CreateTargetLocation();

            Configuration.Configure();

            FileSystemLayer.GetFileSystemInformation();

            Searcher.SearchAndCopyFiles();
        }

        private static IFileSystemLayer ChoseFileSystemLayer()
        {
            IFileSystemLayer fileSystemLayer;

            var platformId = Environment.OSVersion.Platform;

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (platformId)
            {
                case PlatformID.Win32NT:
                    fileSystemLayer = new FileSystemLayerWindows();
                    break;
                case PlatformID.Unix:
                    fileSystemLayer = new FileSystemLayerLinux();
                    break;
                case PlatformID.MacOSX:
                    fileSystemLayer = new FileSystemLayerLinux();
                    break;
                default:
                    Console.WriteLine($"This [{platformId}] OS and/or its filesystem is not supported");
                    throw new NotSupportedException();
            }

            return fileSystemLayer;
        }
    }
}