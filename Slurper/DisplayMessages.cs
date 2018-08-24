using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slurper
{
    class DisplayMessages
    {
  

        public static void help()
        {
            //todo: nicer help
            String txt = "";
            txt += "Copy files that have their filename matched, to ./rip/<hostname><timestamp> directory \n\n";
            txt += "In default mode (without cfg file) it matches jpg files by the jpg extenstion\n";
            txt += "use the /v flag for verbose output => slurper.exe /v \n";
            txt += "use the /d flag for dryrun (no filecopy mode) => slurper.exe /d \n";
            txt += "use the /t flag for trace => slurper.exe /t    (note: setting trace sets verbose) \n";
            txt += "use the /g flag to generate a sample cfg file\n";
            txt += "\n";
            txt += "(optional) when a configfile exits (./slurper.cfg) it is used to specify custom regexes to match \n";
            txt += "\n";
            txt += SampleConfig.sampleConfig;

            Console.WriteLine(txt);
            Environment.Exit(0);
        }

    }
}
