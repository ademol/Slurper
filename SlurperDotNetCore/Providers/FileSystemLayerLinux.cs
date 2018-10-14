using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using SlurperDotNetCore.Contracts;

namespace SlurperDotNetCore.Providers
{
    public class FileSystemLayerLinux : IFileSystemLayer
    {
        public ILogger logger { get; } = LogProvider.Logger;

        public String targetDirBasePath { get; set; }                             // relative directory for file to be copied to

        public char pathSep { get; } = Path.DirectorySeparatorChar;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void CreateTargetLocation()
        {
            String curDir = Directory.GetCurrentDirectory();
            String hostname = (System.Environment.MachineName).ToLower();
            String dateTime = String.Format("{0:yyyyMMdd_HH-mm-ss}", DateTime.Now);

            targetDirBasePath = string.Concat(curDir, pathSep, Configuration.ripDir, pathSep, hostname, "_", dateTime);
            logger.Log($"CreateTargetLocation: [{hostname}][{curDir}][{dateTime}][{targetDirBasePath}]", logLevel.VERBOSE);

            try
            {
                if (!Configuration.DRYRUN) { Directory.CreateDirectory(targetDirBasePath); }
            }
            catch (Exception e)
            {
                logger.Log($"CreateTargetLocation: failed to create director [{targetDirBasePath}][{e.Message}]", logLevel.ERROR);
            }
        }

        public bool IsValidFileSystem(string driveFormat) {
            string[] fileSystemsToSkip = new string[]{ "sysfs", "proc", "tmpfs", "devpts", 
            "cgroupfs", "securityfs", "pstorefs", "mqueue", "debugfs", "hugetlbfs", "fusectl", 
            "fusectl", "isofs", "binfmt_misc" };
            bool fileSystemValid = ! fileSystemsToSkip.Contains(driveFormat);
            return fileSystemValid;
        }

        public void GetMountedPartitionInfo()
        {
            DriveInfo[] allMountpoints = DriveInfo.GetDrives();

            // mydrive
            var t = Directory.GetCurrentDirectory();
            String mylocation = Directory.GetCurrentDirectory();
            String myMountPoint = 
            allMountpoints.Where( j => mylocation.Contains(j.Name)).Max(j => j.Name);

            logger.Log($"GetDriveInfo: mydrive = [{myMountPoint}]", logLevel.VERBOSE);

            foreach (DriveInfo d in allMountpoints)
            {
                // D:\  -> D:
                //String driveID = d.Name.Substring(0, 2).ToUpper();

                string mountPoint = d.Name.ToString();

                // check if drive will be included
                Boolean driveToBeIncluded = false;
                String reason = "configuration";

                // check for wildcard
                if (Configuration.driveFilePatternsTolookfor.ContainsKey(".:") && IsValidFileSystem(d.DriveFormat))
                {
                    
                    driveToBeIncluded = true;
                    reason = "configuration for drivemapping .:";
                }
                // check for specific drive
                if (Configuration.driveFilePatternsTolookfor.ContainsKey(mountPoint))
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
                    Configuration.drivesToSearch.Add(d.Name);
                }
                logger.Log($"GetDriveInfo: found mountpoint [{mountPoint}]\t included? [{driveToBeIncluded}]\t reason[{reason}]", logLevel.VERBOSE);
            }
        }
    }
}
