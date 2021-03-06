﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace Slurper.Logic
{
    public interface IConfigurationService
    {
        void Configure();
    }

    public class ConfigurationService : IConfigurationService
    {
        public static string? SampleConfig { get; private set; }
        public static bool DryRun { get; set; }
        private static string CfgFileName { get; } = "slurper.cfg";
        public static string DestinationDirectory { get; } = "rip";
        private static string DefaultPattern { get; } = @"(?i).*\.jpg$";
        public static List<string> PathList { get; set; } = new();
        public static List<string> PatternsToMatch { get; } = new();

        private readonly ILogger<ConfigurationService> _logger;

        public ConfigurationService(ILogger<ConfigurationService> logger)
        {
            _logger = logger;
        }

        public void Configure()
        {
            LoadConfigFile();
            if (NoConfigurationLoaded) AddFallbackConfiguration();
            LogConfiguration();
        }

        private static bool NoConfigurationLoaded => PatternsToMatch.Count == 0;

        private void AddFallbackConfiguration()
        {
            _logger.LogWarning("Configure: config file [{CfgFileName}] not found, " +
                               "or no valid patterns in file found => using default pattern [{DefaultPattern}]", CfgFileName, DefaultPattern);

            PatternsToMatch.Add(DefaultPattern);
        }

        private void LogConfiguration()
        {
            foreach (var pattern in PatternsToMatch)
                _logger.LogDebug("Configure: Pattern to use: [{Pattern}] ", pattern);
        }

        private static void InitSampleConfig()
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

        public static void GenerateSampleConfig()
        {
            InitSampleConfig();

            Console.WriteLine("generating sample config file [{0}]", CfgFileName);
            try
            {
                File.WriteAllText(CfgFileName, SampleConfig);
            }
            catch (Exception e)
            {
                Console.WriteLine($"generateConfig: failed to generate [{CfgFileName}][{e.Message}]");
            }
        }

        private void LoadConfigFile()
        {
            if (!File.Exists(CfgFileName)) return;

            const string regexPattern = @"^([^#].*)";
            var patternRegex = new Regex(regexPattern);
            try
            {
                using var sr = new StreamReader(CfgFileName);
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    var match = patternRegex.Match(line ?? throw new InvalidOperationException());
                    if (match.Success)
                    {
                        var regex = match.Groups[1].Value;
                        _logger.LogDebug("LoadConfigFile: [{Line}] => for regex:[{Regex}]", line, regex);

                        PatternsToMatch.Add(regex);
                    }
                    else
                    {
                        _logger.LogDebug("LoadConfigFile: [{Line}] => regex:[---skipped---]", line);
                    }
                }
            }
            catch (Exception)
            {
                _logger.LogError("LoadConfigFile: Could not read[{CfgFileName}] [{e.Message}]", CfgFileName);
            }
        }
    }
}
