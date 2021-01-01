using System;
using System.IO;
using System.Collections.Generic;

namespace Ligral
{
    enum LogLevel
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3
    }
    struct LogMessage
    {
        public double Time;
        public string Source; 
        public LogLevel Level;
        public string Message;
        public LogMessage(string source, LogLevel level, string message)
        {
            Time = (DateTime.Now - Logger.StartUpTime).TotalSeconds;
            Source = source;
            Level = level;
            Message = message;
        }
        public override string ToString()
        {
            return string.Format("{0,-6:0.0000} {1,8} {2,10} {3}", Time, Source, '['+Level.ToString()+']', Message);
        }
    }
    class Logger
    {
        public static DateTime StartUpTime = DateTime.Now;
        public static List<LogMessage> Logs = new List<LogMessage>();
        private string source;
        public static bool PrintOut = true;
        public static LogLevel MinPrintOutLevel = LogLevel.Warning;
        public static string LogFile = null;
        public static LogLevel MinLogFileLevel = LogLevel.Info;
        private static ConsoleColor defaultColor = Console.ForegroundColor;
        public Logger(string source)
        {
            this.source = source;
        }
        private void Log(LogMessage message)
        {
            Logs.Add(message);
            if (PrintOut && message.Level >= MinPrintOutLevel)
            {
                Console.WriteLine(message);
            }
            if (LogFile is string logFile && message.Level >= MinLogFileLevel)
            {
                try
                {
                    File.AppendAllText(logFile, message+"\n");
                }
                catch (Exception e)
                {
                    Logger logger = new Logger("Logger");
                    LogFile = null;
                    PrintOut = true;
                    throw logger.Error(new LigralException($"Cannot log to file {logFile}: {e.Message??"No message avalable"}"));
                }
            }
        }
        public void Debug(string message)
        {
            Log(new LogMessage(source, LogLevel.Debug, message));
        }
        public void Info(string message)
        {
            Log(new LogMessage(source, LogLevel.Info, message));
        }
        public void Warn(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Log(new LogMessage(source, LogLevel.Warning, message));
            Console.ForegroundColor = defaultColor;
        }
        public LigralException Error(LigralException exception)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Log(new LogMessage(source, LogLevel.Error, exception.ToString()));
            Console.ForegroundColor = defaultColor;
            return exception;
        }
    }
}