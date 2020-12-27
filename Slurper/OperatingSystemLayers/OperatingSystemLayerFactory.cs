using System;
using Microsoft.Extensions.Logging;

namespace Slurper.OperatingSystemLayers
{
    public static class OperatingSystemLayerFactory
    {
        public static IOperatingSystemLayer Create()
        {
            IOperatingSystemLayer operatingSystemLayer;

            var platformId = Environment.OSVersion.Platform;

            switch (platformId)
            {
                case PlatformID.Win32NT:
                    operatingSystemLayer =
                        new OperatingSystemLayerWindows(new Logger<OperatingSystemLayerWindows>(new LoggerFactory()));
                    break;
                case PlatformID.Unix:
                    operatingSystemLayer =
                        new OperatingSystemLayerLinux(new Logger<OperatingSystemLayerLinux>(new LoggerFactory()));
                    break;
                case PlatformID.MacOSX:
                    operatingSystemLayer =
                        new OperatingSystemLayerLinux(new Logger<OperatingSystemLayerLinux>(new LoggerFactory()));
                    break;
                default:
                    Console.WriteLine($"This [{platformId}] OS and/or its filesystem is not supported");
                    throw new NotSupportedException();
            }

            return operatingSystemLayer;
        }
    }
}