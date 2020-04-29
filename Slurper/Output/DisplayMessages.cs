using System;
using Slurper.Logic;

namespace Slurper.Output
{
    static class DisplayMessages
    {
        public static void Help()
        {
            var txt = "";
            txt += "Copy files that have their filename matched, to ./rip/<hostname><timestamp> directory \n\n";
            txt += "In default mode (without cfg file) it matches jpg files by the jpg extenstion\n";
            txt += "use the /v flag for verbose output => slurper.exe /v \n";
            txt += "use the /d flag for dry run (no file copy mode) => slurper.exe /d \n";
            txt += "use the /t flag for trace => slurper.exe /t    (note: setting trace sets verbose) \n";
            txt += "use the /g flag to generate a sample cfg file\n";
            txt += "\n";
            txt += "(optional) when a config file exits (./slurper.cfg) it is used to specify custom regexs to match \n";
            txt += "\n";
            txt += Configuration.SampleConfig;

            Console.WriteLine(txt);
            Environment.Exit(0);
        }
    }
}
