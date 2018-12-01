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
    public class Searcher
    {
        static readonly ILogger logger = LogProvider.Logger;
        static double countFiles = 0;
        static double countMatches = 0;
        static BlockingCollection<string> blockingCollection = new BlockingCollection<string>();
        static Stopwatch sw;

        public static void SearchAndCopyFiles()
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
                 new Searcher().DirSearch(currentDrive);
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

        public void DirSearch(string sDir)
        {
            List<string> thisDrivePatternsToLookFor = new List<string>();
            String curDrive = sDir.Substring(0, 2);    // aka c:  

        //   if (curDrive != "G:" ) { return; }

            List<string> v;
            Configuration.driveFilePatternsTolookfor.TryGetValue(curDrive.ToUpper(), out v);
            if (v != null) { thisDrivePatternsToLookFor.AddRange(v); }

            // add patterns for all (.:) drives
            Configuration.driveFilePatternsTolookfor.TryGetValue(".:", out v);
            if (v != null) { thisDrivePatternsToLookFor.AddRange(v); }

            // long live the 'null-coalescing' operator ?? to handle cases of 'null'  :)
            foreach (string d in GetDirs(sDir) ?? new String[0])
            {
                foreach (string f in GetFiles(d) ?? new String[0])
                {
                    Spinner.Spin();
                    countFiles++;

                    if ((countMatches + 1) % 100 == 0)
                        logger.Log($"search busy:checked {countFiles} with {countMatches} matches in {sw.Elapsed}", LogLevel.VERBOSE);

                    logger.Log($"[{f}]", LogLevel.TRACE);

                    // check if file is wanted by any of the specified patterns
                    foreach (String p in thisDrivePatternsToLookFor)
                    {
                        if ((new Regex(p).Match(Path.GetFullPath(f))).Success) { blockingCollection.Add(f); countMatches++; break; }
                    }
                }
                try
                {
                    DirSearch(d);
                }
                catch (Exception e)
                {
                    logger.Log($"DirSearch: Could not read dir [{d}][{e.Message}]", LogLevel.ERROR);
                }
            }
        }

        static String[] GetFiles(string dir)
        {
            try
            {
                String[] files = Directory.GetFiles(dir, "*.*");
                return files;
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
                string[] dirs = Directory.GetDirectories(sDir);
                return dirs;
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
