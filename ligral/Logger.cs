namespace Ligral
{
    enum LogLevel
    {
        DEBUG,INFO,WARNING,ERROR
    }
    static class Logger
    {
        public static LogLevel Level = LogLevel.DEBUG;
        private static void Log(LogLevel level, string message)
        {
            if (level>=Level)
            {
                System.Console.WriteLine($"{level.ToString(),8}: {message}");
            }
        }
        public static void Debug(string message)
        {
            Log(LogLevel.DEBUG, message);
        }
        public static void Info(string message)
        {
            Log(LogLevel.INFO, message);
        }
        public static void Warn(string message)
        {
            Log(LogLevel.WARNING, message);
        }
        public static void Error(string message)
        {
            Log(LogLevel.ERROR, message);
        }
    }
}