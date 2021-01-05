/* Copyright 2019-2020 Junruoyu Zheng. All rights reserved.

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

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
        Prompt = 3,
        Error = 4,
        Fatal = 5
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
    class Logger : IConfigurable
    {
        public static DateTime StartUpTime = DateTime.Now;
        public static List<LogMessage> Logs = new List<LogMessage>();
        private string source;
        public static bool PrintOut = true;
        public static bool PrintOutPlainText = true;
        public static LogLevel MinPrintOutLevel = LogLevel.Warning;
        public static string LogFile = null;
        public static LogLevel MinLogFileLevel = LogLevel.Info;
        private static ConsoleColor defaultForeGroundColor = Console.ForegroundColor;
        private static ConsoleColor defaultBackGroundColor = Console.BackgroundColor;
        public Logger(string source)
        {
            this.source = source;
        }
        private void Log(LogMessage message)
        {
            Logs.Add(message);
            if (PrintOut && message.Level >= MinPrintOutLevel)
            {
                TextWriter textWriter = message.Level == LogLevel.Error ? Console.Error : Console.Out;
                if (PrintOutPlainText)
                {
                    textWriter.WriteLine(message.Message);
                }
                else
                {
                    textWriter.WriteLine(message);
                }
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
            Console.ForegroundColor = defaultForeGroundColor;
        }
        public void Prompt(string message)
        {
            Log(new LogMessage(source, LogLevel.Prompt, message));
        }
        public LigralException Error(LigralException exception)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Log(new LogMessage(source, LogLevel.Error, exception.ToString()));
            Console.ForegroundColor = defaultForeGroundColor;
            return exception;
        }
        public void Fatal(string message, Exception exception)
        {
            Console.BackgroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.White;
            Log(new LogMessage(source, LogLevel.Fatal, message + ": " + exception.ToString()));
            Console.ForegroundColor = defaultForeGroundColor;
            Console.BackgroundColor = defaultBackGroundColor;
        }

        public void Configure(Dictionary<string, object> dict)
        {
            foreach (string item in dict.Keys)
            {
                object val = dict[item];
                try
                {
                    switch (item.ToLower())
                    {
                    case "print_out":
                        PrintOut = (bool) val; break;
                    case "min_print_out_level":
                        if (val is string minPrintOutLevelName)
                        {
                            MinPrintOutLevel = ConvertToLogLevel(minPrintOutLevelName);
                        }
                        else
                        {
                            MinPrintOutLevel = (LogLevel) Convert.ToInt32(val);
                        }
                        break;
                    case "min_log_file_level":
                        if (val is string minLogFileLevelName)
                        {
                            MinLogFileLevel = ConvertToLogLevel(minLogFileLevelName);
                        }
                        else
                        {
                            MinLogFileLevel = (LogLevel) Convert.ToInt32(val);
                        }
                        break;
                    case "print_out_plain_text":
                        PrintOutPlainText = (bool) val; break;
                    case "log_file":
                        LogFile = (string) val; break;
                    default:
                        throw new SettingException(item, val, "Unsupported setting in plotter.");
                    }
                }
                catch (System.InvalidCastException)
                {
                    throw Error(new SettingException(item, val, $"Invalid type {val.GetType()} in plotter"));
                }
            }
        }
        private LogLevel ConvertToLogLevel(string logName)
        {
            switch (logName.ToLower())
            {
            case "debug":
                return LogLevel.Debug;
            case "info":
                return LogLevel.Info;
            case "warn":
            case "warning":
                return LogLevel.Warning;
            case "error":
                return LogLevel.Debug;
            default:
                throw Error(new SettingException("log_level", logName, "Unknown log level"));
            }
        }
    }
}