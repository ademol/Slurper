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

            var tasks = ConfigurationService.PathList.Select(path => Task.Run( () => DirSearch(path))).ToList();
            Console.WriteLine($" Task count =  {tasks.Count}");
            await Task.WhenAll(tasks.ToArray());
            Console.WriteLine($"done in {stopWatch.Elapsed}");
            stopWatch.Stop();
        }

        private void DirSearch(string path)
        {
            foreach (var directory in GetDirs(path))
            {
                if (SkipDirectory(directory)) continue;

                GetFilesInCurrentDirectory(directory);

                GetSubDirectories(directory);
            }
        }

        private bool SkipDirectory(string directory)
        {
            if (IsSymbolic(directory))
            {
                _logger.LogTrace("Skip symbolic link [{Directory}]", directory);
                return true;
            }

            if (IsCurrentPath(directory))
            {
                _logger.LogTrace("Skipping my path[{Directory}]", directory);
                return true;
            }

            return false;
        }

        private void GetSubDirectories(string directory)
        {
            try
            {
                DirSearch(directory);
            }
            catch (Exception e)
            {
                _logger.LogError("DirSearch: Could not read dir [{Directory}][{Exception}]", directory, e.Message);
            }
        }

        private void GetFilesInCurrentDirectory(string directory)
        {
            var tasks = new List<Task>();

            foreach (var f in GetFiles(directory))
            {
                if (IsSymbolic(f)) continue;

                Spinner.Spin();
                _logger.LogTrace("[{F}]", f);

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
                _logger.LogError("getFiles: Unauthorized to retrieve fileList from [{Path}][{ExceptionMessage}]", path, e.Message);
            }
            catch (Exception e)
            {
                _logger.LogError("getFiles: Failed to retrieve fileList from [{Path}][{ExceptionMessage}]", path, e.Message);
            }

            return Array.Empty<string>();
        }

        private IEnumerable<string> GetDirs(string path)
        {
            try
            {
                return Directory.GetDirectories(path);
            }
            catch (UnauthorizedAccessException e)
            {
                _logger.LogError("getFiles: Unauthorized to retrieve dirList from [{Path}][{ExceptionMessage}]", path, e.Message);
            }
            catch (Exception e)
            {
                _logger.LogError("getDirs: Failed to retrieve dirList from [{Path}][{ExceptionMessage}]", path, e.Message);
            }

            return Array.Empty<string>();
        }
    }
}

