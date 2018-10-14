using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using SlurperDotNetCore.Providers;

namespace SlurperDotNetCore.Logic
{
    static class Searcher
    {
        static readonly ILogger logger = LogProvider.Logger;

        public static void SearchAndCopyFiles()
        {
            // process each drive
            foreach (String drive in Configuration.drivesToSearch)
            {
                DirSearch(drive);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static void DirSearch(string sDir)
        {

            //driveFilePatternsTolookfor
            // make sure to only use the patterns for the drives requested
            ArrayList thisDrivePatternsToLookFor = new ArrayList();
            // drive to search
            // String curDrive = sDir.Substring(0, 2);    // aka c: 

            Regex rx = new Regex(@"^([^:]+)");
            string curDrive = rx.Matches(sDir)[0].Value.ToString();


            if (curDrive.Length == 1) { curDrive = curDrive.ToUpper(); }

            // add patterns for specific drive
            ArrayList v;
            Configuration.driveFilePatternsTolookfor.TryGetValue(curDrive, out v);
            if (v != null) { thisDrivePatternsToLookFor.AddRange(v); }

            // add patterns for all drives
            Configuration.driveFilePatternsTolookfor.TryGetValue(".:", out v);
            if (v != null) { thisDrivePatternsToLookFor.AddRange(v); }

            // long live the 'null-coalescing' operator ?? to handle cases of 'null'  :)
            foreach (string d in getDirs(sDir) ?? new String[0])
            {
                if (IsSymbolic(d))
                {
                    continue;
                }
                foreach (string f in getFiles(d) ?? new String[0])
                {
                    if (IsSymbolic(f))
                    {
                        continue;
                    }
                    Spinner.Spin();
                    logger.Log($"[{f}]", logLevel.TRACE);

                    // check if file is wanted by any of the specified patterns
                    foreach (String p in thisDrivePatternsToLookFor)
                    {
                        if ((new Regex(p).Match(f)).Success) { Fileripper.RipFile(f); break; }
                    }
                }
                try
                {
                    DirSearch(d);
                }
                catch (Exception e)
                {
                    logger.Log($"DirSearch: Could not read dir [{d}][{e.Message}]", logLevel.ERROR);
                }
            }
        }



        private static bool IsSymbolic(string pathName)
        {
            return System.IO.File.GetAttributes(pathName).HasFlag(FileAttributes.ReparsePoint);
        }



        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        static String[] getFiles(string dir)
        {
            try
            {
                String[] files = Directory.GetFiles(dir, "*.*");
                return files;
            }
            catch (UnauthorizedAccessException e)
            {
                logger.Log($"getFiles: Unauthorized to retrieve fileList from [{dir}][{e.Message}]", logLevel.ERROR);
            }
            catch (Exception e)
            {
                logger.Log($"getFiles: Failed to retrieve fileList from [{dir}][{e.Message}]", logLevel.ERROR);
            }
            return null;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        static String[] getDirs(string sDir)
        {
            try
            {
                string[] dirs = Directory.GetDirectories(sDir);
                return dirs;
            }
            catch (UnauthorizedAccessException e)
            {
                logger.Log($"getFiles: Unauthorized to retrieve dirList from [{sDir}][{e.Message}]", logLevel.ERROR);
            }
            catch (Exception e)
            {
                logger.Log($"getDirs: Failed to retrieve dirList from [{sDir}][{e.Message}]", logLevel.ERROR);
            }
            return null;
        }
    }
}
