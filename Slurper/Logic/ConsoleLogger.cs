using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slurper
{
   class ConsoleLogger
    {

        public static void Log(string Message, logLevel Level = logLevel.LOG)
        {
            //var currentColor = Console.ForegroundColor;
            if (Level == logLevel.LOG ||  Configuration.TRACE)
            {
                Console.WriteLine("[{0}][{1}]", Level, Message);
            }
        }

        //public static void LogConfig(string Message, logLevel Level = logLevel.LOG)
        //{
        //    //var currentColor = Console.ForegroundColor;
        //    if (Level == logLevel.LOG || Configuration.TRACE)
        //    {
        //        Console.WriteLine("[{0}][{1}]", Level, Message);
        //    }
        //}


        public enum logLevel { TRACE, LOG }

    }
}
