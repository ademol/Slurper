namespace Slurper.Contracts
{
    public interface IFileSystemLayer
    {
        ILogger Logger {get;}

        string TargetDirBasePath { get; set; }

        char PathSep { get; } 

        void CreateTargetLocation();

        void GetMountedPartitionInfo();
    }
}
