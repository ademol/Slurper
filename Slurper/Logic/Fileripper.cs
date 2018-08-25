using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alphaleonis.Win32.Filesystem;

namespace Slurper.Logic
{
    class Fileripper
    {
        private static ArrayList filesRipped = new ArrayList();                 // files grabbed, to prevent multiple copies (in case of multiple matching patterns)

        public static void RipFile(String filename)
        {        
 
            // process if not already downloaded  (in case of multiple regex matches)
            if (! filesRipped.Contains(filename))
            {
                filesRipped.Add(filename);

                // determine target filename
                String targetFileName = Path.GetFileName(filename);
                String targetRelativePath = Path.GetDirectoryName(filename);


                targetRelativePath = targetRelativePath.Replace(':', '_');
                String targetPath = FilePath.targetDirBasePath + FilePath.pathSep + targetRelativePath + FilePath.pathSep;


                String targetFileNameFullPath = targetPath + targetFileName;

                if (Configuration.VERBOSE) { Console.WriteLine("RipFile: ripping [{0}] => [{1}]", filename, targetFileNameFullPath); }
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
                    Console.WriteLine("RipFile: copy of [{0}] failed with [{1}]", filename, e.Message);
                }
            }
        }


    }
}
