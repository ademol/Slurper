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




    
        private static string DefaultRegexPattern = @"(?i).*\.jpg";             // the default pattern that is used to search for jpg files

        private static ArrayList filePatternsTolookfor = new ArrayList();       // patterns to search  
        private static ArrayList drivesRequestedToBeSearched = new ArrayList(); // drives requested to searched base on configuration  ('c:'  'd:'  etc..  '.:'  means all)
        private static ArrayList drivesToSearch = new ArrayList();              // actual drives to search (always excludes the drive that the program is run from..)
        private static Dictionary<string, ArrayList> driveFilePatternsTolookfor = new Dictionary<string, ArrayList>();   // hash of drive keys with their pattern values 


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
            foreach (String drive in drivesToSearch)
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
            driveFilePatternsTolookfor.TryGetValue(curDrive.ToUpper(), out v);
            if (v != null) { thisDrivePatternsToLookFor.AddRange(v); }

            // add patterns for all drives
            driveFilePatternsTolookfor.TryGetValue(".:", out v);
            if (v != null) { thisDrivePatternsToLookFor.AddRange(v); }


            // long live the 'null-coalescing' operator ?? to handle cases of 'null'  :)
            foreach (string d in getDirs(sDir) ?? new String[0])
            {
                foreach (string f in getFiles(d) ?? new String[0])
                {
                    Spinner.Spin();

                    ConsoleLogger.Log($"[{f}]", ConsoleLogger.logLevel.TRACE);


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
                    Console.WriteLine("DirSearch: Could not read dir [{0}] [{1}]", d, e.Message);
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
                Console.WriteLine("getFiles: Failed to retrieve fileList from [{0}][{1}]", dir, e.Message);
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
                Console.WriteLine("getDirs: Failed to retrieve dirList from [{0}][{1}]", sDir, e.Message);
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
                if ( driveFilePatternsTolookfor.ContainsKey(".:"))
                {
                    driveToBeIncluded = true;
                    reason = "configuration for drive .:";
                }
                // check for specific drive
                if (driveFilePatternsTolookfor.ContainsKey(driveID))
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
                    drivesToSearch.Add(d.Name);
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
                Console.WriteLine("CreateTargetLocation: failed to create director [{0}][{1}]", FilePath.targetDirBasePath, e.Message);

            }
        }

        static void Configure()
        {
            if (!LoadConfigFile() || driveFilePatternsTolookfor.Count == 0)
            {
                // default config
                if (Configuration.VERBOSE) { Console.WriteLine("Configure: config file [{0}] not found, or no valid patterns in file found => using default pattern [{1}]", Configuration.cfgFileName, DefaultRegexPattern); }

                //// add a regex set as a default.
                //filePatternsTolookfor.Add(DefaultRegexPattern);

                //todo: check => add to driveFilePatternsTolookfor
                ArrayList defPattern = new ArrayList();
                defPattern.Add(DefaultRegexPattern);
                driveFilePatternsTolookfor.Add(".:", defPattern);
            }
            // show patterns used
            if (Configuration.VERBOSE)
            {
                foreach (String drive in driveFilePatternsTolookfor.Keys)
                {
                    ArrayList patterns;
                    driveFilePatternsTolookfor.TryGetValue(drive, out patterns);
                    foreach (String pattern in patterns)
                    {
                        Console.WriteLine("Configure: Pattern to use: disk [{0}]  pattern [{1}] ", drive, pattern);
                    }
                }
            }
        }

        static Boolean LoadConfigFile()
        {
            Boolean cfgLoaded = false;
            if (File.Exists(Configuration.cfgFileName))
            {
                String line;
                String REGEXpattern = @"^([^#]:)(.*)";               // pattern to match valid lines from config file   <driveLetter:><regex>
                Regex r = new Regex(REGEXpattern);
                try
                {
                    //todo: also move to alphafs ?
                    using (System.IO.StreamReader sr = new System.IO.StreamReader(Configuration.cfgFileName))
                    {
                        while (!sr.EndOfStream)
                        {
                            line = sr.ReadLine();
                            Match m = r.Match(line);
                            if (m.Success)
                            {
                                String drive = m.Groups[1].Value.ToUpper();
                                String regex = m.Groups[2].Value;
                                filePatternsTolookfor.Add(regex);
                                drivesRequestedToBeSearched.Add(drive);
                                if (Configuration.VERBOSE) { Console.WriteLine("LoadConfigFile: [{0}] => for drive:[{1}] regex:[{2}]", line, drive, regex); }

                                // add to hash
                                if (driveFilePatternsTolookfor.ContainsKey(drive))
                                {
                                    // add to existing key
                                    ArrayList t;
                                    driveFilePatternsTolookfor.TryGetValue(drive, out t);
                                    t.Add(regex);

                                }
                                else
                                {
                                    ArrayList t = new ArrayList();
                                    t.Add(regex);
                                    driveFilePatternsTolookfor.Add(drive, t);
                                }



                            }
                            else
                            {
                                if (Configuration.VERBOSE) { Console.WriteLine("LoadConfigFile: [{0}] => regex:[{1}]", line, "---skipped---"); }
                            }
                        }
                    }
                    cfgLoaded = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("LoadConfigFile: Could not read [{0}] [{1}]", Configuration.cfgFileName, e.Message);
                }

            }
            return cfgLoaded;
        }
    }
}
