﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Slurper.Logic;

namespace Slurper.OperatingSystemLayers
{
    public class OperatingSystemLayerLinux : IOperatingSystemLayer
    {
        private readonly ILogger<OperatingSystemLayerLinux> _logger;

        public OperatingSystemLayerLinux(ILogger<OperatingSystemLayerLinux> logger)
        {
            _logger = logger;
        }

        public string? TargetDirBasePath { get; private set; }
        public char PathSep { get; } = Path.DirectorySeparatorChar;

        public void CreateTargetLocation()
        {
            var curDir = Directory.GetCurrentDirectory();
            var hostname = Environment.MachineName.ToLower();
            var dateTime = $"{DateTime.Now:yyyyMMdd_HH-mm-ss}";

            TargetDirBasePath = string.Concat(curDir, PathSep, ConfigurationService.DestinationDirectory, PathSep,
                hostname, "_", dateTime);
            _logger.LogDebug("CreateTargetLocation: [{Hostname}][{CurDir}][{DateTime}][{TargetDirBasePath}]", hostname, curDir, dateTime, TargetDirBasePath);

            try
            {
                if (!ConfigurationService.DryRun) Directory.CreateDirectory(TargetDirBasePath);
            }
            catch (Exception e)
            {
                _logger.LogError("CreateTargetLocation: failed to create director [{TargetDirBasePath}][{ExceptionMessage}]", TargetDirBasePath, e.Message);
            }
        }

        public IEnumerable<string> GetSourcePaths()
        {
            var paths = new List<string>();

            var mountpoints = DriveInfo.GetDrives();

            var runLocation = Directory.GetCurrentDirectory();
            var runMountPoint =
                mountpoints.Where(j => runLocation.Contains(j.Name)).Max(j => j.Name);

            _logger.LogDebug("GetDriveInfo: [{RunMountPoint}]", runMountPoint);

            foreach (var driveInfo in mountpoints)
            {
                var toBeIncluded = true;
                var reason = string.Empty;

                if (!IsValidMountPoint(driveInfo))
                {
                    toBeIncluded = false;
                    reason = "not applicable for this mountpoint/fs-type";
                }

                if (driveInfo.Name.Equals(runMountPoint))
                {
                    toBeIncluded = false;
                    reason = "cannot rip from target mount point";
                }

                if (toBeIncluded) paths.Add(driveInfo.Name);

                _logger.LogInformation("GetDriveInfo: found mount point [{PathName}]\t included? [{ToBeIncluded}]\t reason[{Reason}]", driveInfo.Name, toBeIncluded, reason);
            }

            return paths;
        }

        public string SanitizePath(string? path)
        {
            return path ?? string.Empty;
        }

        private static bool IsValidMountPoint(DriveInfo drive)
        {
            return !IsOnExcludedTopLevelPath(drive.Name) && IsValidFileSystem(drive.DriveFormat);
        }

        private static bool IsOnExcludedTopLevelPath(string path)
        {
            var topLevelRegex = new Regex("^(/[^/]*)", RegexOptions.IgnoreCase);
            var matcher = topLevelRegex.Match(path);
            var topLevelPath = path;
            if (matcher.Success) topLevelPath = matcher.Value;

            string[] excluded = {"/proc", "/sys", "/run"};
            return excluded.Contains(topLevelPath);
        }


        private static bool IsValidFileSystem(string driveFormat)
        {
            string[] fileSystemsToSkip =
            {
                "sysfs", "proc", "tmpfs", "devpts",
                "cgroupfs", "securityfs", "pstorefs", "mqueue", "debugfs", "hugetlbfs", "fusectl",
                "fusectl", "isofs", "binfmt_misc", "rpc_pipefs", "bpf", "cgroup", "cgroup2"
            };
            var fileSystemValid = !fileSystemsToSkip.Contains(driveFormat);
            return fileSystemValid;
        }
    }
}
