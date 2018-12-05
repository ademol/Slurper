using System;
using System.IO;

namespace Slurper.Providers
{
    public static class FileSystemLayer
    {
        static readonly ILogger logger = LogProvider.Logger;

        public static String TargetDirBasePath { get; set; }          

        public static char PathSep { get; }  = Path.DirectorySeparatorChar;

        public static void CreateTargetLocation()
        {
            TargetDirBasePath = BuildTargetBasePath();
            logger.Log($"CreateTargetLocation: [{TargetDirBasePath}]", LogLevel.VERBOSE);

            if (Configuration.DRYRUN) { return; }
            try
            {
                Directory.CreateDirectory(TargetDirBasePath);
            }
                catch (Exception e)
            {
                logger.Log($"CreateTargetLocation: failed to create director [{TargetDirBasePath}][{e.Message}]", LogLevel.ERROR);
            }
        }

        private static string BuildTargetBasePath()
        {
            String curDir = Directory.GetCurrentDirectory();
            String hostname = (System.Environment.MachineName).ToLower();
            String dateTime = String.Format("{0:yyyyMMdd_HH-mm-ss}", DateTime.Now);

            return string.Concat(curDir, PathSep, Configuration.ripDir, PathSep, hostname, "_", dateTime);
        }

        public static void GetDriveInfo()
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();

            // mydrive
            String mydrive = Path.GetPathRoot(Directory.GetCurrentDirectory());
            logger.Log($"GetDriveInfo: mydrive = [{mydrive}]", LogLevel.VERBOSE);

            foreach (DriveInfo d in allDrives)
            {
            
                // D:\  -> D:
                String driveIdentifier = d.Name.Substring(0, 2).ToUpper();

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
                if (Configuration.driveFilePatternsTolookfor.ContainsKey(driveIdentifier))
                {
                    driveToBeIncluded = true;
                    reason = "configuration for drive " + driveIdentifier;
                }
                // skip the drive i'm running from
                if ((mydrive.ToUpper()).Equals(d.Name.ToUpper()) && ! Configuration.INCLUDEMYDRIVE)
                {
                    driveToBeIncluded = false;
                    reason = "this the drive i'm running from";
                }
                // skip cdrom
                if (d.DriveType == DriveType.CDRom)
                {
                    driveToBeIncluded = false;
                    reason = "this is a CD/DVDrom drive";
                }
                // include this drive
                if (driveToBeIncluded)
                {
                    Configuration.drivesToSearch.Add(d.Name);
                }
                logger.Log($"GetDriveInfo: found drive [{driveIdentifier}]\t included? [{driveToBeIncluded}]\t reason[{reason}]", LogLevel.VERBOSE);
            }
        }
    }
}
