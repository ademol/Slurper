using System;
using System.Text.RegularExpressions;
using System.Collections;
using Alphaleonis.Win32.Filesystem;
using System.Collections.Generic;

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

        private static Boolean VERBOSE = false;                                 // show additional output what is done
        private static Boolean DRYRUN = false;                                  // (only) show what will be done (has implicit VERBOSE)
        private static Boolean TRACE = false;                                   // VERBOSE + show also unmatched files 
        private static string ripDir = "rip";                                   // relative root directory for files to be copied to
        private static String cfgFileName = "slurper.cfg";                      // regex pattern(s) configuration file
        private static String targetDirBasePath;                                // relative directory for file to be copied to

        private static char pathSep = Path.DirectorySeparatorChar;
        private static string DefaultRegexPattern = @"(?i).*\.jpg";             // the default pattern that is used to search for jpg files

        private static ArrayList filePatternsTolookfor = new ArrayList();       // patterns to search  
        private static ArrayList drivesRequestedToBeSearched = new ArrayList(); // drives requested to searched base on configuration  ('c:'  'd:'  etc..  '.:'  means all)
        private static ArrayList drivesToSearch = new ArrayList();              // actual drives to search (always excludes the drive that the program is run from..)
        private static Dictionary<string, ArrayList> driveFilePatternsTolookfor = new Dictionary<string, ArrayList>();   // hash of drive keys with their pattern values 

        private static ArrayList filesRipped = new ArrayList();                 // files grabbed, to prevent multiple copies (in case of multiple matching patterns)

        private static char[] spinChars = new char[] { '|', '/', '-', '\\' };
        private static int spinCharIdx = 0;

        static void Main(string[] args)
        {
            // handle arguments
            Arguments(args);

            // determine & create target directory
            CreateTargetLocation();

            // configuration 
            Configure();

            // get drives to search
            GetDriveInfo();

            // find files matching pattern(s) from all applicable drives, and copy them to ripdir
            SearchAndCopyFiles();

        }

        static void Arguments(string[] args)
        {
            //todo: more elegant handeling of parameters  and allow for multiple param selection....
            // help ?
            if (args.Length > 0 && args[0].Equals("/h")) { help(); }

            // Verbose ?
            if (args.Length > 0 && args[0].Equals("/v")) { VERBOSE = true; }

            // dryrun
            if (args.Length > 0 && args[0].Equals("/d")) { DRYRUN = true; VERBOSE = true; }

            // trace
            if (args.Length > 0 && args[0].Equals("/t")) { VERBOSE = true; TRACE = true;  }

        }

        static void help()
        {
            //todo: nicer help
            String txt = "";
            txt += "Copy files that have their filename matched, to ./rip/<hostname><timestamp> directory \n\n";
            txt += "In default mode (without cfg file) it matches jpg files by the jpg extenstion\n";
            txt += "use the /v flag for verbose output => slurper.exe /v \n";
            txt += "use the /d flag for dryrun + verbose output (no filecopy mode) => slurper.exe /d      \n";
            txt += "\n";
            txt += "(optional) uses a configfile (./slurper.cfg) to specify custom regexes to match \n";
            txt += "\n";
            txt += "#################################\n";
            txt += "# sample config file            #\n";
            txt += "#################################\n";
            txt += "c:(?i).*\\.jpg\n";
            txt += "f:(?i).*\\.doc\n";
            txt += ".:(?i).*\\.mp3\n";
            txt += "c:(?i).*\\.txt\n";
            txt += "#################################\n";
            txt += "This searches for:\n";
            txt += "    jpg & txt files on the c: drive\n";
            txt += "    doc files on the f: drive\n";
            txt += "    mp3 files on all (.:) drives\n";


            Console.WriteLine(txt);
            Environment.Exit(0);
        }

        static void spin()
        {
            // fold back to begin char when needed
            if (spinCharIdx + 1 == spinChars.Length)
            { spinCharIdx = 0; }
            else
            { spinCharIdx++; }

            char spinChar = spinChars[spinCharIdx];

            //set the spinner position
            Console.CursorLeft = 0;

            //write the new character to the console
            Console.Write(spinChar);
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
                    //if (Debug) { Console.WriteLine(f); }
                    spin();

                    if (TRACE) {Console.WriteLine("TRACE:-[{0}]-",f); }

                    // check if file is wanted by any of the specified patterns
                    foreach (String p in thisDrivePatternsToLookFor)
                    {
                        if ((new Regex(p).Match(f)).Success) { RipFile(f); continue; }
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

        static void RipFile(String filename)
        {
            // process if not already downloaded  (in case of multiple regex matches)
            if (!filesRipped.Contains(filename))
            {
                filesRipped.Add(filename);

                // determine target filename
                String targetFileName = Path.GetFileName(filename);
                String targetRelativePath = Path.GetDirectoryName(filename);


                targetRelativePath = targetRelativePath.Replace(':', '_');
                String targetPath = targetDirBasePath + pathSep + targetRelativePath + pathSep;


                String targetFileNameFullPath = targetPath + targetFileName;

                if (VERBOSE) { Console.WriteLine("RipFile: ripping [{0}] => [{1}]", filename, targetFileNameFullPath); }
                try
                {
                    // do the filecopy unless this is a dryrun
                    if (!DRYRUN)
                    {
                        Directory.CreateDirectory(targetPath);
                        File.Copy(filename, targetFileNameFullPath);
                    }



                }
                catch (Exception e)
                {
                    Console.WriteLine("RipFile: copy of [{0}] failed with [{1}]", filename, e.Message);
                }
            }

        }

        static void GetDriveInfo()
        {
            // look for possible drives to search 

            // all drives
            DriveInfo[] allDrives = DriveInfo.GetDrives();

            // mydrive
            String mydrive = Path.GetPathRoot(Directory.GetCurrentDirectory());
            if (VERBOSE) { Console.WriteLine("GetDriveInfo: mydrive = [{0}]", mydrive); }

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

                if (VERBOSE) { Console.WriteLine("GetDriveInfo: found drive [{0}]\t included? [{1}]\t reason[{2}]", driveID, driveToBeIncluded, reason); }

            }

        }

        static void CreateTargetLocation()
        {
            String curDir = Directory.GetCurrentDirectory();
            String hostname = (System.Environment.MachineName).ToLower();
            String dateTime = String.Format("{0:yyyyMMdd_HH-mm-ss}", DateTime.Now);
            if (VERBOSE) { Console.WriteLine("CreateTargetLocation: [{0}][{1}][{2}]", hostname, curDir, dateTime); }

            targetDirBasePath = string.Concat(curDir, pathSep, ripDir, pathSep, hostname, "_", dateTime);
            if (VERBOSE) { Console.WriteLine("CreateTargetLocation: [{0}]", targetDirBasePath); }

            try
            {
                if (!DRYRUN) { Directory.CreateDirectory(targetDirBasePath); }

            }
            catch (Exception e)
            {
                Console.WriteLine("CreateTargetLocation: failed to create director [{0}][{1}]", targetDirBasePath, e.Message);

            }
        }

        static void Configure()
        {
            if (!LoadConfigFile() || driveFilePatternsTolookfor.Count == 0)
            {
                // default config
                if (VERBOSE) { Console.WriteLine("Configure: config file [{0}] not found, or no valid patterns in file found => using default pattern [{1}]", cfgFileName, DefaultRegexPattern); }

                //// add a regex set as a default.
                //filePatternsTolookfor.Add(DefaultRegexPattern);

                //todo: check => add to driveFilePatternsTolookfor
                ArrayList defPattern = new ArrayList();
                defPattern.Add(DefaultRegexPattern);
                driveFilePatternsTolookfor.Add(".:", defPattern);
            }
            // show patterns used
            if (VERBOSE)
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
            if (File.Exists(cfgFileName))
            {
                String line;
                String REGEXpattern = @"^([^#]:)(.*)";               // pattern to match valid lines from config file   <driveLetter:><regex>
                Regex r = new Regex(REGEXpattern);
                try
                {
                    //todo: also move to alphafs ?
                    using (System.IO.StreamReader sr = new System.IO.StreamReader(cfgFileName))
                    {
                        while (!sr.EndOfStream)
                        {
                            line = sr.ReadLine();
                            //if (Debug) { Console.WriteLine(line); }

                            Match m = r.Match(line);
                            if (m.Success)
                            {
                                String drive = m.Groups[1].Value.ToUpper();
                                String regex = m.Groups[2].Value;
                                filePatternsTolookfor.Add(regex);
                                drivesRequestedToBeSearched.Add(drive);
                                if (VERBOSE) { Console.WriteLine("LoadConfigFile: [{0}] => for drive:[{1}] regex:[{2}]", line, drive, regex); }

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
                                if (VERBOSE) { Console.WriteLine("LoadConfigFile: [{0}] => regex:[{1}]", line, "---skipped---"); }
                            }
                        }
                    }
                    cfgLoaded = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("LoadConfigFile: Could not read [{0}] [{1}]", cfgFileName, e.Message);
                }

            }
            return cfgLoaded;
        }
    }
}
