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
        public static bool Force { get; set; } // undocumented: forces disk/mp running from to be considered 
        public static bool Verbose { get; set; } // show additional output what is done
        public static bool DryRun { get; set; } // (only) show what will be done (has implicit VERBOSE)
        public static bool Trace { get; set; } // VERBOSE + show also unmatched files 
        private static string CfgFileName { get; set; } = "slurper.cfg";                         // regex pattern(s) configuration file
        public static string RipDir { get; set; } = "rip";                                      // relative root directory for files to be copied to

        private static string DefaultRegexPattern { get; set; } = @"(?i).*\.jpg$";                // the default pattern that is used to search for jpg files

        public static List<string> PathList { get; } = new List<string>();                
        public static List<string> PatternsToMatch { get; } = new List<string>();   

        public static void Configure()
        {
            if (!LoadConfigFile() || PatternsToMatch.Count == 0)
            {
                // default config            
                Logger.Log($"Configure: config file [{CfgFileName}] not found, " +
                    $"or no valid patterns in file found => using default pattern [{DefaultRegexPattern}]", LogLevel.Warn);

                PatternsToMatch.Add(DefaultRegexPattern);
            }

            // show patterns used
            if (!Verbose) return;
            foreach (var pattern in PatternsToMatch)
            {
                Logger.Log($"Configure: Pattern to use: [{pattern}] ", LogLevel.Verbose);
            }
        }

        public static void InitSampleConfig()
        {
            // sample config
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "Slurper.slurper.cfg.txt";
            
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

        private static void GenerateSampleConfig()
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
            if (!File.Exists(CfgFileName)) return false;

            var cfgLoaded = false;
            const string regexPattern = @"^([^#].*)";
            var r = new Regex(regexPattern);
            try
            {
                using (var sr = new StreamReader(CfgFileName))
                {
                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine();
                        var m = r.Match(line ?? throw new InvalidOperationException());
                        if (m.Success)
                        {
                            var regex = m.Groups[1].Value;
                            Logger.Log($"LoadConfigFile: [{line}] => for regex:[{regex}]", LogLevel.Verbose);

                            PatternsToMatch.Add(regex);
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
                        GenerateSampleConfig();
                        Environment.Exit(0);
                        break;
                    case 'f':
                        Force = true;
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
