namespace Slurper.Contracts
{
    public interface ILogger
    {
        void Log(string message, LogLevel level);
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