using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Slurper.Contracts;
using Slurper.Output;
using Slurper.Providers;

namespace Slurper.Logic
{
    public class FileSearcher : IFileSearcher
    {
        static readonly ILogger Logger = LogProvider.Logger;
        static readonly IFileripper Fileripper = new Fileripper();

        static double _countFiles;
        static double _countAddedFileNames;
        static readonly BlockingCollection<string> BlockingCollection = new BlockingCollection<string>();
        static Stopwatch _sw;

        readonly List<string> _currentDriveSearchPatterns;

        public FileSearcher()
        {
            _currentDriveSearchPatterns = new List<string>();
        }

        public void DispatchDriveSearchers()
        {
            _sw = new Stopwatch();
            _sw.Start();
            Thread myThread;
            myThread = new Thread(
            BlockingCollectionFileRipper);
            myThread.Start();

            Parallel.ForEach(Configuration.DrivesToSearch, (new ParallelOptions { MaxDegreeOfParallelism = -1 }), currentDrive =>
             {
                 new FileSearcher().DriveSearch(currentDrive);
             });
            BlockingCollection.CompleteAdding();

            var searchingDoneTime = _sw.Elapsed;
            Logger.Log($"searching done:checked {_countFiles} with {_countAddedFileNames} matches in {searchingDoneTime}", LogLevel.Verbose);

            while (!BlockingCollection.IsCompleted)
            { }
            _sw.Stop();
            var copyDoneTime = _sw.Elapsed;
            Logger.Log($"done:checked {_countFiles} with {_countAddedFileNames} matches.  " +
                $"SearchingTime [{searchingDoneTime}]   Searching+CopyTime [{copyDoneTime}]", LogLevel.Verbose);
        }

        public static void BlockingCollectionFileRipper()
        {
            foreach (var fileName in BlockingCollection.GetConsumingEnumerable())
            {
                Fileripper.RipFile(fileName);
            }
        }

        public void DriveSearch(string currentDrive)
        {
            SetSearchPatternsForDrive(currentDrive);
            DirSearch(currentDrive);
        }

        public void SetSearchPatternsForDrive(string driveInfoName)
        {
            String currentDriveIdentifier = driveInfoName.Substring(0, 2); 

            Configuration.DriveFileSearchPatterns.TryGetValue(currentDriveIdentifier.ToUpper(), out List<string> patternsForSpecificDrive);
            if (patternsForSpecificDrive?.Count > 0) { _currentDriveSearchPatterns.AddRange(patternsForSpecificDrive); }

            // include patterns for "all" drives
            Configuration.DriveFileSearchPatterns.TryGetValue(".:", out List<string> patternsForAllDrives);
            if (patternsForAllDrives?.Count > 0) { _currentDriveSearchPatterns.AddRange(patternsForAllDrives); }
        }

        public void DirSearch(string sDir)
        {
            // long live the 'null-coalescing' operator ?? to handle cases of 'null'  :)
            foreach (string fileName in GetFiles(sDir) ?? new String[0])
            {
                Spinner.SearchSpin();
                _countFiles++;

                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if ((_countAddedFileNames + 1) % 100 == 0)
                    Logger.Log($"search busy:checked {_countFiles} with {_countAddedFileNames} matches in {_sw.Elapsed}", LogLevel.Verbose);

                Logger.Log($"[{fileName}]", LogLevel.Trace);

                if (MatchFileAgainstSearchPatterns(fileName))
                {
                    BlockingCollection.Add(fileName);
                    _countAddedFileNames++;
                }
            }
            foreach (string dirEntry in GetDirs(sDir) ?? new String[0])
            {
                DirSearch(dirEntry);
            }
        }

        public bool MatchFileAgainstSearchPatterns(string fileName)
        {
            // check if file is wanted by any of the specified patterns
            foreach (String pattern in _currentDriveSearchPatterns)
            {
                if ((new Regex(pattern, RegexOptions.IgnoreCase).Match(Path.GetFullPath(fileName))).Success) { return true; }
            }
            return false;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static String[] GetFiles(string currentDirectory)
        {
            string[] filesystemEntries;
            try
            {
                filesystemEntries = Directory.GetFiles(currentDirectory);
            }
            catch (UnauthorizedAccessException e)
            {
                Logger.Log($"getFiles: Unauthorized to retrieve file entries from [{currentDirectory}][{e.Message}]", LogLevel.Error);
                filesystemEntries = null;
            }
            catch (Exception e)
            {
                Logger.Log($"getFiles: Failed to retrieve file entries from [{currentDirectory}][{e.Message}]", LogLevel.Error);
                filesystemEntries = null;
            }
            return filesystemEntries;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static String[] GetDirs(string currentDirectory)
        {
            string[] filesystemEntries;
            try
            {
                filesystemEntries = Directory.GetDirectories(currentDirectory);
            }
            catch (UnauthorizedAccessException e)
            {
                Logger.Log($"getDirs: Unauthorized to retrieve (sub)directories from [{currentDirectory}][{e.Message}]", LogLevel.Error);
                filesystemEntries = null;
            }
            catch (Exception e)
            {
                Logger.Log($"getDirs: Failed to retrieve (sub)directories from [{currentDirectory}][{e.Message}]", LogLevel.Error);
                filesystemEntries = null;
            }
            return filesystemEntries;
        }
    }
}
