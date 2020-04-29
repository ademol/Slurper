using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using Slurper.Contracts;
using Slurper.Output;
using Slurper.Providers;

namespace Slurper.Logic
{
    public class Searcher
    {
        private static readonly ILogger Logger = LogProvider.Logger;
        private readonly ArrayList _patterns;

        private Searcher(string startPath)
        {
            _patterns = GetPattern(startPath);
        }

        public static void SearchAndCopyFiles()
        {
            // process each drive
            foreach (string path in Configuration.PathList)
            {
                var searcher = new Searcher(path);
                searcher.DirSearch(path);
            }
        }

        private static ArrayList GetPattern(string path)
        {
            //Patterns
            // make sure to only use the patterns for the path requested
            var patterns = new ArrayList();


            // add patterns for specific path
            Configuration.PatternsToMatch.TryGetValue(path, out var v);
            if (v != null)
            {
                patterns.AddRange(v);
            }
            
            // add patterns for any path
            Configuration.PatternsToMatch.TryGetValue(".", out v);
            if (v != null)
            {
                patterns.AddRange(v);
            }

            return patterns;
        }


        private void DirSearch(string path)
        {
            // long live the 'null-coalescing' operator ?? to handle cases of 'null'  :)
            foreach (var d in GetDirs(path) ?? new String[0])
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
                    foreach (String p in _patterns)
                    {
                        if ((new Regex(p).Match(f)).Success)
                        {
                            FileRipper.RipFile(f);
                            break;
                        }
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


        private static string[] GetFiles(string path)
        {
            try
            {
                var files = Directory.GetFiles(path, "*.*");
                return files;
            }
            catch (UnauthorizedAccessException e)
            {
                Logger.Log($"getFiles: Unauthorized to retrieve fileList from [{path}][{e.Message}]", LogLevel.Error);
            }
            catch (Exception e)
            {
                Logger.Log($"getFiles: Failed to retrieve fileList from [{path}][{e.Message}]", LogLevel.Error);
            }

            return null;
        }

        private static string[] GetDirs(string path)
        {
            try
            {
                var dirs = Directory.GetDirectories(path);
                return dirs;
            }
            catch (UnauthorizedAccessException e)
            {
                Logger.Log($"getFiles: Unauthorized to retrieve dirList from [{path}][{e.Message}]", LogLevel.Error);
            }
            catch (Exception e)
            {
                Logger.Log($"getDirs: Failed to retrieve dirList from [{path}][{e.Message}]", LogLevel.Error);
            }

            return null;
        }
    }
}