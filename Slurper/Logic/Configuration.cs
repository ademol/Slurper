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

        public static List<char> ArgumentFlags { get; set; }
        public static string SampleConfig { get; set; }
        public static bool SILENT { get; set; } = false;

        public static bool INCLUDEMYDRIVE { get; set; } = false;
        public static bool VERBOSE { get; set; } = false;
        public static bool DRYRUN { get; set; } = false;                                        // (only) show what will be done (has implicit VERBOSE)
        public static bool TRACE { get; set; } = false;                                         // VERBOSE + show also unmatched files 
        public static String CfgFileName { get; set; } = "slurper.cfg";                         // regex pattern(s) configuration file
        public static string RipDir { get; set; } = "rip";                                      // relative root directory for files to be copied to
        public static string DefaultDriveRegexPattern { get; set; } = @".*\.jpg";

        public static string ManualDriveRegexPattern { get; set; }

        public static List<string> DrivesToSearch { get; } = new List<string>();                // actual drives to search (always excludes the drive that the program is run from..)
        public static Dictionary<string, List<string>> DriveFileSearchPatterns { get; } = new Dictionary<string, List<string>>();   // hash of drive keys with their pattern values 

        public static void Configure()
        {

            if (ManualDriveRegexPattern != null)
            {
                LoadSingleRegexConfiguration(ManualDriveRegexPattern);
                return;
            }

            if (!File.Exists(Configuration.CfgFileName))
            {
                LoadSingleRegexConfiguration(DefaultDriveRegexPattern);
                return;
            }

            LoadConfigFile();
        }

        private static void LoadSingleRegexConfiguration(string regexPattern)
        {
            string driveIdentifier = ParseDriveIdentifierFromRegexPatternPattern(regexPattern);

            logger.Log($"Configure: using pattern [{regexPattern}] for drive [{driveIdentifier}]", LogLevel.WARN);
            Configuration.DriveFileSearchPatterns.Add(driveIdentifier, new List<string> { regexPattern });
        }

        private static string ParseDriveIdentifierFromRegexPatternPattern(string regexPattern)
        {
            string fallbackDriveIdentifier = ".:";
            Regex MatchDrive = new Regex(@"(^.:).*", RegexOptions.IgnoreCase);
            Match driveMatch = MatchDrive.Match(regexPattern);
            if (driveMatch.Success)
            {
                return driveMatch.Groups[1].Value.ToUpperInvariant();
            }
            else
            {
               return fallbackDriveIdentifier;
            }
        }

        public static void ShowPatternsUsedByDrive()
        {
            foreach (String drive in Configuration.DriveFileSearchPatterns.Keys)
            {
                Configuration.DriveFileSearchPatterns.TryGetValue(drive, out List<string> patterns);
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
                SampleConfig = reader.ReadToEnd();
            }
        }

        private static void generateSampleConfigFile()
        {
            Console.WriteLine("generating sample config file [{0}]", CfgFileName);
            try
            {
                System.IO.File.WriteAllText(CfgFileName, Configuration.SampleConfig);
            }
            catch (Exception e)
            {
                logger.Log($"generateConfig: failed to generate [{CfgFileName}][{e.Message}]", LogLevel.ERROR);
            }
        }

        public static void LoadConfigFile()
        {
            try
            {
                using (StreamReader streamReader = new StreamReader(Configuration.CfgFileName))
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
                logger.Log($"LoadConfigFile: Could not read[{Configuration.CfgFileName}] [{e.Message}]", LogLevel.ERROR);
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
            if (DriveFileSearchPatterns.ContainsKey(drive))
                DriveFileSearchPatterns.TryGetValue(drive, out driveFilePatterns);

            driveFilePatterns.Add(regex);
            DriveFileSearchPatterns[drive] = driveFilePatterns;
        }

        public static void ExtractArgumentFlags(string argument)
        {
            foreach (char c in argument)
                ArgumentFlags.Add(c);
        }

        public static void ProcessArgumentFlags()
        {
            foreach (char c in ArgumentFlags)
            {
                switch (c)
                {
                    case 'i':
                        INCLUDEMYDRIVE = true;
                        break;
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
            logger.Log($"Arguments: VERBOSE[{VERBOSE}] DRYRUN[{DRYRUN}] TRACE[{TRACE}]", LogLevel.VERBOSE);
        }

        public static bool IsValidRegex(string pattern)
        {
            if (string.IsNullOrEmpty(pattern)) return false;
            try
            {
                new Regex(pattern);
            } catch (ArgumentException)
            {
                return false;
            }
            return true;
        }

        public static void ProcessArguments(string[] args)
        {
            List<string> patternKeywords = new List<string>();

            ArgumentFlags = new List<char>();
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
                string buildRegex = patternKeywords.Aggregate((current, next) => current + @".*" + next) + @".*";
                if ( IsValidRegex(buildRegex))
                {
                    ManualDriveRegexPattern = buildRegex;
                } else
                {
                    string providedArguments = patternKeywords.Aggregate((current, next) => current + ", " + next);

                    Console.WriteLine($"Provided argument(s) [{providedArguments}] does not result in valid regex [{buildRegex}].");
                    Environment.Exit(1);
                }
            }
            ProcessArgumentFlags();
        }
    }
}
