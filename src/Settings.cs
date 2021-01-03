using System.IO;
using System.Collections.Generic;
using Ligral.Syntax;
using Ligral.Syntax.ASTs;
using Ligral.Simulation;

namespace Ligral
{
    interface IConfigure
    {
        void Configure(Dictionary<string, object> dict);
    }
    class Settings
    {
        #region Singleton
        private static Settings settingsInstance;
        private static object locker = new object();
        private Logger logger = new Logger("Settings");
        private Settings()
        {
            Solver.Starting += ApplySetting;
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
            string executingPath = Path.GetDirectoryName(typeof(Program).Assembly.Location);
            string defaultSetting = Path.Join(executingPath, "default.lig");
            if (File.Exists(defaultSetting))
            {
                string text = File.ReadAllText(defaultSetting);
                Parser parser = new Parser();
                parser.Load(text);
                ProgramAST p = parser.Parse();
                p.Statements.Statements.RemoveAll(ast => !(ast is ConfAST));
                Interpreter interpreter = Interpreter.GetInstance(defaultSetting);
                interpreter.Interpret(p);
            }
        }
        
        #region Settings
        public double StepSize {get; set;} = 0.01;
        public double StopTime {get; set;} = 10;
        public string OutputFolder { get; set;} = ".";
        public bool NeedOutput {get; set;} = false;
        public string IPAddress { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 8783;
        public bool RealTimeSimulation { get; set; } = false;
        private Tools.Plotter plotter;
        public Dictionary<string, object> InnerPlotterConfiguration {get; set;}

        public string PythonExecutable { get; set; } = "python";

        public void AddSetting(string item, object val)
        {
            try
            {
                switch (item)
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
                    Port = (int) val; break;
                case "realtime":
                    RealTimeSimulation = (bool) val; break;
                case "python":
                    PythonExecutable = (string) val; break;
                case "inner_plotter":
                    InnerPlotterConfiguration = (Dictionary<string, object>) val;
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

        private void ApplySetting()
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
            if (NeedOutput && !Directory.Exists(OutputFolder))
            {
                Directory.CreateDirectory(OutputFolder);
            }
            if (!(InnerPlotterConfiguration is null))
            {
                if (RealTimeSimulation)
                {
                    plotter = new Tools.RTPlotter();
                }
                else
                {
                    plotter = new Tools.Plotter();
                }
                plotter.Configure(InnerPlotterConfiguration);
            }
        }
    }
}