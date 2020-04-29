using System;
using System.IO;
using Slurper.Contracts;
using Slurper.Output;
using Slurper.Providers;

namespace Slurper.Logic
{
    class Fileripper : IFileripper
    {
        static readonly ILogger Logger = LogProvider.Logger;
        const string LongPathPrefix = "\\\\?\\";

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void RipFile(String soureFilePath)
        {
            String targetPath = BuildTargetPath(soureFilePath);
            String targetFilePath = targetPath + Path.GetFileName(soureFilePath);

            Spinner.RipSpin();
            Logger.Log($"RipFile: ripping [{soureFilePath}] => [{targetFilePath}]", LogLevel.Verbose);

            if (Configuration.CmdLineFlagSet.Contains(CmdLineFlag.Dryrun)) { return; }
            try
            {
                Directory.CreateDirectory(LongPathPrefix + targetPath);
                File.Copy(LongPathPrefix + soureFilePath, LongPathPrefix + targetFilePath);
            }
            catch (Exception e)
            {
                Logger.Log($"RipFile: copy of [{soureFilePath}] failed with [{e.Message}]", LogLevel.Error);
            }
        }

        private static string BuildTargetPath(string filename)
        {
            String targetRelativePath = Path.GetDirectoryName(filename);
            targetRelativePath = SanitizePath(targetRelativePath);
            return SystemLayer.TargetDirBasePath + SystemLayer.PathSep + targetRelativePath + SystemLayer.PathSep;
        }

        private static string SanitizePath(string path)
        {
            return path.Replace(':', '_');
        }
    }
}
