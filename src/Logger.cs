/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System;
using System.IO;
using System.Collections.Generic;
using Ligral.Component;

namespace Ligral
{
    public enum LogLevel
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Prompt = 3,
        Error = 4,
        Fatal = 5
    }
    public class LogMessage
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
        public void Solve()
        {
            if (Level == LogLevel.Error)
            {
                Level = LogLevel.Debug;
                Message = "Error is solved: " + Message;
            }
        }
    }
    public class Logger : IConfigurable
    {
        public static DateTime StartUpTime = DateTime.Now;
        public static List<LogMessage> Logs = new List<LogMessage>();
        private string source;
        private static string lastMessage = "";
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
        public static void OnExit()
        {
            if (LogFile is string logFile)
            {
                int i = 0;
                while (i < Logs.Count)
                // foreach (var message in Logs)
                {
                    i++;
                    var message = Logs[i];
                    if (message.Level >= MinLogFileLevel)
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
            }
        }
        private void Log(LogMessage message, bool cache = true)
        {
            if (cache) Logs.Add(message);
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
            // if (LogFile is string logFile && message.Level >= MinLogFileLevel)
            // {
            //     try
            //     {
            //         File.AppendAllText(logFile, message+"\n");
            //     }
            //     catch (Exception e)
            //     {
            //         Logger logger = new Logger("Logger");
            //         LogFile = null;
            //         PrintOut = true;
            //         throw logger.Error(new LigralException($"Cannot log to file {logFile}: {e.Message??"No message avalable"}"));
            //     }
            // }
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
            Logs.Add(new LogMessage(source, LogLevel.Error, exception.ToString()));
            return exception;
        }
        public void Solve()
        {
            foreach (var message in Logs.FindAll(msg => msg.Level == LogLevel.Error))
            {
                message.Solve();
            }
        }
        public void Throw()
        {
            foreach (var message in Logs.FindAll(msg => msg.Level == LogLevel.Error))
            {
                if (message.Message == lastMessage) continue;
                Console.ForegroundColor = ConsoleColor.Red;
                Log(message, false);
                Console.ForegroundColor = defaultForeGroundColor;
                lastMessage = message.Message;
            }
        }
        public void Fatal(Exception exception)
        {
            if (LogFile == null)
            {
                LogFile = "ligral.log";
            }
            MinLogFileLevel = LogLevel.Debug;
            MinPrintOutLevel = LogLevel.Fatal;
            foreach (var log in Logs)
            {
                Log(log, false);
            }
            Console.BackgroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.White;
            // Log(new LogMessage(source, LogLevel.Fatal, ""));
            Log(new LogMessage(source, LogLevel.Fatal, $"Fatal error occurs, details in log {Logger.LogFile}\n"+exception.ToString()));
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
                            MinPrintOutLevel = (LogLevel) val.ToInt();
                        }
                        break;
                    case "min_log_file_level":
                        if (val is string minLogFileLevelName)
                        {
                            MinLogFileLevel = ConvertToLogLevel(minLogFileLevelName);
                        }
                        else
                        {
                            MinLogFileLevel = (LogLevel) val.ToInt();
                        }
                        break;
                    case "print_out_plain_text":
                        PrintOutPlainText = (bool) val; break;
                    case "log_file":
                        LogFile = (string) val; break;
                    default:
                        throw Error(new SettingException(item, val, "Unsupported setting in logger."));
                    }
                }
                catch (System.InvalidCastException)
                {
                    throw Error(new SettingException(item, val, $"Invalid type {val.GetType()} in logger"));
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