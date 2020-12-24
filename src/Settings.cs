using System.IO;
using Ligral.Syntax;
using Ligral.Syntax.ASTs;

namespace Ligral
{
    class Settings
    {
        #region Singleton
        private static Settings settingsInstance;
        private static object locker = new object();
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
                    throw new SettingException("step_size", value, "step size must be positive nonzero.");
                }
                else if (value > stopTime)
                {
                    throw new SettingException("step_size", value, "step size must be small than stop time.");
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
                    throw new SettingException("stop_time", value, "step size must be positive nonzero.");
                }
                else if (value < stepSize)
                {
                    throw new SettingException("stop_time", value, "step size must be larger than step size.");
                }
                stopTime = value;
            }
        }
        private string outputFolder = null;
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
        public bool RealTimeDrawing { get => realTimeDrawing; set => realTimeDrawing = value; }

        private string iPAddress = "127.0.0.1";
        private int port = 8783;
        private bool realTimeDrawing = false;

        public void AddSetting(string item, object val)
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
            case "realtime_draw":
                RealTimeDrawing = (bool) val; break;
            default:
                throw new LigralException($"Unsupported setting {item}");
            }
        }
        #endregion
    }
}