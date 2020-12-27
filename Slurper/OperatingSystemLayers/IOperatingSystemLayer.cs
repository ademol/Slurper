using System.Collections.Generic;

namespace Slurper.OperatingSystemLayers
{
    public interface IOperatingSystemLayer
    {
        string? TargetDirBasePath { get; }

        char PathSep { get; }

        void CreateTargetLocation();

        IEnumerable<string> GetSourcePaths();

        string SanitizePath(string? path);
    }
}
