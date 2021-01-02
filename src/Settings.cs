using System.IO;
using System.Collections.Generic;
using Ligral.Syntax;
using Ligral.Syntax.ASTs;

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
        { }

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
        private double stepSize = 0.01;
        public double StepSize 
        { 
            get { return stepSize;} 
            set 
            { 
                if (value <= 0)
                {
                    throw logger.Error(new SettingException("step_size", value, "step size must be positive nonzero."));
                }
                else if (value > stopTime)
                {
                    throw logger.Error(new SettingException("step_size", value, "step size must be small than stop time."));
                }
                stepSize = value;
            }
        }
        private double stopTime = 10;
        public double StopTime 
        { 
            get { return stopTime;} 
            set 
            { 
                if (value <= 0)
                {
                    throw logger.Error(new SettingException("stop_time", value, "step size must be positive nonzero."));
                }
                else if (value < stepSize)
                {
                    throw logger.Error(new SettingException("stop_time", value, "step size must be larger than step size."));
                }
                stopTime = value;
            }
        }
        private string outputFolder = ".";
        public string OutputFolder { get { return outputFolder;} set { outputFolder = value;}}
        private bool needOutput = false;
        public bool NeedOutput 
        {
            get
            {
                return needOutput;
            }
            set
            {
                if (value)
                {
                    if (!Directory.Exists(OutputFolder))
                    {
                        Directory.CreateDirectory(OutputFolder);
                    }
                }
                needOutput = value;
            }
        }

        public string IPAddress { get => iPAddress; set => iPAddress = value; }
        public int Port { get => port; set => port = value; }
        public bool RealTimeSimulation { get => realTimeSimulation; set => realTimeSimulation = value; }
        private Tools.Plotter plotter;
        public Dictionary<string, object> InnerPlotterConfiguration 
        { 
            get => innerPlotterConfiguration; 
            set  
            {
                innerPlotterConfiguration = value;
                if (plotter is null)
                {
                    plotter = new Tools.Plotter();
                }
                plotter.Configure(value);
            }
        }

        public string PythonExecutable { get => pythonExecutable; set => pythonExecutable = value; }

        private string pythonExecutable = "python";

        private Dictionary<string, object> innerPlotterConfiguration;
        private string iPAddress = "127.0.0.1";
        private int port = 8783;
        private bool realTimeSimulation = false;

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
    }
}