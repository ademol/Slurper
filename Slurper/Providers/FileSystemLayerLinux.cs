using System;
using System.IO;
using System.Linq;
using Slurper.Contracts;
using Slurper.Logic;

namespace Slurper.Providers
{
    public class FileSystemLayerLinux : IFileSystemLayer
    {
        public ILogger Logger { get; } = LogProvider.Logger;

        public String TargetDirBasePath { get; set; }                             // relative directory for file to be copied to

        public char PathSep { get; } = Path.DirectorySeparatorChar;

        public void CreateTargetLocation()
        {
            var curDir = Directory.GetCurrentDirectory();
            var hostname = (Environment.MachineName).ToLower();
            var dateTime = String.Format("{0:yyyyMMdd_HH-mm-ss}", DateTime.Now);

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

        private bool IsValidFileSystem(string driveFormat) {
            string[] fileSystemsToSkip = { "sysfs", "proc", "tmpfs", "devpts", 
            "cgroupfs", "securityfs", "pstorefs", "mqueue", "debugfs", "hugetlbfs", "fusectl", 
            "fusectl", "isofs", "binfmt_misc" };
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
                // D:\  -> D:
                //String driveID = d.Name.Substring(0, 2).ToUpper();

                var mountPoint = d.Name;

                // check if drive will be included
                var driveToBeIncluded = false;
                var reason = "configuration";

                // check for wildcard
                if (Configuration.DriveFilePatternsTolookfor.ContainsKey(".:") && IsValidFileSystem(d.DriveFormat))
                {
                    driveToBeIncluded = true;
                    reason = "configuration for drivemapping .:";
                }

                // check for specific drive
                if (Configuration.DriveFilePatternsTolookfor.ContainsKey(mountPoint))
                {
                    driveToBeIncluded = true;
                    reason = "configuration for drive " + mountPoint;
                }

                // skip the drive i'm running from
                if (runMountPoint.Equals(d.Name))
                {
                    driveToBeIncluded = false;
                    reason = "this the drive i'm running from";
                }

                // include this drive
                if (driveToBeIncluded)
                {
                    Configuration.DrivesToSearch.Add(d.Name);
                }

                Logger.Log($"GetDriveInfo: found mountpoint [{mountPoint}]\t included? [{driveToBeIncluded}]\t reason[{reason}]", LogLevel.Verbose);
            }
        }
    }
}
