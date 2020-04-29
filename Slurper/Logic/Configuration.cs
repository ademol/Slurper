using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Slurper.Contracts;
using Slurper.Output;
using Slurper.Providers;

namespace Slurper.Logic
{
    public static class Configuration
    {
        static readonly ILogger Logger = LogProvider.Logger;

        public static string SampleConfig { get; set; }
        public static bool Verbose { get; set; } // show additional output what is done
        public static bool DryRun { get; set; } // (only) show what will be done (has implicit VERBOSE)
        public static bool Trace { get; set; } // VERBOSE + show also unmatched files 
        private static string CfgFileName { get; set; } = "slurper.cfg";                         // regex pattern(s) configuration file
        public static string RipDir { get; set; } = "rip";                                      // relative root directory for files to be copied to

        private static string DefaultRegexPattern { get; set; } = @"(?i).*\.jpg";                // the default pattern that is used to search for jpg files

        // private static ArrayList FilePatternsTolookfor { get; } = new ArrayList();               // patterns to search  
        // private static ArrayList DrivesRequestedToBeSearched { get; } = new ArrayList();         // drives requested to searched base on configuration  ('c:'  'd:'  etc..  '.:'  means all)
        public static ArrayList DrivesToSearch { get; } = new ArrayList();                      // actual drives to search (always excludes the drive that the program is run from..)
        public static Dictionary<string, ArrayList> DriveFilePatternsTolookfor { get; } = new Dictionary<string, ArrayList>();   // hash of drive keys with their pattern values 

        public static void Configure()
        {
            if (!LoadConfigFile() || DriveFilePatternsTolookfor.Count == 0)
            {
                // default config            
                Logger.Log($"Configure: config file [{CfgFileName}] not found, " +
                    $"or no valid patterns in file found => using default pattern [{DefaultRegexPattern}]", LogLevel.Warn);

                //todo: check => add to driveFilePatternsTolookfor
                var defPattern = new ArrayList {DefaultRegexPattern};
                DriveFilePatternsTolookfor.Add(".:", defPattern);
            }

            // show patterns used
            if (Verbose)
            {
                foreach (var drive in DriveFilePatternsTolookfor.Keys)
                {
                    DriveFilePatternsTolookfor.TryGetValue(drive, out var patterns);
                    if (patterns != null)
                        foreach (String pattern in patterns)
                        {
                            Logger.Log($"Configure: Pattern to use: disk [{drive}]  pattern [{pattern}] ",
                                LogLevel.Verbose);
                        }
                }
            }
        }

        public static void InitSampleConfig()
        {
            // sample config
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "Slurper_Archived.slurper.cfg.txt";

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) return;
            try
            {
                var reader = new StreamReader(stream);
                SampleConfig = reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not read config file [{resourceName}] due to [{ex.Message}]");
            }
        }

        private static void GenerateConfig()
        {
            Console.WriteLine("generating sample config file [{0}]", CfgFileName);
            try
            {
                File.WriteAllText(CfgFileName, SampleConfig);
            }
            catch (Exception e)
            {
                Logger.Log($"generateConfig: failed to generate [{CfgFileName}][{e.Message}]", LogLevel.Error);
            }
        }

        private static bool LoadConfigFile()
        {
            var cfgLoaded = false;
            if (File.Exists(CfgFileName))
            {
                const string regexPattern = @"^([^#]:)(.*)"; // pattern to match valid lines from config file   <driveLetter:><regex>
                var r = new Regex(regexPattern);
                try
                {
                    //todo: also move to alphafs ?
                    using (var sr = new StreamReader(CfgFileName))
                    {
                        while (!sr.EndOfStream)
                        {
                            var line = sr.ReadLine();
                            var m = r.Match(line ?? throw new InvalidOperationException());
                            if (m.Success)
                            {
                                var drive = m.Groups[1].Value.ToUpper();
                                var regex = m.Groups[2].Value;
                                // FilePatternsTolookfor.Add(regex);
                                // DrivesRequestedToBeSearched.Add(drive);
                                Logger.Log($"LoadConfigFile: [{line}] => for drive:[{drive}] regex:[{regex}]", LogLevel.Verbose);

                                // add to hash
                                if (DriveFilePatternsTolookfor.ContainsKey(drive))
                                {
                                    // add to existing key
                                    DriveFilePatternsTolookfor.TryGetValue(drive, out var t);
                                    t?.Add(regex);
                                }
                                else
                                {
                                    var t = new ArrayList {regex};
                                    DriveFilePatternsTolookfor.Add(drive, t);
                                }
                            }
                            else
                            {
                                Logger.Log($"LoadConfigFile: [{line}] => regex:[---skipped---]", LogLevel.Verbose);
                            }
                        }
                    }

                    cfgLoaded = true;
                }
                catch (Exception e)
                {
                    Logger.Log($"LoadConfigFile: Could not read[{CfgFileName}] [{e.Message}]", LogLevel.Error);
                }
            }

            return cfgLoaded;
        }

        public static void ProcessArguments(string[] args)
        {
            // concat the arguments, handle each char as switch selection  (ignore any '/' or '-')
            var concat = string.Join("", args);
            foreach (var c in concat)
            {
                switch (c)
                {
                    case 'h':
                        DisplayMessages.Help();
                        break;
                    case 'v':
                        Verbose = true;
                        break;
                    case 'd':
                        DryRun = true;
                        break;
                    case 't':
                        Trace = true;
                        Verbose = true;
                        break;
                    case '/':
                        break;
                    case '-':
                        break;
                    case 'g':
                        GenerateConfig();
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("option [{0}] not supported", c);
                        DisplayMessages.Help();
                        break;
                }
            }

            Logger.Log($"Arguments: VERBOSE[{Verbose}] DRYRUN[{DryRun}] TRACE[{Trace}]", LogLevel.Verbose);
        }
    }
}
