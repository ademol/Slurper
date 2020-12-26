using System;
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
            var mountpoints = DriveInfo.GetDrives();

            var runLocation = Directory.GetCurrentDirectory();
            var runMountPoint =
                mountpoints.Where(j => runLocation.Contains(j.Name)).Max(j => j.Name);

            _logger.LogDebug($"GetDriveInfo: [{runMountPoint}]");

            foreach (var d in mountpoints)
            {
                var toBeIncluded = true;
                var reason = string.Empty;

                if (!IsValidMountPoint(d))
                {
                    toBeIncluded = false;
                    reason = "not applicable for this mountpoint/fs-type";
                }

                if (d.Name.Equals(runMountPoint))
                {
                    toBeIncluded = false;
                    reason = "cannot rip from target mount point";
                }

                if (toBeIncluded) ConfigurationService.PathList.Add(d.Name);

                _logger.LogInformation($"GetDriveInfo: found mount point [{d.Name}]\t included? [{toBeIncluded}]\t reason[{reason}]");
            }
        }

        public string? SanitizePath(string? path)
        {
            return path;
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
