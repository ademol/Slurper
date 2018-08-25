using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slurper
{
   public class ConsoleLogger : ILogger
    {

        public void Log(string Message, logLevel Level = logLevel.LOG)
        {
            var previousColor = Console.ForegroundColor;
            ConsoleColor color = previousColor;

            bool displayLog = false;
            switch (Level)
            {
                case logLevel.TRACE:
                    if ( Configuration.TRACE ) {
                        displayLog = true;
                        color = ConsoleColor.DarkRed;
                    } 
                    break;
                case logLevel.LOG:
                    displayLog = true;
                    break;
                case logLevel.VERBOSE:
                    if ( Configuration.VERBOSE ) {
                        displayLog = true;
                        color = ConsoleColor.Blue;
                    }
                    break;
                case logLevel.ERROR:
                    displayLog = true;
                    color = ConsoleColor.Red;
                    break;
                case logLevel.WARN:
                    displayLog = true;
                    color = ConsoleColor.Yellow;
                    break;
            }

            if (displayLog)
            {
                Console.ForegroundColor = color;
                Console.WriteLine("[{0}][{1}]", Level, Message);
                Console.ForegroundColor = previousColor;
            }

        }


    }
}
