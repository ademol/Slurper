﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alphaleonis.Win32.Filesystem;
using Slurper.Providers;

namespace Slurper.Logic
{
    class Fileripper
    {
        static readonly ILogger logger = new LogProvider().GetLog();

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