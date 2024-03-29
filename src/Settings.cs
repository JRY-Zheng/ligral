/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.IO;
using System.Linq;
using System.Collections.Generic;
using Ligral.Syntax;
using Ligral.Syntax.ASTs;
using Ligral.Simulation;

namespace Ligral
{
    interface IConfigurable
    {
        void Configure(Dictionary<string, object> dict);
    }
    public class Settings
    {
        #region Singleton
        private static Settings settingsInstance;
        private static object locker = new object();
        private Logger logger = new Logger("Settings");
        private Settings()
        {
            // Solver.Starting += ApplySetting;
        }

        public static Settings GetInstance()
        {
            if (settingsInstance==null)
            {
                lock (locker)
                {
                    if (settingsInstance==null)
                    {
                        settingsInstance = new Settings();
                    }
                }
            }
            return settingsInstance;
        }
        #endregion

        public void GetDefaultSettings()
        {
            string executingPath = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            string defaultSetting = Path.Join(executingPath, "default.lig");
            if (File.Exists(defaultSetting))
            {
                string text = File.ReadAllText(defaultSetting);
                Parser parser = new Parser();
                parser.Load(text, 0);
                ProgramAST p = parser.Parse();
                p.Statements.Statements.RemoveAll(ast => !(ast is ConfAST));
                Interpreter interpreter = Interpreter.GetInstance(defaultSetting);
                interpreter.Interpret(p);
            }
            // ApplySetting();
        }
        
        #region Settings
        public double StepSize {get; set;} = 0.01;
        public double StopTime {get; set;} = 10;
        public string OutputFolder { get; set;} = ".";
        public bool NeedOutput {get; set;} = false;
        public string IPAddress { get; set; } = "127.0.0.1";
        public int ListeningPort { get; set; } = 8783;
        public int SendingPort { get; set; } = 8784;
        public bool RealTimeSimulation { get; set; } = false;
        private Tools.Plotter plotter;
        public Dictionary<string, object> InnerPlotterConfiguration {get; set;} = new Dictionary<string, object>();
        public Dictionary<string, object> LoggerConfiguration {get; set;} = new Dictionary<string, object>();
        public string SolverName {get; set;} = "ode4";
        public string PythonExecutable { get; set; } = "python";
        public Dictionary<string, object> LinearizerConfiguration {get; set;} = new Dictionary<string, object>();
        public Dictionary<string, object> TrimmerConfiguration {get; set;} = new Dictionary<string, object>();
        public Dictionary<string, object> VariableStepSolverConfiguration {get; set;} = new Dictionary<string, object>();
        public void UnionDictionarySetting(string settingName, Dictionary<string, object> origin, Dictionary<string, object> newSetting)
        {
            foreach ((var k, var v) in newSetting)
            {
                if (origin.ContainsKey(k))
                {
                    logger.Warn($"Key overridden in {settingName}: {k}");
                }
                origin[k] = v;
            }
        }

        public void AddSetting(string item, object val)
        {
            try
            {
                switch (item.ToLower())
                {
                case "step_size":
                    StepSize = (double) val; break;
                case "stop_time":
                    StopTime = (double) val; break;
                case "output_folder":
                    OutputFolder = (string) val; break;
                case "ip_address":
                    IPAddress = (string) val; break;
                case "port":
                    ListeningPort = (int) val;
                    SendingPort = ListeningPort+1; break;
                case "listening_port":
                    ListeningPort = (int) val; break;
                case "sending_port":
                    SendingPort = (int) val; break;
                case "realtime":
                    RealTimeSimulation = (bool) val; break;
                case "python":
                    PythonExecutable = (string) val; break;
                case "solver":
                    SolverName = (string) val; break;
                case "inner_plotter":
                    UnionDictionarySetting(item.ToLower(), InnerPlotterConfiguration, (Dictionary<string, object>) val);
                    break;
                case "logger":
                    UnionDictionarySetting(item.ToLower(), LoggerConfiguration, (Dictionary<string, object>) val);
                    break;
                case "lin":
                case "linearizer":
                    UnionDictionarySetting(item.ToLower(), LinearizerConfiguration, (Dictionary<string, object>) val);
                    break;
                case "trim":
                case "trimmer":
                    UnionDictionarySetting(item.ToLower(), TrimmerConfiguration, (Dictionary<string, object>) val);
                    break;
                case "varstep":
                case "varstep_solver":
                    UnionDictionarySetting(item.ToLower(), VariableStepSolverConfiguration, (Dictionary<string, object>) val);
                    break;
                case "author":
                case "date":
                case "license":
                case "email":
                case "home_page":
                case "homepage":
                    break;
                default:
                    throw logger.Error(new SettingException(item, val, "Unsupported setting."));
                }
            }
            catch (System.InvalidCastException)
            {
                throw logger.Error(new SettingException(item, val, $"Invalid type {val.GetType()}"));
            }
        }
        #endregion
        public bool HasSettings(Dictionary<string, object> setting)
        {
            return setting != null && setting.Count > 0;
        }

        public void ApplySetting()
        {
            if (StepSize <= 0)
            {
                throw logger.Error(new SettingException("step_size", StepSize, "step size must be positive nonzero."));
            }
            if (StepSize > StopTime)
            {
                throw logger.Error(new SettingException("step_size", StepSize, $"step size must be small than stop time {StopTime}."));
            }
            if (StopTime <= 0)
            {
                throw logger.Error(new SettingException("stop_time", StopTime, "step size must be positive nonzero."));
            }
            if (HasSettings(InnerPlotterConfiguration))
            {
                if (plotter is null)
                {
                    if (RealTimeSimulation)
                    {
                        plotter = new Tools.RTPlotter();
                    }
                    else
                    {
                        plotter = new Tools.Plotter();
                    }
                }
                logger.Debug("Plotter is instantiated");
                plotter.Configure(InnerPlotterConfiguration);
                logger.Debug("Plotter is configured");
            }
            if (HasSettings(LoggerConfiguration))
            {
                logger.Configure(LoggerConfiguration);
            }
        }
    }
}