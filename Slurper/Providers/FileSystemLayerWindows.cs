using System;
using System.IO;
using Slurper.Contracts;
using Slurper.Logic;

namespace Slurper.Providers
{
    public class FileSystemLayerWindows : IFileSystemLayer
    {
        public ILogger Logger { get; } = LogProvider.Logger;

        public string TargetDirBasePath { get; set; } // relative directory for file to be copied to

        public char PathSep { get; } = Path.DirectorySeparatorChar;

        public void CreateTargetLocation()
        {
            var curDir = Directory.GetCurrentDirectory();
            var hostname = (Environment.MachineName).ToLower();
            var dateTime = $"{DateTime.Now:yyyyMMdd_HH-mm-ss}";

            TargetDirBasePath = string.Concat(curDir, PathSep, Configuration.RipDir, PathSep, hostname, "_", dateTime);
            Logger.Log($"CreateTargetLocation: [{hostname}][{curDir}][{dateTime}][{TargetDirBasePath}]",
                LogLevel.Verbose);

            try
            {
                if (!Configuration.DryRun)
                {
                    Directory.CreateDirectory(TargetDirBasePath);
                }
            }
            catch (Exception e)
            {
                Logger.Log($"CreateTargetLocation: failed to create director [{TargetDirBasePath}][{e.Message}]",
                    LogLevel.Error);
            }
        }

        public void GetMountedPartitionInfo()
        {
            var allDrives = DriveInfo.GetDrives();
            
            var myDrive = Path.GetPathRoot(Directory.GetCurrentDirectory());
            Logger.Log($"GetDriveInfo: myDrive = [{myDrive}]", LogLevel.Verbose);

            foreach (var d in allDrives)
            {
                // D:\  -> D:
                var driveId = d.Name.Substring(0, 2).ToUpper();

                // check if drive will be included
                var driveToBeIncluded = false;
                var reason = "configuration";

                // check for wildcard
                if (Configuration.DriveFilePatternsTolookfor.ContainsKey(".:"))
                {
                    driveToBeIncluded = true;
                    reason = "configuration for drive .:";
                }

                // check for specific drive
                if (Configuration.DriveFilePatternsTolookfor.ContainsKey(driveId))
                {
                    driveToBeIncluded = true;
                    reason = "configuration for drive " + driveId;
                }

                // skip the drive i'm running from
                if (myDrive != null && (myDrive.ToUpper()).Equals(d.Name.ToUpper()))
                {
                    driveToBeIncluded = false;
                    reason = "this the drive i'm running from";
                }

                // include this drive
                if (driveToBeIncluded)
                {
                    Configuration.DrivesToSearch.Add(d.Name);
                }

                Logger.Log(
                    $"GetDriveInfo: found drive [{driveId}]\t included? [{driveToBeIncluded}]\t reason[{reason}]",
                    LogLevel.Verbose);
            }
        }
    }
}