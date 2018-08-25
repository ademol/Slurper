
using Slurper.Logic;
using Slurper.Providers;

namespace Slurper
{
    class Program
    {
        /*
         * Sluper: Utility to search for files on a Windows computer that match one or more regex patterns. 
         *         The files found are then copied to a subdirectory in the location from where the program is run.
         *         
         *         note: 
         *         The drive that the program is run from, is excluded from searching.
         *           => suggested use is to run this program from an portable location (USB/HD) 
         *           
         */
    
        static readonly ILogger logger = new LogProvider().GetLog();

        static void Main(string[] args)
        {
            // init
            Configuration.InitSampleConfig();

            // handle arguments
            Configuration.ProcessArguments(args);

            // determine & create target directory
            FileSystemLayer.CreateTargetLocation();
            
            // configuration 
            Configuration.Configure();

            // get drives to search
            FileSystemLayer.GetDriveInfo();

            // find files matching pattern(s) from all applicable drives, and copy them to ripdir
            Searcher.SearchAndCopyFiles();
        }
    }
}
