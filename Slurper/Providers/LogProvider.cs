
namespace Slurper.Providers
{
   public static class LogProvider
    {
        public static ILogger Logger { get; } = new ConsoleLogger();
    }
}
