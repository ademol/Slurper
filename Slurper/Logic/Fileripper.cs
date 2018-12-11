using System;
using System.IO;
using Slurper.Contracts;
using Slurper.Providers;

namespace Slurper.Logic
{
    class fileripper : IFileripper
    {
        static readonly ILogger logger = LogProvider.Logger;
        const string longPathPrefix = "\\\\?\\";

        public void RipFile(String soureFilePath)
        {
            String targetPath = BuildTargetPath(soureFilePath);
            String targetFilePath = targetPath + Path.GetFileName(soureFilePath);

            Spinner.RipSpin();
            logger.Log($"RipFile: ripping [{soureFilePath}] => [{targetFilePath}]", LogLevel.VERBOSE);

            if (Configuration.cmdLineFlagSet.Contains(CmdLineFlag.DRYRUN)) { return; }
            try
            {
                Directory.CreateDirectory(longPathPrefix + targetPath);
                File.Copy(longPathPrefix + soureFilePath, longPathPrefix + targetFilePath);
            }
            catch (Exception e)
            {
                logger.Log($"RipFile: copy of [{soureFilePath}] failed with [{e.Message}]", LogLevel.ERROR);
            }
        }

        private string BuildTargetPath(string filename)
        {
            String targetRelativePath = Path.GetDirectoryName(filename);
            targetRelativePath = SanitizePath(targetRelativePath);
            return SystemLayer.TargetDirBasePath + SystemLayer.PathSep + targetRelativePath + SystemLayer.PathSep;
        }

        private string SanitizePath(string path)
        {
            return path.Replace(':', '_');
        }
    }
}
