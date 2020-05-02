using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Slurper.Contracts;
using Slurper.Output;
using Slurper.Providers;

namespace Slurper.Logic
{
    public class Searcher
    {
        private static readonly ILogger Logger = LogProvider.Logger;
        private readonly string _curPath = Directory.GetCurrentDirectory();
        private readonly List<string> _patterns = ConfigurationService.PatternsToMatch;

        public static void SearchAndCopyFiles()
        {
            foreach (var path in ConfigurationService.PathList)
            {
                var searcher = new Searcher();
                searcher.DirSearch(path);
            }
        }

        private void DirSearch(string path)
        {
            foreach (var d in GetDirs(path) ?? new string[0])
            {
                if (SkipDirectory(d)) continue;

                GetFilesInCurrentDirectory(d);

                GetSubDirectories(d);
            }
        }

        private bool SkipDirectory(string d)
        {
            if (IsSymbolic(d))
            {
                Logger.Log($"Skip symbolic link [{d}]", LogLevel.Trace);
                return true;
            }

            if (IsCurrentPath(d))
            {    
                Logger.Log($"Skipping my path[{d}]", LogLevel.Trace);
                return true;
            }

            return false;
        }

        private void GetSubDirectories(string d)
        {
            try
            {
                DirSearch(d);
            }
            catch (Exception e)
            {
                Logger.Log($"DirSearch: Could not read dir [{d}][{e.Message}]", LogLevel.Error);
            }
        }

        private void GetFilesInCurrentDirectory(string d)
        {
            foreach (var f in GetFiles(d) ?? new string[0])
            {
                if (IsSymbolic(f)) continue;

                Spinner.Spin();
                Logger.Log($"[{f}]", LogLevel.Trace);

                if (_patterns.Any(p => new Regex(p).Match(f).Success)) FileRipper.RipFile(f);
            }
        }

        private bool IsCurrentPath(string path)
        {
            return path.Equals(_curPath);
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
                return Directory.GetDirectories(path);
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