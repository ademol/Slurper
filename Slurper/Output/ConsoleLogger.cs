using System;
using System.Collections.Generic;
using System.Diagnostics;
using Slurper.Contracts;
using Slurper.Logic;

namespace Slurper.Output
{
   public class ConsoleLogger : ILogger
    {
        private static readonly Dictionary<LogLevel, ConsoleColor> ConsoleLevelColor = new Dictionary<LogLevel, ConsoleColor>
        {
            { LogLevel.Trace, ConsoleColor.DarkRed },
            { LogLevel.Log, ConsoleColor.White },
            { LogLevel.Verbose, ConsoleColor.Yellow },
            { LogLevel.Error, ConsoleColor.Red },
            { LogLevel.Warn, ConsoleColor.DarkYellow }
        };

        private static readonly ConsoleColor ForeGroundColor = Console.ForegroundColor;

        public void Log(string message, LogLevel messageLogLevel)
        {
            if (Configuration.CmdLineFlagSet.Contains(CmdLineFlag.Silent)) return;
            if (messageLogLevel == LogLevel.Trace && !Configuration.CmdLineFlagSet.Contains(CmdLineFlag.Trace)) return;
            if (messageLogLevel == LogLevel.Verbose && !Configuration.CmdLineFlagSet.Contains(CmdLineFlag.Verbose)) return;

            SetForeGroundColorForLogLevel(messageLogLevel);
            WriteToConsole(GetCallingMember(), messageLogLevel, message);
            RestoreForeGroundcolor();
        }

        private void WriteToConsole(string callingMethod, LogLevel level, string logMessage)
        {

            lock (this)
            {
                Console.WriteLine($"[{callingMethod}][{level.ToString()}][{logMessage}]");
            }
        }

        private string GetCallingMember()
        {
            StackFrame frame = new StackFrame(2, true);
            return frame.GetMethod().ToString();
        }

        private void RestoreForeGroundcolor()
        {
            Console.ForegroundColor = ForeGroundColor;
        }

        private void SetForeGroundColorForLogLevel(LogLevel level)
        {
            if (ConsoleLevelColor.TryGetValue(level, out ConsoleColor foregroundColor))
            {
                Console.ForegroundColor = foregroundColor;
            }
        }

        public void Log(string message)
        {
            Log(message, LogLevel.Log);
        }
    }
}
