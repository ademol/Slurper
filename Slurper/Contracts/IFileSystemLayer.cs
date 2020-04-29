namespace Slurper.Contracts
{
    public interface IFileSystemLayer
    {
        string TargetDirBasePath { get; set; }

        char PathSep { get; } 

        void CreateTargetLocation();

        void GetMountedPartitionInfo();
        
        
        
        
        
    }
}
