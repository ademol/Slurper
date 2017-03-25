using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;

namespace Slurper
{
    class Program
    {

        private static Boolean Debug = false;
        private static String cfgFileName = "slurper.cfg";
        private static string ripDir = "rip";
        private static String targetDirPath;

        private static char pathSep = Path.DirectorySeparatorChar;
        private static string DefaultRegexPattern = @"(?i).*\.jpg";

        private static ArrayList filePatternsTolookfor = new ArrayList();
        private static ArrayList drivesToSearch = new ArrayList();

        private static ArrayList filesRipped = new ArrayList();

        private static char[] spinChars = new char[] { '|', '/', '-', '\\' };
        private static int spinCharIdx = 0;
        
        static void Main(string[] args)
        {
            //todo: more elegant handeling of parameters
            // help ?
            if (args.Length > 0 && args[0].Equals("/h")) { help(); }

            // Debug ?
            if (args.Length > 0 && args[0].Equals("/d")) { Debug = true; }

            // create target directory
            CreateTargetLocation();

            // configuration 
            Configure();

            // drives
            GetDriveInfo();

            // find files matching pattern(s) from all applicable drives, and copy them to ripdir
            SearchAndCopyFiles();

        }


        static void help()
        {
            //todo: nicer help
            String txt = "Copy files that have their filename matched, to ./rip/<hostname><timestamp> directory \n\n";
            txt += "In default mode (without cfg file) it matches jpg files with regex \n(i?).*\\\\.jpg\n\n";
            txt += "use the /d flag for verbose output => slurper.exe /d \n";
            txt += "\n";
            txt += "(optional) uses a configfile (./slurper.cfg) to specify custom regexes to match \n";
    
            Console.WriteLine(txt);
            Environment.Exit(0);
        }

        static void spin()
        {
            // fold back to begin char when needed
            if ( spinCharIdx + 1 == spinChars.Length )
            {
                spinCharIdx = 0;
            } else
            {
                spinCharIdx++;
            }

            char spinChar = spinChars[spinCharIdx];

            //set the spinner position to the current console position
            //Console.CursorLeft = Console.CursorLeft > 0 ? Console.CursorLeft - 1 : 0;
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
                String targetFileName = filename;

                targetFileName = targetFileName.Replace('\\', '_');
                targetFileName = targetFileName.Replace(':', '_');

                targetFileName = targetDirPath + pathSep + targetFileName;
                 
                if (Debug) { Console.WriteLine("ripping [{0}] => [{1}]", filename, targetFileName); }
                try
                {
                    //todo: move to http://alphafs.alphaleonis.com/ 
                    File.Copy(filename, targetFileName);
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
            if (Debug) { Console.WriteLine("mydrive = [{0}]", mydrive); }

            foreach (DriveInfo d in allDrives)
            {
                // skip the drive i'm running from
                if ((mydrive.ToUpper()).Equals(d.Name.ToUpper()))
                {
                    if (Debug) { Console.WriteLine("skipping mydrive [{0}]", mydrive); }
                    continue;
                }

                if (Debug) { Console.WriteLine(d.Name); }
                drivesToSearch.Add(d.Name);
            }

        }

        static void CreateTargetLocation()
        {
            String curDir = Directory.GetCurrentDirectory();
            String hostname = (System.Environment.MachineName).ToLower();
            String dateTime = String.Format("{0:yyyyMMdd_hh-mm-ss}", DateTime.Now);
            if (Debug) { Console.WriteLine("[{0}][{1}][{2}]", hostname, curDir, dateTime); }

            targetDirPath = string.Concat(curDir, pathSep, ripDir, pathSep, hostname, "_", dateTime);
            if (Debug) { Console.WriteLine(targetDirPath); }

            try
            {
                Directory.CreateDirectory(targetDirPath);


            }
            catch (Exception e)
            {
                Console.WriteLine("failed to create director [{0}][{1}]", targetDirPath, e.Message);

            }
        }

        static void Configure()
        {
            if (!LoadConfigFile())
            {
                // default config
                if (Debug) { Console.WriteLine("config file [{0}] not found => using defaults", cfgFileName); }

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
                    using (StreamReader sr = new StreamReader(cfgFileName))
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
                                if (Debug) { Console.WriteLine("[{0}] => regex:[{1}]", line, regex); }
                            }
                            else
                            {
                                if (Debug) { Console.WriteLine("[{0}] => regex:[{1}]", line, "---skipped---"); }
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
