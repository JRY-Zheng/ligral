using System.Collections.Generic;
using Ligral.Tools;

namespace Ligral.Simulation
{
    class Observation
    {
        public static List<double> TimeList = new List<double>();
        public List<double> ObservationList = new List<double>();
        public double OutputVariable {get; private set;}
        private bool isCommitted = true;
        public string Name;
        public delegate void SteppedHandler();
        public static event SteppedHandler Stepped;
        public delegate void StoppedHandler();
        public static event StoppedHandler Stopped;
        public static List<(string, Observation)> ObservationPool = new List<(string, Observation)>();
        public static Observation CreateObservation(string name = null)
        {
            Observation observation = new Observation();
            observation.Name = name??$"{ObservationPool.Count}";
            if (ObservationPool.Exists(item => item.Item1 == observation.Name))
            {
                throw new LigralException($"Observation name conflict: {observation.Name}");
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
                throw new LigralException($"Duplicate commits in Observation {Name}");
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
                CsvTable table = new CsvTable();
                table.AddColumn("Time", Observation.TimeList);
                foreach ((string name, var observation) in ObservationPool)
                {
                    table.AddColumn(name, observation.ObservationList);
                }
                Settings settings = Settings.GetInstance();
                settings.NeedOutput = true;
                string currentDirectory = System.IO.Directory.GetCurrentDirectory();
                string dataFile = System.IO.Path.Join(currentDirectory, settings.OutputFolder, "Data.csv");
                table.DumpFile(dataFile, true);
            }
            if (Stopped != null) Stopped();
        }
    }
}