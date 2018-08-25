using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Slurper
{
    public class Configuration
    {
        public static string sampleConfig { get; set; }
        public static bool VERBOSE { get; set; } = false;       // show additional output what is done

        public static bool DRYRUN { get; set; } = false;        // (only) show what will be done (has implicit VERBOSE)

        public static bool TRACE { get; set; } = false;         // VERBOSE + show also unmatched files 


        public static String cfgFileName { get; set; } = "slurper.cfg";      // regex pattern(s) configuration file



        public static string ripDir { get; set; } = "rip";                   // relative root directory for files to be copied to

        public static void InitSampleConfig()
        {
            // sample config
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "Slurper.slurper.cfg.txt";

            using (System.IO.Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (System.IO.StreamReader reader = new System.IO.StreamReader(stream))
            {
                sampleConfig = reader.ReadToEnd();
            }

        }



        private static void generateConfig()
        {
            Console.WriteLine("generating sample config file [{0}]", cfgFileName);
            try
            {
                System.IO.File.WriteAllText(cfgFileName, Configuration.sampleConfig);
            }
            catch (Exception e)
            {
                Console.WriteLine("generateConfig: failed to generate [{0}][{1}]", cfgFileName, e.Message);
            }
        }


        public static void ProcessArguments(string[] args)
        {

            // concat the arguments, handle each char as switch selection  (ignore any '/' or '-')
            string concat = String.Join("", args);
            foreach (char c in concat)
            {
                switch (c)
                {
                    case 'h':
                        DisplayMessages.help();
                        break;
                    case 'v':
                        VERBOSE = true;
                        break;
                    case 'd':
                        DRYRUN = true;
                        break;
                    case 't':
                        TRACE = true;
                        VERBOSE = true;
                        break;
                    case '/':
                        break;
                    case '-':
                        break;
                    case 'g':
                        generateConfig();
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("option [{0}] not supported", c);
                        DisplayMessages.help();
                        break;
                }
            }
            if (VERBOSE) { Console.WriteLine("Arguments: VERBOSE[{0}] DRYRUN[{1}] TRACE[{2}]", VERBOSE, DRYRUN, TRACE); }





        }

    }
}
