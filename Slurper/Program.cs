using System;
using System.Text.RegularExpressions;
using System.Collections;
using Alphaleonis.Win32.Filesystem;

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
        private static string ripDir = "rip";                                   // relative root directory for files to be copied to
        private static String cfgFileName = "slurper.cfg";                      // regex pattern(s) configuration file
        private static String targetDirBasePath;                                // relative directory for file to be copied to

        private static char pathSep = Path.DirectorySeparatorChar;
        private static string DefaultRegexPattern = @"(?i).*\.jpg";             // the default pattern that is used to search for jpg files

        private static ArrayList filePatternsTolookfor = new ArrayList();       // patterns to search  
        private static ArrayList drivesToSearch = new ArrayList();              // drives to search (excludes the drive that the program is run from..)

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
        }

        static void help()
        {
            //todo: nicer help
            String txt = "Copy files that have their filename matched, to ./rip/<hostname><timestamp> directory \n\n";
            txt += "In default mode (without cfg file) it matches jpg files with regex \n(i?).*\\\\.jpg\n\n";
            txt += "use the /v flag for verbose output => slurper.exe /v \n";
            txt += "use the /d flag for dryrun + verbose output (no filecopy mode) => slurper.exe /d      \n";
            txt += "\n";
            txt += "(optional) uses a configfile (./slurper.cfg) to specify custom regexes to match \n";

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

            // long live the 'null-coalescing' operator ?? to handle cases of 'null'  :)
            foreach (string d in getDirs(sDir) ?? new String[0])
            {
                foreach (string f in getFiles(d) ?? new String[0])
                {
                    //if (Debug) { Console.WriteLine(f); }
                    spin();

                    // check if file is wanted by any of the specified patterns
                    foreach (String p in filePatternsTolookfor)
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
                    Console.WriteLine("Could not read dir [{0}] [{1}]", d, e.Message);
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
                Console.WriteLine("Failed to retrieve fileList from [{0}][{1}]", dir, e.Message);
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
                Console.WriteLine("Failed to retrieve dirList from [{0}][{1}]", sDir, e.Message);
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

                if (VERBOSE) { Console.WriteLine("ripping [{0}] => [{1}]", filename, targetFileNameFullPath); }
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
                    Console.WriteLine("copy of [{0}] failed with [{1}]", filename, e.Message);
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
            if (VERBOSE) { Console.WriteLine("mydrive = [{0}]", mydrive); }

            foreach (DriveInfo d in allDrives)
            {
                // skip the drive i'm running from
                if ((mydrive.ToUpper()).Equals(d.Name.ToUpper()))
                {
                    if (VERBOSE) { Console.WriteLine("skipping mydrive [{0}]", mydrive); }
                    continue;
                }

                if (VERBOSE) { Console.WriteLine(d.Name); }
                drivesToSearch.Add(d.Name);
            }

        }

        static void CreateTargetLocation()
        {
            String curDir = Directory.GetCurrentDirectory();
            String hostname = (System.Environment.MachineName).ToLower();
            String dateTime = String.Format("{0:yyyyMMdd_hh-mm-ss}", DateTime.Now);
            if (VERBOSE) { Console.WriteLine("[{0}][{1}][{2}]", hostname, curDir, dateTime); }

            targetDirBasePath = string.Concat(curDir, pathSep, ripDir, pathSep, hostname, "_", dateTime);
            if (VERBOSE) { Console.WriteLine(targetDirBasePath); }

            try
            {
                if (!DRYRUN) { Directory.CreateDirectory(targetDirBasePath); }

            }
            catch (Exception e)
            {
                Console.WriteLine("failed to create director [{0}][{1}]", targetDirBasePath, e.Message);

            }
        }

        static void Configure()
        {
            if (!LoadConfigFile())
            {
                // default config
                if (VERBOSE) { Console.WriteLine("config file [{0}] not found => using defaults", cfgFileName); }

                // add a regex set as a default.
                filePatternsTolookfor.Add(DefaultRegexPattern);
            }
        }

        static Boolean LoadConfigFile()
        {
            Boolean cfgLoaded = false;
            if (File.Exists(cfgFileName))
            {
                String line;
                String REGEXpattern = @"^[^#].*";
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
                                String regex = m.Groups[0].Value;
                                filePatternsTolookfor.Add(regex);
                                if (VERBOSE) { Console.WriteLine("[{0}] => regex:[{1}]", line, regex); }
                            }
                            else
                            {
                                if (VERBOSE) { Console.WriteLine("[{0}] => regex:[{1}]", line, "---skipped---"); }
                            }
                        }
                    }
                    cfgLoaded = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Could not read [{0}] [{1}]", cfgFileName, e.Message);
                }

            }
            return cfgLoaded;
        }
    }
}
