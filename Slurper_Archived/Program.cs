﻿using System;
using Slurper.Contracts;
using Slurper.Logic;
using Slurper.Providers;

[assembly: CLSCompliant(true)]
namespace Slurper
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
        private static readonly IFileSearcher FileSearcher = new FileSearcher();

        static void Main(string[] args)
        {
            // init
            Configuration.InitSampleConfig();

            // handle arguments
            Configuration.ProcessArguments(args);

            if (Configuration.CmdLineFlagSet.Contains(CmdLineFlag.Generate)) {
                Configuration.GenerateSampleConfigFile();
                Environment.Exit(0);
            }

            // determine & create target directory
            SystemLayer.CreateTargetLocation();
            
            // configuration 
            Configuration.Configure();
            Configuration.ShowPatternsUsedByDrive();

            // get drives to search
            SystemLayer.GetDriveInfo();

            // find files matching pattern(s) from all applicable drives, and copy them to the targetLocation
            FileSearcher.DispatchDriveSearchers();
        }
    }
}
