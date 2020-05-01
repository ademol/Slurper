namespace Slurper.Contracts
{
    public interface IFileSystemLayer
    {
        string TargetDirBasePath { get; }

        char PathSep { get; }

        void CreateTargetLocation();

        void SetSourcePaths();

        string SanitizePath(string path);
    }
}