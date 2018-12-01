using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Slurper
{
   public class ConsoleLogger : ILogger
    {
        private static readonly Dictionary<logLevel, ConsoleColor> consoleLevelColor = new Dictionary<logLevel, ConsoleColor>()
        {
            { logLevel.TRACE, ConsoleColor.DarkRed },
            { logLevel.LOG, ConsoleColor.White },
            { logLevel.VERBOSE, ConsoleColor.Yellow },
            { logLevel.ERROR, ConsoleColor.Red },
            { logLevel.WARN, ConsoleColor.DarkYellow }
        };

        private static readonly ConsoleColor foreGroundColor = Console.ForegroundColor;

        public void Log(string Message, logLevel messageLogLevel)
        {
            if (Configuration.SILENT) return;
            if (messageLogLevel == logLevel.TRACE && !Configuration.TRACE) return;
            if (messageLogLevel == logLevel.VERBOSE && !Configuration.VERBOSE) return;

            SetForeGroundColorForLogLevel(messageLogLevel);
            WriteToConsole(GetCallingMember(), messageLogLevel, Message);
            RestoreForeGroundcolor();
        }

        private void WriteToConsole(string callingMethod, logLevel level, string logMessage)
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

        private void SetForeGroundColorForLogLevel(logLevel Level)
        {
            if (consoleLevelColor.TryGetValue(Level, out ConsoleColor foregroundColor))
            {
                Console.ForegroundColor = foregroundColor;
            }
        }

        public void Log(string Message)
        {
            Log(Message, logLevel.LOG);
        }
    }
}
