namespace Slurper
{
    public interface ILogger
    {
        void Log(string Message, logLevel Level);
        void Log(string Message);
    }
    public enum logLevel
    {
        TRACE,
        LOG,
        VERBOSE,
        WARN,
        ERROR
    }

}