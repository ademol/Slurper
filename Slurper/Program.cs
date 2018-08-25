using System;
using System.Text.RegularExpressions;
using System.Collections;
using Alphaleonis.Win32.Filesystem;
using System.Collections.Generic;
using System.Reflection;
using Slurper.Logic;

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

        static ILogger logger = new ConsoleLogger();

        static void Main(string[] args)
        {
            // init
            Configuration.InitSampleConfig();

            // handle arguments
            Configuration.ProcessArguments(args);

            // determine & create target directory
            CreateTargetLocation();

            // configuration 
            Configure();

            // get drives to search
            GetDriveInfo();

            // find files matching pattern(s) from all applicable drives, and copy them to ripdir
            SearchAndCopyFiles();

        }






      
        static void SearchAndCopyFiles()
        {
            // process each drive
            foreach (String drive in Configuration.drivesToSearch)
            {
                DirSearch(drive);
            }
        }

        static void DirSearch(string sDir)
        {


            //driveFilePatternsTolookfor
            // make sure to only use the patterns for the drives requested
            ArrayList thisDrivePatternsToLookFor = new ArrayList();
            // drive to search
            String curDrive = sDir.Substring(0, 2);    // aka c:  

            // add patterns for specific drive
            ArrayList v;
            Configuration.driveFilePatternsTolookfor.TryGetValue(curDrive.ToUpper(), out v);
            if (v != null) { thisDrivePatternsToLookFor.AddRange(v); }

            // add patterns for all drives
            Configuration.driveFilePatternsTolookfor.TryGetValue(".:", out v);
            if (v != null) { thisDrivePatternsToLookFor.AddRange(v); }


            // long live the 'null-coalescing' operator ?? to handle cases of 'null'  :)
            foreach (string d in getDirs(sDir) ?? new String[0])
            {
                foreach (string f in getFiles(d) ?? new String[0])
                {
                    Spinner.Spin();

                    logger.Log($"[{f}]", logLevel.TRACE);


                    // check if file is wanted by any of the specified patterns
                    foreach (String p in thisDrivePatternsToLookFor)
                    {
                        if ((new Regex(p).Match(f)).Success) { Fileripper.RipFile(f); continue; }
                    }

                }
                try
                {
                    DirSearch(d);
                }
                catch (Exception e)
                {
                    logger.Log($"DirSearch: Could not read dir [{d}][{e.Message}]", logLevel.ERROR);
                }
            }

        }

        static String[] getFiles(string dir)
        {

            try
            {
                String[] files = Directory.GetFiles(dir, "*.*");
                return files;
            }
            catch (Exception e)
            {
                logger.Log($"getFiles: Failed to retrieve fileList from [{dir}][{e.Message}]", logLevel.ERROR);
            }
            return null;
        }

        static String[] getDirs(string sDir)
        {
            try
            {
                string[] dirs = Directory.GetDirectories(sDir);
                return dirs;
            }
            catch (Exception e)
            {
                logger.Log($"getDirs: Failed to retrieve dirList from [{sDir}][{e.Message}]", logLevel.ERROR);
            }
            return null;
        }

        static void GetDriveInfo()
        {
            // look for possible drives to search 

            // all drives
            DriveInfo[] allDrives = DriveInfo.GetDrives();

            // mydrive
            String mydrive = Path.GetPathRoot(Directory.GetCurrentDirectory());
            if (Configuration.VERBOSE) { Console.WriteLine("GetDriveInfo: mydrive = [{0}]", mydrive); }

            foreach (DriveInfo d in allDrives)
            {
                // D:\  -> D:
                String driveID = d.Name.Substring(0, 2).ToUpper();
                
                // check if drive will be included
                Boolean driveToBeIncluded = false;
                String reason = "configuration";

                // check for wildcard
                if (Configuration.driveFilePatternsTolookfor.ContainsKey(".:"))
                {
                    driveToBeIncluded = true;
                    reason = "configuration for drive .:";
                }
                // check for specific drive
                if (Configuration.driveFilePatternsTolookfor.ContainsKey(driveID))
                {
                    driveToBeIncluded = true;
                    reason = "configuration for drive " + driveID;
                }


                // skip the drive i'm running from
                if ((mydrive.ToUpper()).Equals(d.Name.ToUpper()))
                {
                    driveToBeIncluded = false;
                    reason = "this the drive i'm running from";
                }

                // include this drive
                if (driveToBeIncluded)
                {
                    Configuration.drivesToSearch.Add(d.Name);
                }

                if (Configuration.VERBOSE) { Console.WriteLine("GetDriveInfo: found drive [{0}]\t included? [{1}]\t reason[{2}]", driveID, driveToBeIncluded, reason); }

            }

        }

        static void CreateTargetLocation()
        {
            String curDir = Directory.GetCurrentDirectory();
            String hostname = (System.Environment.MachineName).ToLower();
            String dateTime = String.Format("{0:yyyyMMdd_HH-mm-ss}", DateTime.Now);
            if (Configuration.VERBOSE) { Console.WriteLine("CreateTargetLocation: [{0}][{1}][{2}]", hostname, curDir, dateTime); }

            FilePath.targetDirBasePath = string.Concat(curDir, FilePath.pathSep, Configuration.ripDir, FilePath.pathSep, hostname, "_", dateTime);
            if (Configuration.VERBOSE) { Console.WriteLine("CreateTargetLocation: [{0}]", FilePath.targetDirBasePath); }

            try
            {
                if (!Configuration.DRYRUN) { Directory.CreateDirectory(FilePath.targetDirBasePath); }

            }
            catch (Exception e)
            {
                logger.Log($"CreateTargetLocation: failed to create director [{FilePath.targetDirBasePath}][{e.Message}]", logLevel.ERROR);
            }

        }
 
        static void Configure()
        {
            if (!Configuration.LoadConfigFile() || Configuration.driveFilePatternsTolookfor.Count == 0)
            {
                // default config            
                logger.Log($"Configure: config file [{Configuration.cfgFileName}] not found, " +
                    $"or no valid patterns in file found => using default pattern [{Configuration.DefaultRegexPattern}]", logLevel.WARN);

                //// add a regex set as a default.
                //filePatternsTolookfor.Add(DefaultRegexPattern);

                //todo: check => add to driveFilePatternsTolookfor
                ArrayList defPattern = new ArrayList();
                defPattern.Add(Configuration.DefaultRegexPattern);
                Configuration.driveFilePatternsTolookfor.Add(".:", defPattern);
            }
            // show patterns used
            if (Configuration.VERBOSE)
            {
                foreach (String drive in Configuration.driveFilePatternsTolookfor.Keys)
                {
                    ArrayList patterns;
                    Configuration.driveFilePatternsTolookfor.TryGetValue(drive, out patterns);
                    foreach (String pattern in patterns)
                    {
                        logger.Log($"Configure: Pattern to use: disk [{drive}]  pattern [{pattern}] ", logLevel.VERBOSE);
                    }
                }
            }
        }

       
    }
}
