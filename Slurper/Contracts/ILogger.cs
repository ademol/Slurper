
namespace Slurper.Contracts
{
    public interface ILogger
    {
        void Log(string message, LogLevel level);
        void Log(string message);
    }
    public enum LogLevel
    {
        Trace,
        Log,
        Verbose,
        Warn,
        Error
    }
}