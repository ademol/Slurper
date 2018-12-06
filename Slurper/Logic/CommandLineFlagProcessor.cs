
using System;

using Slurper.Providers;

namespace Slurper.Logic
{
    public enum CmdLineFlag { SILENT, INCLUDEMYDRIVE, VERBOSE, DRYRUN, TRACE, GENERATE };

    public class CommandLineFlagProcessor
    {
        static readonly ILogger logger = LogProvider.Logger;

        public static void ProcessArgumentFlags(string argument)
        {
            foreach (char c in argument)
            {
                switch (c)
                {
                    case 'i':
                        Configuration.cmdLineFlagSet.Add(CmdLineFlag.INCLUDEMYDRIVE);
                        break;
                    case 's':
                        Configuration.cmdLineFlagSet.Add(CmdLineFlag.SILENT);
                        break;
                    case 'h':
                        DisplayMessages.Help();
                        break;
                    case 'v':
                        Configuration.cmdLineFlagSet.Add(CmdLineFlag.VERBOSE);
                        break;
                    case 'd':
                        Configuration.cmdLineFlagSet.Add(CmdLineFlag.DRYRUN);
                        break;
                    case 't':
                        Configuration.cmdLineFlagSet.Add(CmdLineFlag.TRACE);
                        Configuration.cmdLineFlagSet.Add(CmdLineFlag.VERBOSE);
                        break;
                    case '/':
                        break;
                    case '-':
                        break;
                    case 'g':
                        Configuration.cmdLineFlagSet.Add(CmdLineFlag.GENERATE);
                        break;
                    default:
                        Console.WriteLine("option [{0}] not supported", c);
                        DisplayMessages.Help();
                        Environment.Exit(0);
                        break;
                }
            }
            var displayCmdLineOptionsSet = String.Join(",", Configuration.cmdLineFlagSet.ToArray());
            logger.Log($"Arguments: [{displayCmdLineOptionsSet}] ", LogLevel.VERBOSE);
        }       
    }
}
