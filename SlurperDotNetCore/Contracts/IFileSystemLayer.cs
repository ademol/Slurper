namespace SlurperDotNetCore.Contracts
{
    public interface IFileSystemLayer
    {
         
        ILogger Logger {get;}

        string TargetDirBasePath { get; set; }                             // relative directory for file to be copied to

        char PathSep { get; } 

        void CreateTargetLocation();

        void GetMountedPartitionInfo();
    }
}
