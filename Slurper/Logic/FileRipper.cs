using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Slurper.OperatingSystemLayers;


namespace Slurper.Logic
{
    public class FileRipper
    {
        private readonly ILogger<FileRipper> _logger;
        private readonly IOperatingSystemLayer _operatingSystemLayer;

        public FileRipper(ILogger<FileRipper> logger)
        {
            _logger = logger;
            _operatingSystemLayer = OperatingSystemLayerFactory.Create();
        }

        public async Task RipFile(string filename)
        {
            var targetPath = TargetPath(filename);
            var targetFileNameFullPath = targetPath + Path.GetFileName(filename);

            _logger.LogDebug("RipFile: ripping [{Filename}] => [{TargetFileNameFullPath}]", filename, targetFileNameFullPath);

            if (ConfigurationService.DryRun) return;

            try
            {
                Directory.CreateDirectory(targetPath);
                await Task.Run(() => File.Copy(filename, targetFileNameFullPath));
            }
            catch (Exception e)
            {
                _logger.LogError("RipFile: copy of [{Filename}] failed with [{ExceptionMessage}]", filename, e.Message);
            }
        }

        private string TargetPath(string filename)
        {
            try
            {
                var relativePath = Path.GetDirectoryName(filename);

                relativePath = _operatingSystemLayer.SanitizePath(relativePath);

                var sep = _operatingSystemLayer.PathSep;
                var targetDirBasePath = _operatingSystemLayer.TargetDirBasePath;

                return $"{targetDirBasePath}{sep}{relativePath}{sep}";
            } catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}\n");
                throw;
            }
        }
    }
}
