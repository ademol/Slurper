using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alphaleonis.Win32.Filesystem;
using Slurper.Logic;

namespace Slurper.Providers
{
    public class FileSystemLayer
    {
        static readonly ILogger logger = new LogProvider().GetLog();

        public static void CreateTargetLocation()
        {
            String curDir = Directory.GetCurrentDirectory();
            String hostname = (System.Environment.MachineName).ToLower();
            String dateTime = String.Format("{0:yyyyMMdd_HH-mm-ss}", DateTime.Now);
            if (Configuration.VERBOSE) { Console.WriteLine("CreateTargetLocation: [{0}][{1}][{2}]", hostname, curDir, dateTime); }

            FilePath.targetDirBasePath = string.Concat(curDir, FilePath.pathSep, Configuration.ripDir, FilePath.pathSep, hostname, "_", dateTime);
            if (Configuration.VERBOSE) { Console.WriteLine("CreateTargetLocation: [{0}]", FilePath.targetDirBasePath); }

            try
            {
                if (!Configuration.DRYRUN) { Directory.CreateDirectory(FilePath.targetDirBasePath); }

            }
            catch (Exception e)
            {
                logger.Log($"CreateTargetLocation: failed to create director [{FilePath.targetDirBasePath}][{e.Message}]", logLevel.ERROR);
            }
        }

        public static void GetDriveInfo()
        {
            // look for possible drives to search 

            // all drives
            DriveInfo[] allDrives = DriveInfo.GetDrives();

            // mydrive
            String mydrive = Path.GetPathRoot(Directory.GetCurrentDirectory());
            if (Configuration.VERBOSE) { Console.WriteLine("GetDriveInfo: mydrive = [{0}]", mydrive); }

            foreach (DriveInfo d in allDrives)
            {
                // D:\  -> D:
                String driveID = d.Name.Substring(0, 2).ToUpper();

                // check if drive will be included
                Boolean driveToBeIncluded = false;
                String reason = "configuration";

                // check for wildcard
                if (Configuration.driveFilePatternsTolookfor.ContainsKey(".:"))
                {
                    driveToBeIncluded = true;
                    reason = "configuration for drive .:";
                }
                // check for specific drive
                if (Configuration.driveFilePatternsTolookfor.ContainsKey(driveID))
                {
                    driveToBeIncluded = true;
                    reason = "configuration for drive " + driveID;
                }


                // skip the drive i'm running from
                if ((mydrive.ToUpper()).Equals(d.Name.ToUpper()))
                {
                    driveToBeIncluded = false;
                    reason = "this the drive i'm running from";
                }

                // include this drive
                if (driveToBeIncluded)
                {
                    Configuration.drivesToSearch.Add(d.Name);
                }

                if (Configuration.VERBOSE) { Console.WriteLine("GetDriveInfo: found drive [{0}]\t included? [{1}]\t reason[{2}]", driveID, driveToBeIncluded, reason); }

            }

        }



    }
}
