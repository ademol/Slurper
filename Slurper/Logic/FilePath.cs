using Alphaleonis.Win32.Filesystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slurper.Logic
{
    public class FilePath
    {
        public static String targetDirBasePath { get; set; }                             // relative directory for file to be copied to

        public static char pathSep = Path.DirectorySeparatorChar;

    }
}
