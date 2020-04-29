using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slurper
{
    class Configurator
    {
        public static void Configure()
        {
            if (!LoadConfigFile() || driveFilePatternsTolookfor.Count == 0)
            {
                // default config
                if (VERBOSE) { Console.WriteLine("Configure: config file [{0}] not found, or no valid patterns in file found => using default pattern [{1}]", cfgFileName, DefaultRegexPattern); }

                //// add a regex set as a default.
                //filePatternsTolookfor.Add(DefaultRegexPattern);

                //todo: check => add to driveFilePatternsTolookfor
                ArrayList defPattern = new ArrayList();
                defPattern.Add(DefaultRegexPattern);
                driveFilePatternsTolookfor.Add(".:", defPattern);
            }
            // show patterns used
            if (VERBOSE)
            {
                foreach (String drive in driveFilePatternsTolookfor.Keys)
                {
                    ArrayList patterns;
                    driveFilePatternsTolookfor.TryGetValue(drive, out patterns);
                    foreach (String pattern in patterns)
                    {
                        Console.WriteLine("Configure: Pattern to use: disk [{0}]  pattern [{1}] ", drive, pattern);
                    }
                }
            }
        }

        static Boolean LoadConfigFile()
        {
            Boolean cfgLoaded = false;
            if (File.Exists(cfgFileName))
            {
                String line;
                String REGEXpattern = @"^([^#]:)(.*)";               // pattern to match valid lines from config file   <driveLetter:><regex>
                Regex r = new Regex(REGEXpattern);
                try
                {
                    //todo: also move to alphafs ?
                    using (System.IO.StreamReader sr = new System.IO.StreamReader(cfgFileName))
                    {
                        while (!sr.EndOfStream)
                        {
                            line = sr.ReadLine();
                            //if (Debug) { Console.WriteLine(line); }

                            Match m = r.Match(line);
                            if (m.Success)
                            {
                                String drive = m.Groups[1].Value.ToUpper();
                                String regex = m.Groups[2].Value;
                                filePatternsTolookfor.Add(regex);
                                drivesRequestedToBeSearched.Add(drive);
                                if (VERBOSE) { Console.WriteLine("LoadConfigFile: [{0}] => for drive:[{1}] regex:[{2}]", line, drive, regex); }

                                // add to hash
                                if (driveFilePatternsTolookfor.ContainsKey(drive))
                                {
                                    // add to existing key
                                    ArrayList t;
                                    driveFilePatternsTolookfor.TryGetValue(drive, out t);
                                    t.Add(regex);

                                }
                                else
                                {
                                    ArrayList t = new ArrayList();
                                    t.Add(regex);
                                    driveFilePatternsTolookfor.Add(drive, t);
                                }



                            }
                            else
                            {
                                if (VERBOSE) { Console.WriteLine("LoadConfigFile: [{0}] => regex:[{1}]", line, "---skipped---"); }
                            }
                        }
                    }
                    cfgLoaded = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("LoadConfigFile: Could not read [{0}] [{1}]", cfgFileName, e.Message);
                }

            }
            return cfgLoaded;
        }


    }
}
