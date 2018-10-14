namespace SlurperDotNetCore.Contracts
{
    public interface IFileSystemLayer
    {
         
        ILogger logger {get;}

        string targetDirBasePath { get; set; }                             // relative directory for file to be copied to

        char pathSep { get; } 

        void CreateTargetLocation();

        void GetDriveInfo();
    }
}
