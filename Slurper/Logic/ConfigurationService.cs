using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Slurper.Contracts;
using Slurper.Output;
using Slurper.Providers;

namespace Slurper.Logic
{
    public static class ConfigurationService
    {
        private static readonly ILogger Logger = LogProvider.Logger;
        public static string SampleConfig { get; private set; }
        public static bool Verbose { get; private set; }
        public static bool DryRun { get; private set; }
        public static bool Trace { get; private set; }
        private static string CfgFileName { get; } = "slurper.cfg";
        public static string DestinationDirectory { get; } = "rip";
        private static string DefaultPattern { get; } = @"(?i).*\.jpg$";
        public static List<string> PathList { get; } = new List<string>();
        public static List<string> PatternsToMatch { get; } = new List<string>();

        public static void Configure()
        {
            LoadConfigFile();

            if (NoPatternsLoaded()) AddDefaultConfig();

            if (!Verbose) return;

            LogPatterns();
        }

        private static bool NoPatternsLoaded()
        {
            return PatternsToMatch.Count == 0;
        }

        private static void AddDefaultConfig()
        {
            Logger.Log($"Configure: config file [{CfgFileName}] not found, " +
                       $"or no valid patterns in file found => using default pattern [{DefaultPattern}]",
                LogLevel.Warn);

            PatternsToMatch.Add(DefaultPattern);
        }

        private static void LogPatterns()
        {
            foreach (var pattern in PatternsToMatch)
                Logger.Log($"Configure: Pattern to use: [{pattern}] ", LogLevel.Verbose);
        }

        public static void InitSampleConfig()
        {
            var assembly = Assembly.GetExecutingAssembly();
            const string resourceName = "Slurper.slurper.cfg.txt";

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

        private static void LoadConfigFile()
        {
            if (!File.Exists(CfgFileName)) return;

            const string regexPattern = @"^([^#].*)";
            var r = new Regex(regexPattern);
            try
            {
                using var sr = new StreamReader(CfgFileName);
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
            catch (Exception e)
            {
                Logger.Log($"LoadConfigFile: Could not read[{CfgFileName}] [{e.Message}]", LogLevel.Error);
            }
        }

        public static void ProcessArguments(string[] args)
        {
            var charArguments = string.Join("", args);
            foreach (var c in charArguments)
                switch (c)
                {
                    case 'h':
                        DisplayMessages.Help();
                        Environment.Exit(0);
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
                    default:
                        Console.WriteLine("option [{0}] not supported", c);
                        DisplayMessages.Help();
                        Environment.Exit(0);
                        break;
                }

            Logger.Log($"Arguments: VERBOSE[{Verbose}] DRYRUN[{DryRun}] TRACE[{Trace}]", LogLevel.Verbose);
        }

        public static IFileSystemLayer ChoseFileSystemLayer()
        {
            IFileSystemLayer fileSystemLayer;

            var platformId = Environment.OSVersion.Platform;

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (platformId)
            {
                case PlatformID.Win32NT:
                    fileSystemLayer = new FileSystemLayerWindows();
                    break;
                case PlatformID.Unix:
                    fileSystemLayer = new FileSystemLayerLinux();
                    break;
                case PlatformID.MacOSX:
                    fileSystemLayer = new FileSystemLayerLinux();
                    break;
                default:
                    Console.WriteLine($"This [{platformId}] OS and/or its filesystem is not supported");
                    throw new NotSupportedException();
            }

            return fileSystemLayer;
        }
    }
}