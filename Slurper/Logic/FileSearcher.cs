using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Slurper.Providers;

namespace Slurper.Logic
{
    public class FileSearcher
    {
        static readonly ILogger logger = LogProvider.Logger;
        static double countFiles = 0;
        static double countAddedFileNames = 0;
        static BlockingCollection<string> blockingCollection = new BlockingCollection<string>();
        static Stopwatch sw;

        List<string> currentDriveSearchPatterns;

        public FileSearcher()
        {
            currentDriveSearchPatterns = new List<string>();
        }

        public static void DispatchDriveSearchers()
        {
            sw = new Stopwatch();
            sw.Start();
            System.Threading.Thread myThread;
            myThread = new System.Threading.Thread(
            new System.Threading.ThreadStart(BlockingCollectionFileRipper));
            myThread.Start();

            Parallel.ForEach(Configuration.DrivesToSearch, (new ParallelOptions { MaxDegreeOfParallelism = -1 }), (currentDrive) =>
             {
                 new FileSearcher().DriveSearch(currentDrive);
             });
            blockingCollection.CompleteAdding();
            logger.Log($"searching done:checked {countFiles} with {countAddedFileNames} matches in {sw.Elapsed}", LogLevel.VERBOSE);

            while (!blockingCollection.IsCompleted)
            { }
            sw.Stop();
            logger.Log($"copying done:checked {countFiles} with {countAddedFileNames} matches in {sw.Elapsed}", LogLevel.VERBOSE);

        }

        public static void BlockingCollectionFileRipper()
        {
            foreach (var fileName in blockingCollection.GetConsumingEnumerable())
            {
                new Fileripper().RipFile(fileName);
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
            if (patternsForSpecificDrive?.Count > 0) { currentDriveSearchPatterns.AddRange(patternsForSpecificDrive); }

            // include patterns for "all" drives
            Configuration.DriveFileSearchPatterns.TryGetValue(".:", out List<string> patternsForAllDrives);
            if (patternsForAllDrives?.Count > 0) { currentDriveSearchPatterns.AddRange(patternsForAllDrives); }
        }

        public void DirSearch(string sDir)
        {
            // long live the 'null-coalescing' operator ?? to handle cases of 'null'  :)
            foreach (string fileName in GetFiles(sDir) ?? new String[0])
            {
                Spinner.SearchSpin();
                countFiles++;

                if ((countAddedFileNames + 1) % 100 == 0)
                    logger.Log($"search busy:checked {countFiles} with {countAddedFileNames} matches in {sw.Elapsed}", LogLevel.VERBOSE);

                logger.Log($"[{fileName}]", LogLevel.TRACE);

                if (MatchFileAgainstSearchPatterns(fileName))
                {
                    blockingCollection.Add(fileName);
                    countAddedFileNames++;
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
            foreach (String pattern in currentDriveSearchPatterns)
            {
                if ((new Regex(pattern, RegexOptions.IgnoreCase).Match(Path.GetFullPath(fileName))).Success) { return true; }
            }
            return false;
        }

        public String[] GetFiles(string currentDirectory)
        {
            string[] filesystemEntries;
            try
            {
                filesystemEntries = Directory.GetFiles(currentDirectory);
            }
            catch (UnauthorizedAccessException e)
            {
                logger.Log($"getFiles: Unauthorized to retrieve file entries from [{currentDirectory}][{e.Message}]", LogLevel.ERROR);
                filesystemEntries = null;
            }
            catch (Exception e)
            {
                logger.Log($"getFiles: Failed to retrieve file entries from [{currentDirectory}][{e.Message}]", LogLevel.ERROR);
                filesystemEntries = null;
            }
            return filesystemEntries;
        }

        public String[] GetDirs(string currentDirectory)
        {
            string[] filesystemEntries;
            try
            {
                filesystemEntries = Directory.GetDirectories(currentDirectory);
            }
            catch (UnauthorizedAccessException e)
            {
                logger.Log($"getDirs: Unauthorized to retrieve (sub)directories from [{currentDirectory}][{e.Message}]", LogLevel.ERROR);
                filesystemEntries = null;
            }
            catch (Exception e)
            {
                logger.Log($"getDirs: Failed to retrieve (sub)directories from [{currentDirectory}][{e.Message}]", LogLevel.ERROR);
                filesystemEntries = null;
            }
            return filesystemEntries;
        }

    }
}
