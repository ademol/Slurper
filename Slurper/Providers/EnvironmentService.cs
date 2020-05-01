using System;

namespace Slurper.Providers
{
    public class EnvironmentService
    {
        public PlatformID GetOsPlatform()
        {
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                    return PlatformID.Win32NT;
                case PlatformID.Unix:
                    return PlatformID.Unix;
                case PlatformID.MacOSX:
                    return PlatformID.MacOSX;
                default:
                    Console.WriteLine("This OS and/or its filesystem is not supported");
                    throw new NotSupportedException();
            }
        }
    }
}