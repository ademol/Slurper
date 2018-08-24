using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Slurper
{
    public class SampleConfig
    {
        public static string sampleConfig;

        public static void Init()
        {
            // sample config
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "Slurper.slurper.cfg.txt";

            using (System.IO.Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (System.IO.StreamReader reader = new System.IO.StreamReader(stream))
            {
                sampleConfig = reader.ReadToEnd();
            }

            //

        }
    }
}
