using System;
using System.IO;
using Slurper.Contracts;
using Slurper.Logic;

namespace Slurper.Providers
{
    public class FileSystemLayerWindows : IFileSystemLayer
    {
        private ILogger Logger { get; } = LogProvider.Logger;

        public string TargetDirBasePath { get; private set; } // relative directory for file to be copied to

        public char PathSep { get; } = Path.DirectorySeparatorChar;

        public void CreateTargetLocation()
        {
            var curDir = Directory.GetCurrentDirectory();
            var hostname = Environment.MachineName.ToLower();
            var dateTime = $"{DateTime.Now:yyyyMMdd_HH-mm-ss}";

            TargetDirBasePath = string.Concat(curDir, PathSep, ConfigurationService.DestinationDirectory, PathSep,
                hostname, "_", dateTime);
            Logger.Log($"CreateTargetLocation: [{hostname}][{curDir}][{dateTime}][{TargetDirBasePath}]",
                LogLevel.Verbose);

            try
            {
                if (!ConfigurationService.DryRun) Directory.CreateDirectory(TargetDirBasePath);
            }
            catch (Exception e)
            {
                Logger.Log($"CreateTargetLocation: failed to create director [{TargetDirBasePath}][{e.Message}]",
                    LogLevel.Error);
            }
        }

        public void SetSourcePaths()
        {
            var allDrives = DriveInfo.GetDrives();

            var myDrive = Path.GetPathRoot(Directory.GetCurrentDirectory());
            Logger.Log($"GetDriveInfo: myDrive = [{myDrive}]", LogLevel.Verbose);

            foreach (var d in allDrives)
            {
                if (d.Name.Equals(myDrive?.ToUpper()))
                {
                    Logger.Log($"GetDriveInfo: found drive [{d.Name}], but skipped i'm running from it", LogLevel.Verbose);
                    continue;
                }

                ConfigurationService.PathList.Add(d.Name);

                Logger.Log($"GetDriveInfo: found drive [{d.Name}]", LogLevel.Verbose);
            }
        }

        public string SanitizePath(string path)
        {
            return path.Replace(':', '_');
        }
    }
}
