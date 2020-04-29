using System.Collections;

namespace Slurper.Contracts
{
    public interface IFileSystemLayer
    {
        string TargetDirBasePath { get; set; }

        char PathSep { get; } 

        void CreateTargetLocation();

        void GetMountedPartitionInfo();

        ArrayList GetPattern(string sDir);



    }
}
