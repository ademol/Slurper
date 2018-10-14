using System;
using System.IO;
using SlurperDotNetCore.Providers;

namespace SlurperDotNetCore.Logic
{
    static class Fileripper
    {
        static readonly ILogger logger = LogProvider.Logger;
    
        public static void RipFile(String filename)
        {
            String targetFileName = Path.GetFileName(filename); 
            String targetRelativePath = Path.GetDirectoryName(filename);

            targetRelativePath = targetRelativePath.Replace(':', '_');
            String targetPath = SlurperDotNetCore.Program.fileSystemLayer.targetDirBasePath 
            + SlurperDotNetCore.Program.fileSystemLayer.pathSep 
            + targetRelativePath + SlurperDotNetCore.Program.fileSystemLayer.pathSep;

            String targetFileNameFullPath = targetPath + targetFileName;

            logger.Log($"RipFile: ripping [{filename}] => [{targetFileNameFullPath}]", logLevel.VERBOSE);

            try
            {
                // do the filecopy unless this is a dryrun
                if (!Configuration.DRYRUN)
                {
                    Directory.CreateDirectory(targetPath);
                    File.Copy(filename, targetFileNameFullPath);
                }
            }
            catch (Exception e)
            {
                logger.Log($"RipFile: copy of [{filename}] failed with [{e.Message}]", logLevel.ERROR);
            }
        }
    }
}
