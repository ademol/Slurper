﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Alphaleonis.Win32.Filesystem;

using Slurper.Providers;

namespace Slurper.Logic
{
    public class Searcher
    {
        static readonly ILogger logger = LogProvider.Logger;

        public static void SearchAndCopyFiles()
        {
            int maxParallel = Configuration.PARALLEL ?  -1  : 1;
            Parallel.ForEach(Configuration.drivesToSearch, (new ParallelOptions { MaxDegreeOfParallelism = maxParallel }),(currentDrive) =>
            {
                new Searcher().DirSearch(currentDrive);
            } );
        }

        public void DirSearch(string sDir)
        {

            //driveFilePatternsTolookfor
            // make sure to only use the patterns for the drives requested
            List<string> thisDrivePatternsToLookFor = new List<string>();
            // drive to search
            String curDrive = sDir.Substring(0, 2);    // aka c:  

            // add patterns for specific drive
            List<string> v;
            Configuration.driveFilePatternsTolookfor.TryGetValue(curDrive.ToUpper(), out v);
            if (v != null) { thisDrivePatternsToLookFor.AddRange(v); }

            // add patterns for all drives
            Configuration.driveFilePatternsTolookfor.TryGetValue(".:", out v);
            if (v != null) { thisDrivePatternsToLookFor.AddRange(v); }

            // long live the 'null-coalescing' operator ?? to handle cases of 'null'  :)
            foreach (string d in getDirs(sDir) ?? new String[0])
            {
                foreach (string f in getFiles(d) ?? new String[0])
                {
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
