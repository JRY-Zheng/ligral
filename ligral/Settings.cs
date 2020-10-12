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
        public double StepSize = 0.01;
        public double StopTime = 10;
        public bool VariableStep = false;
        public string OutputFolder = null;
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