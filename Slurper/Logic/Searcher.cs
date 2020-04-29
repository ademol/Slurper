using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using Slurper.Contracts;
using Slurper.Output;
using Slurper.Providers;

namespace Slurper.Logic
{
    static class Searcher
    {
        static readonly ILogger Logger = LogProvider.Logger;

        public static void SearchAndCopyFiles()
        {
            // process each drive
            foreach (String drive in Configuration.DrivesToSearch)
            {
                DirSearch(drive);
            }
        }

        private static void DirSearch(string sDir)
        {
            //driveFilePatternsTolookfor
            // make sure to only use the patterns for the drives requested
            var thisDrivePatternsToLookFor = new ArrayList();
            // drive to search
            // String curDrive = sDir.Substring(0, 2);    // aka c: 

            var rx = new Regex(@"^([^:]+:)");
            var curDrive = rx.Matches(sDir)[0].Value;


            if (curDrive.Length == 1) { curDrive = curDrive.ToUpper(); }

            // add patterns for specific drive
            ArrayList v;
            Configuration.DriveFilePatternsTolookfor.TryGetValue(curDrive, out v);
            if (v != null) { thisDrivePatternsToLookFor.AddRange(v); }

            // add patterns for all drives
            Configuration.DriveFilePatternsTolookfor.TryGetValue(".:", out v);
            if (v != null) { thisDrivePatternsToLookFor.AddRange(v); }

            // long live the 'null-coalescing' operator ?? to handle cases of 'null'  :)
            foreach (var d in GetDirs(sDir) ?? new String[0])
            {
                if (IsSymbolic(d))
                {
                    continue;
                }

                foreach (var f in GetFiles(d) ?? new String[0])
                {
                    if (IsSymbolic(f))
                    {
                        continue;
                    }

                    Spinner.Spin();
                    Logger.Log($"[{f}]", LogLevel.Trace);

                    // check if file is wanted by any of the specified patterns
                    foreach (String p in thisDrivePatternsToLookFor)
                    {
                        if ((new Regex(p).Match(f)).Success) { FileRipper.RipFile(f); break; }
                    }
                }

                try
                {
                    DirSearch(d);
                }
                catch (Exception e)
                {
                    Logger.Log($"DirSearch: Could not read dir [{d}][{e.Message}]", LogLevel.Error);
                }
            }
        }


        private static bool IsSymbolic(string pathName)
        {
            return File.GetAttributes(pathName).HasFlag(FileAttributes.ReparsePoint);
        }


        private static string[] GetFiles(string dir)
        {
            try
            {
                var files = Directory.GetFiles(dir, "*.*");
                return files;
            }
            catch (UnauthorizedAccessException e)
            {
                Logger.Log($"getFiles: Unauthorized to retrieve fileList from [{dir}][{e.Message}]", LogLevel.Error);
            }
            catch (Exception e)
            {
                Logger.Log($"getFiles: Failed to retrieve fileList from [{dir}][{e.Message}]", LogLevel.Error);
            }

            return null;
        }

        private static string[] GetDirs(string sDir)
        {
            try
            {
                var dirs = Directory.GetDirectories(sDir);
                return dirs;
            }
            catch (UnauthorizedAccessException e)
            {
                Logger.Log($"getFiles: Unauthorized to retrieve dirList from [{sDir}][{e.Message}]", LogLevel.Error);
            }
            catch (Exception e)
            {
                Logger.Log($"getDirs: Failed to retrieve dirList from [{sDir}][{e.Message}]", LogLevel.Error);
            }

            return null;
        }
    }
}
