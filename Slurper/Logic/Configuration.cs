using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using Slurper.Providers;

namespace Slurper
{
    public static class Configuration
    {

        static readonly ILogger logger = LogProvider.Logger;

        public static List<char> argumentFlags { get; set; }
        public static string sampleConfig { get; set; }
        public static bool SILENT { get; set; } = false;

        public static bool VERBOSE { get; set; } = false;
        public static bool PARALLEL { get; set; } = false;
        public static bool DRYRUN { get; set; } = false;                                        // (only) show what will be done (has implicit VERBOSE)
        public static bool TRACE { get; set; } = false;                                         // VERBOSE + show also unmatched files 
        public static String cfgFileName { get; set; } = "slurper.cfg";                         // regex pattern(s) configuration file
        public static string ripDir { get; set; } = "rip";                                      // relative root directory for files to be copied to
        public static string defaultDriveRegexPattern { get; set; } = @".*\.jpg";         
        public static string ManualDriveRegexPattern { get; set; }


        public static List<string> drivesToSearch { get; } = new List<string>();                // actual drives to search (always excludes the drive that the program is run from..)
        public static Dictionary<string, List<string>> driveFilePatternsTolookfor { get; } = new Dictionary<string, List<string>>();   // hash of drive keys with their pattern values 

        public static void Configure()
        {
            if (ManualDriveRegexPattern != null)
            {
                LoadSingleConfiguration(ManualDriveRegexPattern);
            }
            else
            {
                if (!File.Exists(Configuration.cfgFileName))
                {
                    LoadSingleConfiguration(defaultDriveRegexPattern);
                    return;
                }
            }
            ShowPatternsUsedByDrive();

        }

        private static void LoadSingleConfiguration(string regexPattern)
        {
            logger.Log($"Configure:  using pattern [{regexPattern}]", LogLevel.WARN);

            string defaultDriverIdentifier = ".:";
            string driverIdentifier;


            Regex MatchDrive = new Regex(@"(^.:).*", RegexOptions.IgnoreCase);
            Match driveMatch = MatchDrive.Match(regexPattern);
            if (driveMatch.Success )
            {
                driverIdentifier = driveMatch.Groups[1].Value.ToUpperInvariant();
            } else
            {
                driverIdentifier = defaultDriverIdentifier;
            }

            Configuration.driveFilePatternsTolookfor.Add(driverIdentifier, new List<string> { regexPattern });
        }

        public static void ShowPatternsUsedByDrive()
        {
            foreach (String drive in Configuration.driveFilePatternsTolookfor.Keys)
            {
                Configuration.driveFilePatternsTolookfor.TryGetValue(drive, out List<string> patterns);
                foreach (String pattern in patterns)
                {
                    logger.Log($"Configure: Pattern to use: disk [{drive}]  pattern [{pattern}] ", LogLevel.VERBOSE);
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
                System.IO.StreamReader reader = new System.IO.StreamReader(stream);
                sampleConfig = reader.ReadToEnd();
            }
        }

        private static void generateSampleConfigFile()
        {
            Console.WriteLine("generating sample config file [{0}]", cfgFileName);
            try
            {
                System.IO.File.WriteAllText(cfgFileName, Configuration.sampleConfig);
            }
            catch (Exception e)
            {
                logger.Log($"generateConfig: failed to generate [{cfgFileName}][{e.Message}]", LogLevel.ERROR);
            }
        }


        public static void LoadConfigFile()
        {


            try
            {
                using (StreamReader streamReader = new StreamReader(Configuration.cfgFileName))
                {
                    while (!streamReader.EndOfStream)
                    {
                        ParseConfigLines(streamReader.ReadLine());
                    }
                }
                return;
            }
            catch (Exception e)
            {
                logger.Log($"LoadConfigFile: Could not read[{Configuration.cfgFileName}] [{e.Message}]", LogLevel.ERROR);
            }
        }

        private static void ParseConfigLines(string line)
        {
            // pattern to match valid lines from config file   <driveLetter:><remaining-regex>
            String ValidConfigLine = @"^([^#]:)\s*(.*)";


            Regex PatternToMatchValidConfigLine = new Regex(ValidConfigLine);
            Match matchedConfigurationLine = PatternToMatchValidConfigLine.Match(line);

            if (!matchedConfigurationLine.Success)
            {
                logger.Log($"LoadConfigFile: [{line}] => regex:[---skipped---]", LogLevel.VERBOSE);
                return;
            }

            String drive = matchedConfigurationLine.Groups[1].Value.ToUpper();
            String regex = matchedConfigurationLine.Groups[2].Value;

            StoreRegexToSearchByDrive(drive, regex);
            logger.Log($"LoadConfigFile: [{line}] => for drive:[{drive}] regex:[{regex}]", LogLevel.VERBOSE);

        }

        private static void StoreRegexToSearchByDrive(string drive, string regex)
        {
            List<string> driveFilePatterns = new List<string>();
            if (driveFilePatternsTolookfor.ContainsKey(drive))
                driveFilePatternsTolookfor.TryGetValue(drive, out driveFilePatterns);

            driveFilePatterns.Add(regex);
            driveFilePatternsTolookfor[drive] = driveFilePatterns;
        }

        public static void ExtractArgumentFlags(string argument)
        {
            foreach (char c in argument)
                argumentFlags.Add(c);
        }

        public static void ProcessArgumentFlags()
        {
            foreach (char c in argumentFlags)
            {
                switch (c)
                {
                    case 's':
                        SILENT = true;
                        break;
                    case 'h':
                        DisplayMessages.Help();
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
                    case 'p':
                        PARALLEL = true;
                        break;
                    case '/':
                        break;
                    case '-':
                        break;
                    case 'g':
                        generateSampleConfigFile();
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("option [{0}] not supported", c);
                        DisplayMessages.Help();
                        Environment.Exit(0);
                        break;
                }
            }
            logger.Log($"Arguments: VERBOSE[{VERBOSE}] DRYRUN[{DRYRUN}] TRACE[{TRACE}] PARALLEL[{PARALLEL}]", LogLevel.VERBOSE);
        }

        public static void ProcessArguments(string[] args)
        {
            List<string> patternKeywords = new List<string>();

            argumentFlags = new List<char>();
            foreach (var argument in args)
            {
                if (argument.StartsWith("/") || argument.StartsWith("-"))
                {
                    ExtractArgumentFlags(argument);
                }
                else
                {
                    patternKeywords.Add(argument);
                }
            }
            if (patternKeywords.Count > 0)
            {
                ManualDriveRegexPattern = patternKeywords.Aggregate((current, next) => current + @".*" + next) + @".*";
            }
            ProcessArgumentFlags();
        }
    }
}
