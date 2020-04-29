using System;
using Slurper.Contracts;
using Slurper.Logic;

namespace Slurper.Output
{
   public class ConsoleLogger : ILogger
    {

        public void Log(string message, LogLevel level)
        {
            var previousColor = Console.ForegroundColor;
            ConsoleColor color = previousColor;

            bool displayLog = false;
            switch (level)
            {
                case LogLevel.Trace:
                    if ( Configuration.Trace ) {
                        displayLog = true;
                        color = ConsoleColor.DarkRed;
                    } 
                    break;
                case LogLevel.Log:
                    displayLog = true;
                    break;
                case LogLevel.Verbose:
                    if ( Configuration.Verbose ) {
                        displayLog = true;
                        color = ConsoleColor.DarkYellow;
                    }
                    break;
                case LogLevel.Error:
                    displayLog = true;
                    color = ConsoleColor.Red;
                    break;
                case LogLevel.Warn:
                    displayLog = true;
                    color = ConsoleColor.Yellow;
                    break;
            }

            if (displayLog)
            {
                Console.ForegroundColor = color;
                Console.WriteLine("[{0}][{1}]", level, message);
                Console.ForegroundColor = previousColor;
            }

        }

        public void Log(string message)
        {
            Log(message, LogLevel.Log);
        }
    }
}
