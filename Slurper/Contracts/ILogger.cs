namespace Slurper
{
    public interface ILogger
    {
        void Log(string Message, logLevel Level = logLevel.LOG);
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