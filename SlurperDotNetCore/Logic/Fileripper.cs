using System;
using System.IO;
using SlurperDotNetCore.Contracts;
using SlurperDotNetCore.Providers;

namespace SlurperDotNetCore.Logic
{
    static class Fileripper
    {
        static readonly ILogger Logger = LogProvider.Logger;
    
        public static void RipFile(String filename)
        {
            String targetFileName = Path.GetFileName(filename); 
            String targetRelativePath = Path.GetDirectoryName(filename);

            targetRelativePath = targetRelativePath.Replace(':', '_');
            String targetPath = Program.FileSystemLayer.TargetDirBasePath 
            + Program.FileSystemLayer.PathSep 
            + targetRelativePath + Program.FileSystemLayer.PathSep;

            String targetFileNameFullPath = targetPath + targetFileName;

            Logger.Log($"RipFile: ripping [{filename}] => [{targetFileNameFullPath}]", LogLevel.Verbose);

            try
            {
                // do the filecopy unless this is a dryrun
                if (!Configuration.Dryrun)
                {
                    Directory.CreateDirectory(targetPath);
                    File.Copy(filename, targetFileNameFullPath);
                }
            }
            catch (Exception e)
            {
                Logger.Log($"RipFile: copy of [{filename}] failed with [{e.Message}]", LogLevel.Error);
            }
        }
    }
}
