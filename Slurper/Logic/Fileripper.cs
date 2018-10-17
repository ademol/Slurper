using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alphaleonis.Win32.Filesystem;
using Slurper.Providers;

namespace Slurper.Logic
{
    static class Fileripper
    {
        static readonly ILogger logger = LogProvider.Logger;

          public static void RipFile(String filename)
        {
            String targetFileName = Path.GetFileName(filename);
            String targetRelativePath = Path.GetDirectoryName(filename);

            targetRelativePath = targetRelativePath.Replace(':', '_');
            String targetPath = FileSystemLayer.targetDirBasePath + FileSystemLayer.pathSep + targetRelativePath + FileSystemLayer.pathSep;
            String targetFileNameFullPath = targetPath + targetFileName;

            logger.Log($"RipFile: ripping [{filename}] => [{targetFileNameFullPath}]", logLevel.VERBOSE);

            try
            {
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
