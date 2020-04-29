
using Slurper.Contracts;
using Slurper.Output;

namespace Slurper.Providers
{
   public static class LogProvider
    {
        public static ILogger Logger { get; } = new ConsoleLogger();
    }
}
