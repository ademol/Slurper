using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;


namespace Slurper.Logic
{
    public class FileRipper
    {
        private readonly ILogger<FileRipper> _logger;

        public FileRipper(ILogger<FileRipper> logger)
        {
            _logger = logger;
        }

        public async Task RipFile(string filename)
        {
            var targetPath = TargetPath(filename);
            var targetFileNameFullPath = targetPath + Path.GetFileName(filename);

            _logger.LogDebug($"RipFile: ripping [{filename}] => [{targetFileNameFullPath}]");

            try
            {
                if (ConfigurationService.DryRun) return;
                Directory.CreateDirectory(targetPath);
                await Task.Run(() => File.Copy(filename, targetFileNameFullPath));
            }
            catch (Exception e)
            {
                _logger.LogError($"RipFile: copy of [{filename}] failed with [{e.Message}]");
            }
        }

        private string TargetPath(string filename)
        {
            var targetRelativePath = Path.GetDirectoryName(filename);
            targetRelativePath = SlurperApp.OperatingSystemLayer.SanitizePath(targetRelativePath);

            var sep = SlurperApp.OperatingSystemLayer.PathSep;
            var targetDirBasePath = SlurperApp.OperatingSystemLayer.TargetDirBasePath;

            return $"{targetDirBasePath}{sep}{targetRelativePath}{sep}";
        }
    }
}
