
using Slurper.Contracts;

namespace Slurper.Providers
{
    public static class ConfigurationProvider
    {
        public static IConfiguration Configuration { get; } = new Configuration();
    }
}
