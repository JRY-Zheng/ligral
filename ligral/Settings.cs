using System.IO;

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
        private bool variableStep = false;
        public bool VariableStep { get { return variableStep;} set { variableStep = value;}}
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
        
        public void AddSetting(string item, object val)
        {
            switch (item)
            {
                case "step_size":
                    StepSize = (double) val;
                    VariableStep = false; break;
                case "stop_time":
                    StopTime = (double) val; break;
                case "variable_step":
                    VariableStep = (bool) val; break;
                case "output_folder":
                    OutputFolder = (string) val; break;
                default:
                    throw new LigralException($"Unsupported setting {item}");
            }
        }
        #endregion
    }
}