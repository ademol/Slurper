using System;
using System.IO;
using System.Linq;
using SlurperDotNetCore.Contracts;
using SlurperDotNetCore.Logic;

namespace SlurperDotNetCore.Providers
{
    public class FileSystemLayerLinux : IFileSystemLayer
    {
        public ILogger Logger { get; } = LogProvider.Logger;

        public String TargetDirBasePath { get; set; }                             // relative directory for file to be copied to

        public char PathSep { get; } = Path.DirectorySeparatorChar;

        public void CreateTargetLocation()
        {
            String curDir = Directory.GetCurrentDirectory();
            String hostname = (Environment.MachineName).ToLower();
            String dateTime = String.Format("{0:yyyyMMdd_HH-mm-ss}", DateTime.Now);

            TargetDirBasePath = string.Concat(curDir, PathSep, Configuration.RipDir, PathSep, hostname, "_", dateTime);
            Logger.Log($"CreateTargetLocation: [{hostname}][{curDir}][{dateTime}][{TargetDirBasePath}]", LogLevel.Verbose);

            try
            {
                if (!Configuration.Dryrun) { Directory.CreateDirectory(TargetDirBasePath); }
            }
            catch (Exception e)
            {
                Logger.Log($"CreateTargetLocation: failed to create director [{TargetDirBasePath}][{e.Message}]", LogLevel.Error);
            }
        }

        public bool IsValidFileSystem(string driveFormat) {
            string[] fileSystemsToSkip = { "sysfs", "proc", "tmpfs", "devpts", 
            "cgroupfs", "securityfs", "pstorefs", "mqueue", "debugfs", "hugetlbfs", "fusectl", 
            "fusectl", "isofs", "binfmt_misc" };
            bool fileSystemValid = ! fileSystemsToSkip.Contains(driveFormat);
            return fileSystemValid;
        }

        public void GetMountedPartitionInfo()
        {
            DriveInfo[] allMountpoints = DriveInfo.GetDrives();

            // mydrive
            String mylocation = Directory.GetCurrentDirectory();
            String myMountPoint = 
            allMountpoints.Where( j => mylocation.Contains(j.Name)).Max(j => j.Name);

            Logger.Log($"GetDriveInfo: mydrive = [{myMountPoint}]", LogLevel.Verbose);

            foreach (DriveInfo d in allMountpoints)
            {
                // D:\  -> D:
                //String driveID = d.Name.Substring(0, 2).ToUpper();

                string mountPoint = d.Name;

                // check if drive will be included
                Boolean driveToBeIncluded = false;
                String reason = "configuration";

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
                if (myMountPoint.Equals(d.Name))
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
