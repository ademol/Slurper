﻿using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using Slurper.Logic;

namespace Slurper.OperatingSystemLayers
{
    public class OperatingSystemLayerWindows : IOperatingSystemLayer
    {
        private readonly ILogger<OperatingSystemLayerWindows> _logger;

        public OperatingSystemLayerWindows(ILogger<OperatingSystemLayerWindows> logger)
        {
            _logger = logger;
        }

        public string? TargetDirBasePath { get; private set; } // relative directory for file to be copied to

        public char PathSep { get; } = Path.DirectorySeparatorChar;

        public void CreateTargetLocation()
        {
            var curDir = Directory.GetCurrentDirectory();
            var hostname = Environment.MachineName.ToLower();
            var dateTime = $"{DateTime.Now:yyyyMMdd_HH-mm-ss}";

            TargetDirBasePath = string.Concat(
                curDir, PathSep, ConfigurationService.DestinationDirectory, PathSep, hostname, "_", dateTime);
            _logger.LogDebug("CreateTargetLocation: [{Hostname}][{CurDir}][{DateTime}][{TargetDirBasePath}]", hostname, curDir, dateTime, TargetDirBasePath);

            if (ConfigurationService.DryRun) return;
            
            try
            {
                Directory.CreateDirectory(TargetDirBasePath);
            }
            catch (Exception e)
            {
                _logger.LogError("CreateTargetLocation: failed to create director [{TargetDirBasePath}][{ExceptionMessage}]", TargetDirBasePath, e.Message);
            }
        }

        public IEnumerable<string> GetSourcePaths()
        {
            var paths = new List<string>();

            var allDrives = DriveInfo.GetDrives();

            var myDrive = Path.GetPathRoot(Directory.GetCurrentDirectory());
            _logger.LogDebug("GetDriveInfo: myDrive = [{MyDrive}]", myDrive);

            foreach (var driveInfo in allDrives)
            {
                if (driveInfo.Name.Equals(myDrive?.ToUpper()))
                {
                    _logger.LogDebug("GetDriveInfo: found drive [{DriveName}], but skipped i'm running from it", driveInfo.Name);
                    continue;
                }

                paths.Add(driveInfo.Name);

                _logger.LogDebug("GetDriveInfo: found drive [{DriveName}]", driveInfo.Name);
            }

            return paths;
        }

        public string SanitizePath(string? path)
        {
            return path != null ? path.Replace(':', '_') : string.Empty;
        }
    }
}
