
using Slurper.Logic;
using System.Collections.Generic;

namespace Slurper.Contracts
{
    public interface IConfiguration
    {
        void Configure();
        void ShowPatternsUsedByDrive();
        void InitSampleConfig();
        void GenerateSampleConfigFile();
        void LoadConfigFile();

        List<CmdLineFlag> getCmdLineFlagSet();


        bool IsValidRegex(string pattern);

        void ProcessArguments(string[] args);
      //  Configuration GetInstance();
    }
}
