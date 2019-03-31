using System;
using System.IO;
using Slurper.Contracts;
using Slurper.Logic;

namespace Slurper.Providers
{
    public static class SystemLayer
    {
        static readonly ILogger Logger = LogProvider.Logger;

        public static String TargetDirBasePath { get; set; }          

        public static char PathSep { get; }  = Path.DirectorySeparatorChar;

        public static void CreateTargetLocation()
        {
            TargetDirBasePath = BuildTargetBasePath();
            Logger.Log($"CreateTargetLocation: [{TargetDirBasePath}]", LogLevel.Verbose);

            if (Configuration.CmdLineFlagSet.Contains(CmdLineFlag.Dryrun)) { return; }
            try
            {
                Directory.CreateDirectory(TargetDirBasePath);
            }
                catch (Exception e)
            {
                Logger.Log($"CreateTargetLocation: failed to create director [{TargetDirBasePath}][{e.Message}]", LogLevel.Error);
            }
        }

        private static string BuildTargetBasePath()
        {
            String curDir = Directory.GetCurrentDirectory();
            String hostname = (Environment.MachineName).ToLower();
            String dateTime = String.Format("{0:yyyyMMdd_HH-mm-ss}", DateTime.Now);

            return string.Concat(curDir, PathSep, Configuration.RipDir, PathSep, hostname, "_", dateTime);
        }

        public static void GetDriveInfo()
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();

            // mydrive
            String mydrive = Path.GetPathRoot(Directory.GetCurrentDirectory());
            Logger.Log($"GetDriveInfo: mydrive = [{mydrive}]", LogLevel.Verbose);

            foreach (DriveInfo d in allDrives)
            {
            
                // D:\  -> D:
                String driveIdentifier = d.Name.Substring(0, 2).ToUpper();

                // check if drive will be included
                Boolean driveToBeIncluded = false;
                String reason = "configuration";

                // check for wildcard
                if (Configuration.DriveFileSearchPatterns.ContainsKey(".:"))
                {
                    driveToBeIncluded = true;
                    reason = "configuration for drive .:";
                }
                // check for specific drive
                if (Configuration.DriveFileSearchPatterns.ContainsKey(driveIdentifier))
                {
                    driveToBeIncluded = true;
                    reason = "configuration for drive " + driveIdentifier;
                }
                // skip the drive i'm running from
                if ((mydrive.ToUpper()).Equals(d.Name.ToUpper()) && ! Configuration.CmdLineFlagSet.Contains(CmdLineFlag.Includemydrive))
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
                    Configuration.DrivesToSearch.Add(d.Name);
                }
                Logger.Log($"GetDriveInfo: found drive [{driveIdentifier}]\t included? [{driveToBeIncluded}]\t reason[{reason}]", LogLevel.Verbose);
            }
        }
    }
}
