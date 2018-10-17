using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

using SlurperDotNetCore.Providers;


namespace SlurperDotNetCore
{
    static public class Configuration
    {
        static readonly ILogger logger = LogProvider.Logger;

        public static string sampleConfig { get; set; }
        public static bool VERBOSE { get; set; } = false;                                       // show additional output what is done
        public static bool DRYRUN { get; set; } = false;                                        // (only) show what will be done (has implicit VERBOSE)
        public static bool TRACE { get; set; } = false;                                         // VERBOSE + show also unmatched files 
        public static String cfgFileName { get; set; } = "slurper.cfg";                         // regex pattern(s) configuration file
        public static string ripDir { get; set; } = "rip";                                      // relative root directory for files to be copied to

        public static string DefaultRegexPattern { get; set; } = @"(?i).*\.jpg";                // the default pattern that is used to search for jpg files

        public static ArrayList filePatternsTolookfor { get; } = new ArrayList();               // patterns to search  
        public static ArrayList drivesRequestedToBeSearched { get; } = new ArrayList();         // drives requested to searched base on configuration  ('c:'  'd:'  etc..  '.:'  means all)
        public static ArrayList drivesToSearch { get; } = new ArrayList();                      // actual drives to search (always excludes the drive that the program is run from..)
        public static Dictionary<string, ArrayList> driveFilePatternsTolookfor { get; } = new Dictionary<string, ArrayList>();   // hash of drive keys with their pattern values 

        public static void Configure()
        {
            if (!Configuration.LoadConfigFile() || Configuration.driveFilePatternsTolookfor.Count == 0)
            {
                // default config            
                logger.Log($"Configure: config file [{Configuration.cfgFileName}] not found, " +
                    $"or no valid patterns in file found => using default pattern [{Configuration.DefaultRegexPattern}]", logLevel.WARN);

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

        public static void InitSampleConfig()
        {
            // sample config
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "Slurper.slurper.cfg.txt";

            using (System.IO.Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                try
                {
                    System.IO.StreamReader reader = new System.IO.StreamReader(stream);
                    sampleConfig = reader.ReadToEnd();
                } catch (Exception ex)
                {
                    Console.WriteLine($"Could not read config file [{resourceName}] due to [{ex.Message}]");
                }
            }
        }

        private static void generateConfig()
        {
            Console.WriteLine("generating sample config file [{0}]", cfgFileName);
            try
            {
                System.IO.File.WriteAllText(cfgFileName, Configuration.sampleConfig);
            }
            catch (Exception e)
            {
                logger.Log($"generateConfig: failed to generate [{cfgFileName}][{e.Message}]", logLevel.ERROR);
            }
        }

        public static Boolean LoadConfigFile()
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
                    using (StreamReader sr = new StreamReader(Configuration.cfgFileName))
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
                                logger.Log($"LoadConfigFile: [{line}] => for drive:[{drive}] regex:[{regex}]", logLevel.VERBOSE);

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
                                logger.Log($"LoadConfigFile: [{line}] => regex:[---skipped---]", logLevel.VERBOSE);
                            }
                        }
                    }
                    cfgLoaded = true;
                }
                catch (Exception e)
                {
                    logger.Log($"LoadConfigFile: Could not read[{Configuration.cfgFileName}] [{e.Message}]", logLevel.ERROR);
                }

            }
            return cfgLoaded;
        }

        public static void ProcessArguments(string[] args)
        {
            // concat the arguments, handle each char as switch selection  (ignore any '/' or '-')
            string concat = String.Join("", args);
            foreach (char c in concat)
            {
                switch (c)
                {
                    case 'h':
                        DisplayMessages.help();
                        break;
                    case 'v':
                        VERBOSE = true;
                        break;
                    case 'd':
                        DRYRUN = true;
                        break;
                    case 't':
                        TRACE = true;
                        VERBOSE = true;
                        break;
                    case '/':
                        break;
                    case '-':
                        break;
                    case 'g':
                        generateConfig();
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("option [{0}] not supported", c);
                        DisplayMessages.help();
                        break;
                }
            }
            logger.Log($"Arguments: VERBOSE[{VERBOSE}] DRYRUN[{DRYRUN}] TRACE[{TRACE}]", logLevel.VERBOSE);
        }
    }
}
