using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Slurper.Contracts;
using Slurper.Logic;

namespace Slurper.Providers
{
    public class FileSystemLayerLinux : IFileSystemLayer
    {
        private ILogger Logger { get; } = LogProvider.Logger;

       public string TargetDirBasePath { get; set; }
       public char PathSep { get; } = Path.DirectorySeparatorChar;

        public void CreateTargetLocation()
        {
            var curDir = Directory.GetCurrentDirectory();
            var hostname = (Environment.MachineName).ToLower();
            var dateTime = $"{DateTime.Now:yyyyMMdd_HH-mm-ss}";

            TargetDirBasePath = string.Concat(curDir, PathSep, Configuration.RipDir, PathSep, hostname, "_", dateTime);
            Logger.Log($"CreateTargetLocation: [{hostname}][{curDir}][{dateTime}][{TargetDirBasePath}]", LogLevel.Verbose);

            try
            {
                if (!Configuration.DryRun) { Directory.CreateDirectory(TargetDirBasePath); }
            }
            catch (Exception e)
            {
                Logger.Log($"CreateTargetLocation: failed to create director [{TargetDirBasePath}][{e.Message}]", LogLevel.Error);
            }
        }

        private bool IsValidMountPoint(DriveInfo drive)
        {
            return !IsOnExcludedTopLevelPath(drive.Name) && IsValidFileSystem(drive.DriveFormat);
        }

        private bool IsOnExcludedTopLevelPath(string path)
        {
            var topLevelRegex = new Regex("^(/[^/]*)", RegexOptions.IgnoreCase);
            var matcher = topLevelRegex.Match(path);
            var topLevelPath = path;
            if (matcher.Success)
            {
                topLevelPath = matcher.Value;
            }

            string[] excluded = { "/proc", "/sys", "/run" };
            return excluded.Contains(topLevelPath);
        }


        private bool IsValidFileSystem(string driveFormat)
        {
            if (driveFormat == null) { return false; }

            string[] fileSystemsToSkip = { "sysfs", "proc", "tmpfs", "devpts", 
            "cgroupfs", "securityfs", "pstorefs", "mqueue", "debugfs", "hugetlbfs", "fusectl", 
            "fusectl", "isofs", "binfmt_misc", "rpc_pipefs", "bpf", "cgroup", "cgroup2" };
            var fileSystemValid = ! fileSystemsToSkip.Contains(driveFormat);
            return fileSystemValid;
        }

        public void GetMountedPartitionInfo()
        {
            var mountpoints = DriveInfo.GetDrives();
            
            var runLocation = Directory.GetCurrentDirectory();
            var runMountPoint = 
            mountpoints.Where( j => runLocation.Contains(j.Name)).Max(j => j.Name);

            Logger.Log($"GetDriveInfo: [{runMountPoint}]", LogLevel.Verbose);

            foreach (var d in mountpoints)
            {
                var toBeIncluded = true;
                var reason = string.Empty;
                
                if (runMountPoint.Equals(d.Name) && ! Configuration.Force)
                {
                    toBeIncluded = false;
                    reason = "this the drive i'm running from";
                }
                
                if (! IsValidMountPoint(d))
                {
                    toBeIncluded = false;
                    reason = "not applicable for this mountpoint/fs-type";
                }
                
                
                if (toBeIncluded)
                {
                    Configuration.PathList.Add(d.Name);
                }

                Logger.Log($"GetDriveInfo: found mount point [{d.Name}]\t included? [{toBeIncluded}]\t reason[{reason}]", LogLevel.Verbose);
            }
        }
    }
}