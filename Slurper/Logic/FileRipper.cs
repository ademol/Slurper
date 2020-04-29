using System;
using System.IO;
using Slurper.Contracts;
using Slurper.Providers;

namespace Slurper.Logic
{
    internal static class FileRipper
    {
        private static readonly ILogger Logger = LogProvider.Logger;
    
        public static void RipFile(String filename)
        {
            var targetFileName = Path.GetFileName(filename); 
            var targetRelativePath = Path.GetDirectoryName(filename);

            targetRelativePath = targetRelativePath?.Replace(':', '_');
            var targetPath = Program.FileSystemLayer.TargetDirBasePath 
                             + Program.FileSystemLayer.PathSep 
                             + targetRelativePath + Program.FileSystemLayer.PathSep;

            var targetFileNameFullPath = targetPath + targetFileName;

            Logger.Log($"RipFile: ripping [{filename}] => [{targetFileNameFullPath}]", LogLevel.Verbose);

            try
            {
                if (Configuration.DryRun) return;
                Directory.CreateDirectory(targetPath);
                File.Copy(filename, targetFileNameFullPath);
            }
            catch (Exception e)
            {
                Logger.Log($"RipFile: copy of [{filename}] failed with [{e.Message}]", LogLevel.Error);
            }
        }
    }
}
