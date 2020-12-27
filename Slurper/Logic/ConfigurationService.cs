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
        public static List<string> PathList { get; set; } = new List<string>();
        public static List<string> PatternsToMatch { get; } = new List<string>();

        private readonly ILogger<ConfigurationService> _logger;

        public ConfigurationService(ILogger<ConfigurationService> logger)
        {
            _logger = logger;
        }

        public void Configure()
        {
            LoadConfigFile();

            if (PatternsToMatch.Count == 0) AddDefaultConfig();

            LogPatterns();
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
                _logger.LogDebug($"Configure: Pattern to use: [{pattern}] ");
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
                        _logger.LogDebug($"LoadConfigFile: [{line}] => for regex:[{regex}]");

                        PatternsToMatch.Add(regex);
                    }
                    else
                    {
                        _logger.LogDebug($"LoadConfigFile: [{line}] => regex:[---skipped---]");
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"LoadConfigFile: Could not read[{CfgFileName}] [{e.Message}]");
            }
        }
    }
}
