using System;
using System.IO;
using Slurper.Contracts;
using Slurper.Providers;

namespace Slurper.Logic
{
    internal static class FileRipper
    {
        private static readonly ILogger Logger = LogProvider.Logger;

        public static void RipFile(string filename)
        {
            var targetPath = TargetPath(filename);
            var targetFileNameFullPath = targetPath + Path.GetFileName(filename);

            Logger.Log($"RipFile: ripping [{filename}] => [{targetFileNameFullPath}]", LogLevel.Verbose);

            try
            {
                if (ConfigurationService.DryRun) return;
                Directory.CreateDirectory(targetPath);
                File.Copy(filename, targetFileNameFullPath);
            }
            catch (Exception e)
            {
                Logger.Log($"RipFile: copy of [{filename}] failed with [{e.Message}]", LogLevel.Error);
            }
        }

        private static string TargetPath(string filename)
        {
            var targetRelativePath = Path.GetDirectoryName(filename);
            targetRelativePath = Program.FileSystemLayer.SanitizePath(targetRelativePath);

            var sep = Program.FileSystemLayer.PathSep;
            var targetDirBasePath = Program.FileSystemLayer.TargetDirBasePath;

            return $"{targetDirBasePath}{sep}{targetRelativePath}{sep}";
        }
    }
}