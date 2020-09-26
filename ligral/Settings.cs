namespace Ligral
{
    class Settings
    {
        #region Singleton
        private static Settings settingsInstance;
        private static object locker = new object();
        private Settings()
        { }

        public Settings GetInstance()
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
        private double stepTime = 0.01;
        public double StepTime {get {return stepTime;}}
        private double stopTime = 10;
        public double StopTime {get {return stopTime;}}
        private bool variableStep = false;
        public bool VariableStep {get {return variableStep;}}
        
        public void AddSetting(string item, object val)
        {
            switch (item)
            {
                case "step_time":
                    stepTime = (double) val; break;
                case "stop_time":
                    stopTime = (double) val; break;
                case "variable_step":
                    variableStep = (bool) val; break;
            }
        }
        #endregion
    }
}