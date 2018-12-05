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
        static double countMatches = 0;
        static BlockingCollection<string> blockingCollection = new BlockingCollection<string>();
        static Stopwatch sw;

        List<string> thisDrivePatternsToLookFor;

        public FileSearcher()
        {
            thisDrivePatternsToLookFor = new List<string>();
        }

        public static void SearchDrives()
        {
            sw = new Stopwatch();
            sw.Start();
            System.Threading.Thread myThread;
            myThread = new System.Threading.Thread(
            new System.Threading.ThreadStart(BlockingCollectionFileRipper));
            myThread.Start();

            int maxParallel = Configuration.PARALLEL ? -1 : 1;
            logger.Log($"maxParallel = [{maxParallel}]", LogLevel.VERBOSE);

            Parallel.ForEach(Configuration.drivesToSearch, (new ParallelOptions { MaxDegreeOfParallelism = maxParallel }), (currentDrive) =>
             {
                 new FileSearcher().DriveSearch(currentDrive);
             });
            blockingCollection.CompleteAdding();
            logger.Log($"search done:checked {countFiles} with {countMatches} matches in {sw.Elapsed}", LogLevel.VERBOSE);
            while (!blockingCollection.IsCompleted)
            { }
            sw.Stop();
            logger.Log($"copy done:checked {countFiles} with {countMatches} matches in {sw.Elapsed}", LogLevel.VERBOSE);

        }

        public static void BlockingCollectionFileRipper()
        {
            foreach (var item in blockingCollection.GetConsumingEnumerable())
            {
                new Fileripper().RipFile(item);
            }
        }


        public void DriveSearch(string currentDrive)
        {
            SetFilePatternsForDrive(currentDrive);
            DirSearch(currentDrive);
        }

        public void SetFilePatternsForDrive(string driveInfoName)
        {
            String driveIdentifier = driveInfoName.Substring(0, 2); 

            Configuration.driveFilePatternsTolookfor.TryGetValue(driveIdentifier.ToUpper(), out List<string> patternsForSpecificDrive);
            if (patternsForSpecificDrive != null) { thisDrivePatternsToLookFor.AddRange(patternsForSpecificDrive); }

            // add patterns for all (.:) drives
            Configuration.driveFilePatternsTolookfor.TryGetValue(".:", out List<string> patternsForAllDrives);
            if (patternsForAllDrives != null) { thisDrivePatternsToLookFor.AddRange(patternsForAllDrives); }
        }


        public void DirSearch(string sDir)
        {
            // long live the 'null-coalescing' operator ?? to handle cases of 'null'  :)
            foreach (string directoryName in GetDirs(sDir) ?? new String[0])
            {
                foreach (string fileName in GetFiles(directoryName) ?? new String[0])
                {
                    Spinner.SearchSpin();
                    countFiles++;

                    if ((countMatches + 1) % 100 == 0)
                        logger.Log($"search busy:checked {countFiles} with {countMatches} matches in {sw.Elapsed}", LogLevel.VERBOSE);

                    logger.Log($"[{fileName}]", LogLevel.TRACE);

                    // check if file is wanted by any of the specified patterns
                    foreach (String pattern in thisDrivePatternsToLookFor)
                    {
                        if ((new Regex(pattern, RegexOptions.IgnoreCase).Match(Path.GetFullPath(fileName))).Success) { blockingCollection.Add(fileName); countMatches++; break; }
                    }
                }
                try
                {
                    DirSearch(directoryName);
                }
                catch (Exception e)
                {
                    logger.Log($"DirSearch: Could not read dir [{directoryName}][{e.Message}]", LogLevel.ERROR);
                }
            }
        }

        static String[] GetFiles(string dir)
        {
            try
            {
                return Directory.GetFiles(dir, "*.*"); 
            }
            catch (UnauthorizedAccessException e)
            {
                logger.Log($"getFiles: Unauthorized to retrieve fileList from [{dir}][{e.Message}]", LogLevel.ERROR);
            }
            catch (Exception e)
            {
                logger.Log($"getFiles: Failed to retrieve fileList from [{dir}][{e.Message}]", LogLevel.ERROR);
            }
            return null;
        }

        static String[] GetDirs(string sDir)
        {
            try
            {
                return Directory.GetDirectories(sDir);
            }
            catch (UnauthorizedAccessException e)
            {
                logger.Log($"getFiles: Unauthorized to retrieve dirList from [{sDir}][{e.Message}]", LogLevel.ERROR);
            }
            catch (Exception e)
            {
                logger.Log($"getDirs: Failed to retrieve dirList from [{sDir}][{e.Message}]", LogLevel.ERROR);
            }
            return null;
        }
    }
}
