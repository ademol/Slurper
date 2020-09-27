using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Slurper.Contracts;
using Slurper.Logic;

namespace Slurper.Providers
{
    public class OperatingSystemLayerWindows : IOperatingSystemLayer
    {
        private readonly ILogger<OperatingSystemLayerWindows> _logger;

        public OperatingSystemLayerWindows(ILogger<OperatingSystemLayerWindows> logger)
        {
            _logger = logger;
        }

        public string TargetDirBasePath { get; private set; } // relative directory for file to be copied to

        public char PathSep { get; } = Path.DirectorySeparatorChar;

        public void CreateTargetLocation()
        {
            var curDir = Directory.GetCurrentDirectory();
            var hostname = Environment.MachineName.ToLower();
            var dateTime = $"{DateTime.Now:yyyyMMdd_HH-mm-ss}";

            TargetDirBasePath = string.Concat(curDir, PathSep, ConfigurationService.DestinationDirectory, PathSep,
                hostname, "_", dateTime);
            _logger.LogDebug($"CreateTargetLocation: [{hostname}][{curDir}][{dateTime}][{TargetDirBasePath}]");

            try
            {
                if (!ConfigurationService.DryRun) Directory.CreateDirectory(TargetDirBasePath);
            }
            catch (Exception e)
            {
                _logger.LogError($"CreateTargetLocation: failed to create director [{TargetDirBasePath}][{e.Message}]");
            }
        }

        public void SetSourcePaths()
        {
            var allDrives = DriveInfo.GetDrives();

            var myDrive = Path.GetPathRoot(Directory.GetCurrentDirectory());
            _logger.LogDebug($"GetDriveInfo: myDrive = [{myDrive}]");

            foreach (var d in allDrives)
            {
                if (d.Name.Equals(myDrive?.ToUpper()))
                {
                    _logger.LogDebug($"GetDriveInfo: found drive [{d.Name}], but skipped i'm running from it");
                    continue;
                }

                ConfigurationService.PathList.Add(d.Name);

                _logger.LogDebug($"GetDriveInfo: found drive [{d.Name}]");
            }
        }

        public string SanitizePath(string path)
        {
            return path.Replace(':', '_');
        }
    }
}
