using System;
using System.IO;

using Slurper.Providers;

namespace Slurper.Logic
{
    class Fileripper
    {
        static readonly ILogger logger = LogProvider.Logger;
        const string longPathPrefix = "\\\\?\\";

        public void RipFile(String soureFilePath)
        {
            String targetPath = BuildTargetPath(soureFilePath);
            String targetFilePath = targetPath + Path.GetFileName(soureFilePath);

            logger.Log($"RipFile: ripping [{soureFilePath}] => [{targetFilePath}]", logLevel.VERBOSE);

            if (Configuration.DRYRUN) { return; }
            try
            {
                Directory.CreateDirectory(longPathPrefix + targetPath);
                File.Copy(longPathPrefix + soureFilePath, longPathPrefix + targetFilePath);
            }
            catch (Exception e)
            {
                logger.Log($"RipFile: copy of [{soureFilePath}] failed with [{e.Message}]", logLevel.ERROR);
            }
        }

        private string BuildTargetPath(string filename)
        {
            String targetRelativePath = Path.GetDirectoryName(filename);
            targetRelativePath = targetRelativePath.Replace(':', '_');
            return FileSystemLayer.targetDirBasePath + FileSystemLayer.pathSep + targetRelativePath + FileSystemLayer.pathSep;
        }

    }
}
