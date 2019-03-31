using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Slurper.Contracts;
using Slurper.Providers;

namespace Slurper.Logic
{
    public static class Configuration
    {
        private static readonly ILogger Logger = LogProvider.Logger;

        public static readonly Collection<CmdLineFlag> CmdLineFlagSet = new Collection<CmdLineFlag>();

        public static string SampleConfig { get; set; }
        public static String CfgFileName { get; set; } = "slurper.cfg";                        
        public static string RipDir { get; set; } = "rip";                                      // relative root directory for files to be copied to
        public static string DefaultDriveRegexPattern { get; set; } = @".*\.jpg";
        public static string ManualDriveRegexPattern { get; set; }
        public static Collection<string> DrivesToSearch { get; } = new Collection<string>();    // actual drives to search (excludes the drive that the program is run from..)
        public static Dictionary<string, List<string>> DriveFileSearchPatterns { get; } =
            new Dictionary<string, List<string>>(); // hash of drive keys with their pattern values 

        public static void Configure()
        {
            if (ManualDriveRegexPattern != null)
            {
                LoadSingleRegexConfiguration(ManualDriveRegexPattern);
                return;
            }

            if (File.Exists(CfgFileName))
            {
                LoadConfigFile();
            }
            else
            {
                LoadSingleRegexConfiguration(DefaultDriveRegexPattern);
            }
        }

        private static void LoadSingleRegexConfiguration(string regexPattern)
        {
            string driveIdentifier = ParseDriveIdentifierFromRegexPatternPattern(regexPattern);
            Logger.Log($"Configure: using pattern [{regexPattern}] for drive [{driveIdentifier}]", LogLevel.Warn);
            DriveFileSearchPatterns.Add(driveIdentifier, new List<string> { regexPattern });
        }

        private static string ParseDriveIdentifierFromRegexPatternPattern(string regexPattern)
        {
            string fallbackDriveIdentifier = ".:";
            Regex matchDrive = new Regex(@"(^.:).*", RegexOptions.IgnoreCase);
            Match driveMatch = matchDrive.Match(regexPattern);
            if (driveMatch.Success)
            {
                return driveMatch.Groups[1].Value.ToUpperInvariant();
            }

            return fallbackDriveIdentifier;
        }

        public static void ShowPatternsUsedByDrive()
        {
            foreach (String drive in DriveFileSearchPatterns.Keys)
            {
                DriveFileSearchPatterns.TryGetValue(drive, out List<string> patterns);
                if (patterns != null)
                    foreach (String pattern in patterns)
                    {
                        Logger.Log($"Configure: Pattern to use: disk [{drive}]  pattern [{pattern}] ",
                            LogLevel.Verbose);
                    }
            }
        }

        public static void InitSampleConfig()
        {
            // sample config
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "Slurper.slurper.cfg.txt";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                StreamReader reader = new StreamReader(stream ?? throw new InvalidOperationException());
                SampleConfig = reader.ReadToEnd();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static void GenerateSampleConfigFile()
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static void LoadConfigFile()
        {
            try
            {
                using (StreamReader streamReader = new StreamReader(CfgFileName))
                {
                    while (!streamReader.EndOfStream)
                    {
                        ParseConfigLines(streamReader.ReadLine());
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log($"LoadConfigFile: Could not read[{CfgFileName}] [{e.Message}]", LogLevel.Error);
            }
        }

        private static void ParseConfigLines(string line)
        {
            // pattern to match valid lines from config file   <driveLetter:><remaining-regex>
            String ValidConfigLine = @"^([^#]:)\s*(.*)";

            Regex patternToMatchValidConfigLine = new Regex(ValidConfigLine);
            Match matchedConfigurationLine = patternToMatchValidConfigLine.Match(line);

            if (!matchedConfigurationLine.Success)
            {
                Logger.Log($"LoadConfigFile: [{line}] => regex:[---skipped---]", LogLevel.Verbose);
                return;
            }

            String drive = matchedConfigurationLine.Groups[1].Value.ToUpper();
            String regex = matchedConfigurationLine.Groups[2].Value;

            StoreRegexToSearchByDrive(drive, regex);
            Logger.Log($"LoadConfigFile: [{line}] => for drive:[{drive}] regex:[{regex}]", LogLevel.Verbose);
        }

        private static void StoreRegexToSearchByDrive(string drive, string regex)
        {
            List<string> driveFilePatterns = new List<string>();
            if (DriveFileSearchPatterns.ContainsKey(drive))
                DriveFileSearchPatterns.TryGetValue(drive, out driveFilePatterns);

            if (driveFilePatterns != null)
            {
                driveFilePatterns.Add(regex);
                DriveFileSearchPatterns[drive] = driveFilePatterns;
            }
        }

        private static bool IsValidRegex(string pattern)
        {
            if (string.IsNullOrEmpty(pattern)) return false;
            try
            {
                // ReSharper disable once ObjectCreationAsStatement
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

            foreach (var argument in args)
            {
                if (argument.StartsWith("/") || argument.StartsWith("-"))
                {
                    CommandLineFlagProcessor.ProcessArgumentFlags(argument);
                    if (CmdLineFlagSet.Contains(CmdLineFlag.Generate)) {
                        GenerateSampleConfigFile();
                        Environment.Exit(0);
                    }
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
        }
    }
}
