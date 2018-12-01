
namespace Slurper
{
    public interface ILogger
    {
        void Log(string Message, LogLevel Level);
        void Log(string Message);
    }
    public enum LogLevel
    {
        TRACE,
        LOG,
        VERBOSE,
        WARN,
        ERROR
    }
}