using System;
using Slurper.Contracts;
using Slurper.Logic;

namespace Slurper.Output
{
    public class ConsoleLogger : ILogger
    {
        public void Log(string message, LogLevel level = LogLevel.Log)
        {
            switch (level)
            {
                case LogLevel.Trace:
                    TraceLog(message);
                    break;
                case LogLevel.Log:
                    LogLog(message);
                    break;
                case LogLevel.Verbose:
                    VerboseLog(message);
                    break;
                case LogLevel.Error:
                    ErrorLog(message);
                    break;
                case LogLevel.Warn:
                    WarnLog(message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }

        private static void DoLog(ConsoleColor color, LogLevel level, string message)
        {   
            var previous = Console.ForegroundColor;
 
            Console.ForegroundColor = color;
            Console.WriteLine("[{0}][{1}]", level, message);
            
            Console.ForegroundColor = previous;
        }

        private static void WarnLog(string message)
        {
            DoLog(ConsoleColor.Yellow, LogLevel.Warn, message);
        }

        private static void ErrorLog(string message)
        {
            DoLog(ConsoleColor.Red, LogLevel.Warn, message);
        }

        private static void VerboseLog(string message)
        {
            if (!ConfigurationService.Verbose) return;
            DoLog(ConsoleColor.DarkYellow, LogLevel.Verbose, message);
        }

        private static void LogLog(string message)
        {
            DoLog(Console.ForegroundColor, LogLevel.Log, message);
        }

        private static void TraceLog(string message)
        {
            if (!ConfigurationService.Trace) return;
            {
                DoLog(ConsoleColor.DarkRed, LogLevel.Verbose, message);
            }
        }
    }
}