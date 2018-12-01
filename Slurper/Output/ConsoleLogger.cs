using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Slurper
{
   public class ConsoleLogger : ILogger
    {
        private static readonly Dictionary<LogLevel, ConsoleColor> consoleLevelColor = new Dictionary<LogLevel, ConsoleColor>()
        {
            { LogLevel.TRACE, ConsoleColor.DarkRed },
            { LogLevel.LOG, ConsoleColor.White },
            { LogLevel.VERBOSE, ConsoleColor.Yellow },
            { LogLevel.ERROR, ConsoleColor.Red },
            { LogLevel.WARN, ConsoleColor.DarkYellow }
        };

        private static readonly ConsoleColor foreGroundColor = Console.ForegroundColor;

        public void Log(string Message, LogLevel messageLogLevel)
        {
            if (Configuration.SILENT) return;
            if (messageLogLevel == LogLevel.TRACE && !Configuration.TRACE) return;
            if (messageLogLevel == LogLevel.VERBOSE && !Configuration.VERBOSE) return;

            SetForeGroundColorForLogLevel(messageLogLevel);
            WriteToConsole(GetCallingMember(), messageLogLevel, Message);
            RestoreForeGroundcolor();
        }

        private void WriteToConsole(string callingMethod, LogLevel level, string logMessage)
        {
            Console.WriteLine($"[{callingMethod}][{level.ToString()}][{logMessage}]");
        }

        private string GetCallingMember()
        {
            StackFrame frame = new StackFrame(2, true);
            return frame.GetMethod().ToString();
        }

        private void RestoreForeGroundcolor()
        {
            Console.ForegroundColor = foreGroundColor;
        }

        private void SetForeGroundColorForLogLevel(LogLevel Level)
        {
            if (consoleLevelColor.TryGetValue(Level, out ConsoleColor foregroundColor))
            {
                Console.ForegroundColor = foregroundColor;
            }
        }

        public void Log(string Message)
        {
            Log(Message, LogLevel.LOG);
        }
    }
}
