/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using System.IO;
using Ligral.Tools;

namespace Ligral.Simulation
{
    public class Observation
    {
        public static List<double> TimeList = new List<double>();
        public List<double> ObservationList = new List<double>();
        public double OutputVariable {get; private set;}
        private bool isCommitted = true;
        public string Name;
        public static string DataFile;
        private Logger loggerInstance;
        public delegate void SteppedHandler();
        public static event SteppedHandler Stepped;
        public delegate void StoppedHandler();
        public static event StoppedHandler Stopped;

        static Observation()
        {
            Solver.Stepped += OnStepped;
            Solver.Stopped += OnStopped;
        }
        protected Logger logger
        {
            get
            {
                if (loggerInstance is null)
                {
                    loggerInstance = new Logger(Name);
                }
                return loggerInstance;
            }
        }
        public static List<(string, Observation)> ObservationPool = new List<(string, Observation)>();
        public static Observation CreateObservation(string name = null)
        {
            Observation observation = new Observation();
            observation.Name = name??$"{ObservationPool.Count}";
            if (ObservationPool.Exists(item => item.Item1 == observation.Name))
            {
                // throw logger.Error(new LigralException($"Observation name conflict: {observation.Name}"));
                // [TODO] add log system to warn override
                return ObservationPool.Find(item => item.Item1 == observation.Name).Item2;
            }
            else
            {
                ObservationPool.Add((observation.Name, observation));
                return observation;
            }
        }
        private Observation() {}
        public void Cache(double value)
        {
            isCommitted = false;
            OutputVariable = value;
        }
        public void Commit()
        {
            if (!isCommitted)
            {
                ObservationList.Add(OutputVariable);
                isCommitted = true;
            }
            else
            {
                throw logger.Error(new LigralException($"Duplicate commits in Observation {Name}"));
            }
        }
        public static void OnStepped()
        {
            ObservationPool.ForEach(item => item.Item2.Commit());
            TimeList.Add(Solver.Time);
            if (Stepped != null) Stepped(); 
        }
        public static void OnStopped()
        {
            if (ObservationPool.Count > 0)
            {
                Storage table = new Storage();
                table.AddColumn("Time", Observation.TimeList);
                foreach ((string name, var observation) in ObservationPool)
                {
                    table.AddColumn(name, observation.ObservationList);
                }
                Settings settings = Settings.GetInstance();
                if (!Directory.Exists(settings.OutputFolder))
                {
                    Directory.CreateDirectory(settings.OutputFolder);
                }
                string currentDirectory = System.IO.Directory.GetCurrentDirectory();
                DataFile = Path.Join(currentDirectory, settings.OutputFolder, "Data.csv");
                table.DumpFile(DataFile, true);
            }
            if (Stopped != null) Stopped();
        }
    }
}