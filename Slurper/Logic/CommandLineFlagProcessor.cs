
using System;
using System.Linq;
using Slurper.Contracts;
using Slurper.Output;
using Slurper.Providers;

namespace Slurper.Logic
{
    public enum CmdLineFlag { Silent, Includemydrive, Verbose, Dryrun, Trace, Generate }

    public static class CommandLineFlagProcessor
    {
        private static readonly ILogger Logger = LogProvider.Logger;

        public static void ProcessArgumentFlags(string argument)
        {
            foreach (char c in argument)
            {
                switch (c)
                {
                    case 'i':
                        Configuration.CmdLineFlagSet.Add(CmdLineFlag.Includemydrive);
                        break;
                    case 's':
                        Configuration.CmdLineFlagSet.Add(CmdLineFlag.Silent);
                        break;
                    case 'h':
                        DisplayMessages.Help();
                        break;
                    case 'v':
                        Configuration.CmdLineFlagSet.Add(CmdLineFlag.Verbose);
                        break;
                    case 'd':
                        Configuration.CmdLineFlagSet.Add(CmdLineFlag.Dryrun);
                        break;
                    case 't':
                        Configuration.CmdLineFlagSet.Add(CmdLineFlag.Trace);
                        Configuration.CmdLineFlagSet.Add(CmdLineFlag.Verbose);
                        break;
                    case '/':
                        break;
                    case '-':
                        break;
                    case 'g':
                        Configuration.CmdLineFlagSet.Add(CmdLineFlag.Generate);
                        break;
                    default:
                        Console.WriteLine("option [{0}] not supported", c);
                        DisplayMessages.Help();
                        Environment.Exit(0);
                        break;
                }
            }
            var displayCmdLineOptionsSet = String.Join(",", Configuration.CmdLineFlagSet.ToArray());
            Logger.Log($"Arguments: [{displayCmdLineOptionsSet}] ", LogLevel.Verbose);
        }       
    }
}
