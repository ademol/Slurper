using System;
using SlurperDotNetCore;
using SlurperDotNetCore.Contracts;
using SlurperDotNetCore.Logic;
using SlurperDotNetCore.Providers;

namespace SlurperDotNetCore
{
    class Program
    {
        /*
        * Sluper: Utility to search for files on a Windows computer that match one or more regex patterns. 
        *         The files found are then copied to a subdirectory in the location from where the program is run.
        *         
        *         note: 
        *         The drive that the program is run from, is excluded from searching.
        *           => suggested use is to run this program from an portable location (USB/HD) 
        *           
        */


        public static  IFileSystemLayer fileSystemLayer { get; private set; }

        static void Main(string[] args)
        {
            // init
            Configuration.InitSampleConfig();

            // handle arguments
            Configuration.ProcessArguments(args);

            fileSystemLayer = ChoseFileSystemLayer();


            // determine & create target directory
            fileSystemLayer.CreateTargetLocation();

            // configuration 
            Configuration.Configure();

            // get drives to search
            fileSystemLayer.GetDriveInfo();

            // find files matching pattern(s) from all applicable drives, and copy them to the targetLocation
            Searcher.SearchAndCopyFiles();
        }

        static IFileSystemLayer ChoseFileSystemLayer()
        {
            IFileSystemLayer fileSystemLayer;
            switch (System.Environment.OSVersion.Platform)
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
                    Console.WriteLine("This OS and/or its filesystem is not supported");
                    throw new NotSupportedException();
            }
            return fileSystemLayer;
        }
    }
}
