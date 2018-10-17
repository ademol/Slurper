using System;
using System.IO;

using SlurperDotNetCore.Contracts;

namespace SlurperDotNetCore.Providers
{
    public class FileSystemLayerWindows : IFileSystemLayer 
    {
        public ILogger logger { get; } = LogProvider.Logger;

        public String targetDirBasePath { get; set; }                             // relative directory for file to be copied to

        public  char pathSep { get; }  = Path.DirectorySeparatorChar;

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

        public void GetMountedPartitionInfo()
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();

            // mydrive
            String mydrive = Path.GetPathRoot(Directory.GetCurrentDirectory());
            logger.Log($"GetDriveInfo: mydrive = [{mydrive}]", logLevel.VERBOSE);

            foreach (DriveInfo d in allDrives)
            {
                // D:\  -> D:
                String driveID = d.Name.Substring(0, 2).ToUpper();

                // check if drive will be included
                Boolean driveToBeIncluded = false;
                String reason = "configuration";

                // check for wildcard
                if (Configuration.driveFilePatternsTolookfor.ContainsKey(".:"))
                {
                    driveToBeIncluded = true;
                    reason = "configuration for drive .:";
                }
                // check for specific drive
                if (Configuration.driveFilePatternsTolookfor.ContainsKey(driveID))
                {
                    driveToBeIncluded = true;
                    reason = "configuration for drive " + driveID;
                }
                // skip the drive i'm running from
                if ((mydrive.ToUpper()).Equals(d.Name.ToUpper()))
                {
                    driveToBeIncluded = false;
                    reason = "this the drive i'm running from";
                }

                // include this drive
                if (driveToBeIncluded)
                {
                    Configuration.drivesToSearch.Add(d.Name);
                }
                logger.Log($"GetDriveInfo: found drive [{driveID}]\t included? [{driveToBeIncluded}]\t reason[{reason}]", logLevel.VERBOSE);
            }
        }
    }
}
