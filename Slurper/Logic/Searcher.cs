using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Slurper.Output;

namespace Slurper.Logic
{
    public class Searcher
    {
        private readonly ILogger<Searcher> _logger;
        private readonly string _curPath = Directory.GetCurrentDirectory();
        private readonly List<string> _patterns = ConfigurationService.PatternsToMatch;
        private readonly FileRipper _fileRipper;

        public Searcher(ILogger<Searcher> logger, FileRipper fileRipper)
        {
            _logger = logger;
            _fileRipper = fileRipper;
        }

        public async Task SearchAndCopyFiles()
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            //ConfigurationService.PathList.ForEach( path => tasks.Add(DirSearchAsync(path)));
            var tasks = ConfigurationService.PathList.Select(path => Task.Run( () => DirSearch(path))).ToList();

            await Task.WhenAll(tasks.ToArray());
            Console.WriteLine($"done in {stopWatch.Elapsed}");
            stopWatch.Stop();
        }

        private void DirSearch(string path)
        {
            foreach (var d in GetDirs(path))
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
                _logger.LogTrace($"Skip symbolic link [{d}]");
                return true;
            }

            if (IsCurrentPath(d))
            {
                _logger.LogTrace($"Skipping my path[{d}]");
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
                _logger.LogError($"DirSearch: Could not read dir [{d}][{e.Message}]");
            }
        }

        private void GetFilesInCurrentDirectory(string d)
        {
            var tasks = new List<Task>();

            foreach (var f in GetFiles(d))
            {
                if (IsSymbolic(f)) continue;

                Spinner.Spin();
                _logger.LogTrace($"[{f}]");

                if (_patterns.Any(p => new Regex(p).Match(f).Success))
                {
                    tasks.Add(_fileRipper.RipFile(f));
                }
            }

            Task.WaitAll(tasks.ToArray());
        }

        private bool IsCurrentPath(string path)
        {
            return path.Equals(_curPath);
        }


        private static bool IsSymbolic(string pathName)
        {
            return File.GetAttributes(pathName).HasFlag(FileAttributes.ReparsePoint);
        }


        private IEnumerable<string> GetFiles(string path)
        {
            try
            {
                var files = Directory.GetFiles(path, "*.*");
                return files;
            }
            catch (UnauthorizedAccessException e)
            {
                _logger.LogError($"getFiles: Unauthorized to retrieve fileList from [{path}][{e.Message}]");
            }
            catch (Exception e)
            {
                _logger.LogError($"getFiles: Failed to retrieve fileList from [{path}][{e.Message}]");
            }

            return Array.Empty<string>();
        }

        private string[] GetDirs(string path)
        {
            try
            {
                return Directory.GetDirectories(path);
            }
            catch (UnauthorizedAccessException e)
            {
                _logger.LogError($"getFiles: Unauthorized to retrieve dirList from [{path}][{e.Message}]");
            }
            catch (Exception e)
            {
                _logger.LogError($"getDirs: Failed to retrieve dirList from [{path}][{e.Message}]");
            }

            return Array.Empty<string>();
        }
    }
}

