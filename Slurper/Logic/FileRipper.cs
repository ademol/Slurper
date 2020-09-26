using System;
using System.IO;
using System.Threading.Tasks;
using Slurper.Contracts;
using Slurper.Providers;

namespace Slurper.Logic
{
    public class FileRipper
    {
        private static readonly ILogger Logger = LogProvider.Logger;

        public async Task RipFile(string filename)
        {
            var targetPath = TargetPath(filename);
            var targetFileNameFullPath = targetPath + Path.GetFileName(filename);

            Logger.Log($"RipFile: ripping [{filename}] => [{targetFileNameFullPath}]", LogLevel.Verbose);

            try
            {
                if (ConfigurationService.DryRun) return;
                Directory.CreateDirectory(targetPath);
                await Task.Run(() => File.Copy(filename, targetFileNameFullPath));
            }
            catch (Exception e)
            {
                Logger.Log($"RipFile: copy of [{filename}] failed with [{e.Message}]", LogLevel.Error);
            }
        }

        private string TargetPath(string filename)
        {
            var targetRelativePath = Path.GetDirectoryName(filename);
            targetRelativePath = Program.FileSystemLayer.SanitizePath(targetRelativePath);

            var sep = Program.FileSystemLayer.PathSep;
            var targetDirBasePath = Program.FileSystemLayer.TargetDirBasePath;

            return $"{targetDirBasePath}{sep}{targetRelativePath}{sep}";
        }
    }
}
