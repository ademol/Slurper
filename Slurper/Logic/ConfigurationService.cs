using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Slurper.Contracts;
using Slurper.Output;
using Slurper.Providers;

namespace Slurper.Logic
{
    public interface IConfigurationService
    {
        void Configure();
        void InitSampleConfig();
        void ProcessArguments(string[] args);
        IOperatingSystemLayer ChoseFileSystemLayer();
    }

    public class ConfigurationService : IConfigurationService
    {
        public static string SampleConfig { get; private set; }
        public static bool Verbose { get; private set; }
        public static bool DryRun { get; private set; }
        public static bool Trace { get; private set; }
        private static string CfgFileName { get; } = "slurper.cfg";
        public static string DestinationDirectory { get; } = "rip";
        private static string DefaultPattern { get; } = @"(?i).*\.jpg$";
        public static List<string> PathList { get; } = new List<string>();
        public static List<string> PatternsToMatch { get; } = new List<string>();

        private readonly ILogger<ConfigurationService> _logger;

        private readonly OperatingSystemLayerWindows _operatingSystemLayerWindows;
        private readonly OperatingSystemLayerLinux _operatingSystemLayerLinux;

        public ConfigurationService(ILogger<ConfigurationService> logger, OperatingSystemLayerWindows operatingSystemLayerWindows, OperatingSystemLayerLinux operatingSystemLayerLinux)
        {
            _logger = logger;
            _operatingSystemLayerWindows = operatingSystemLayerWindows;
            _operatingSystemLayerLinux = operatingSystemLayerLinux;
        }

        public void Configure()
        {
            LoadConfigFile();

            if (NoPatternsLoaded()) AddDefaultConfig();

            if (!Verbose) return;

            LogPatterns();
        }

        private bool NoPatternsLoaded()
        {
            return PatternsToMatch.Count == 0;
        }

        private void AddDefaultConfig()
        {
            _logger.LogWarning($"Configure: config file [{CfgFileName}] not found, " +
                        $"or no valid patterns in file found => using default pattern [{DefaultPattern}]");

            PatternsToMatch.Add(DefaultPattern);
        }

        private void LogPatterns()
        {
            foreach (var pattern in PatternsToMatch)
                _logger.LogInformation($"Configure: Pattern to use: [{pattern}] ");
        }

        public void InitSampleConfig()
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

        private void GenerateSampleConfig()
        {
            Console.WriteLine("generating sample config file [{0}]", CfgFileName);
            try
            {
                File.WriteAllText(CfgFileName, SampleConfig);
            }
            catch (Exception e)
            {
                _logger.LogError($"generateConfig: failed to generate [{CfgFileName}][{e.Message}]");
            }
        }

        private void LoadConfigFile()
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
                        _logger.LogInformation($"LoadConfigFile: [{line}] => for regex:[{regex}]");

                        PatternsToMatch.Add(regex);
                    }
                    else
                    {
                        _logger.LogInformation($"LoadConfigFile: [{line}] => regex:[---skipped---]");
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"LoadConfigFile: Could not read[{CfgFileName}] [{e.Message}]");
            }
        }

        public void ProcessArguments(string[] args)
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

            _logger.LogInformation($"Arguments: VERBOSE[{Verbose}] DRYRUN[{DryRun}] TRACE[{Trace}]");
        }

        public IOperatingSystemLayer ChoseFileSystemLayer()
        {
            IOperatingSystemLayer operatingSystemLayer;

            var platformId = Environment.OSVersion.Platform;

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (platformId)
            {
                case PlatformID.Win32NT:
                    operatingSystemLayer = _operatingSystemLayerWindows;
                    break;
                case PlatformID.Unix:
                    operatingSystemLayer = _operatingSystemLayerWindows;
                    break;
                case PlatformID.MacOSX:
                    operatingSystemLayer = _operatingSystemLayerLinux;
                    break;
                default:
                    Console.WriteLine($"This [{platformId}] OS and/or its filesystem is not supported");
                    throw new NotSupportedException();
            }

            return operatingSystemLayer;
        }
    }
}
