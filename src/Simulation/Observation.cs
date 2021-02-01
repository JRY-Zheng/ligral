/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ligral.Tools;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Component;

namespace Ligral.Simulation
{
    public class Observation
    {
        public static List<double> TimeList = new List<double>();
        public List<double> ObservationList = new List<double>();
        public double OutputVariable {get; private set;}
        private double cachedOutputVariable;
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
        static Logger logger = new Logger("Observation");
        public static List<Observation> ObservationPool = new List<Observation>();
        public static Dictionary<string, ObservationHandle> ObservationHandles = new Dictionary<string, ObservationHandle>();
        public static Observation CreateObservation(string name)
        {
            name = name??$"Output{ObservationPool.Count}";
            if (ObservationPool.Exists(obs => obs.Name == name))
            {
                logger.Warn($@"The signal {name} is logged more than once, but only a single output is stored. 
Make sure you did not log two different signals under the same name.");
                // This statement should not be error:
                //     x -> Print; x -> Scope;
                // We just print x and scope x, only one observation handle will be create.
                // However, be careful about this case:
                //     x -> Print; y -> Scope{name:'x'};
                // Still only one observation handle will be create, so unexpected behaviour will occur.
                return ObservationPool.Find(obs => obs.Name == name);
            }
            else
            {
                Observation observation = new Observation(name);
                ObservationPool.Add(observation);
                return observation;
            }
        }
        public static ObservationHandle CreateObservation(string name, int rowNo, int colNo)
        {
            name = name??$"Output{ObservationPool.Count}";
            ObservationHandle handle;
            if (ObservationHandles.ContainsKey(name))
            {
                handle = ObservationHandles[name];
                if (handle.rowNo != rowNo && handle.colNo != colNo)
                {
                    throw logger.Error(new LigralException($"Two different signals are stored under the same name {name}"));
                }
                else
                {
                    logger.Warn($@"The signal {name} is logged more than once, but only a single output handle is generated. 
Make sure you did not log two different signals under the same name.");
                }
            }
            else
            {
                handle = new ObservationHandle(name, rowNo, colNo);
                ObservationHandles.Add(name, handle);
            }
            return handle;
        }
        public static string GetNames()
        {
            return $"[{string.Join(", ", ObservationPool.Select((obs, index)=>obs.Name))}]";
        }
        private Observation(string name) 
        {
            Name = name;
            loggerInstance = new Logger(name);
        }
        public void Cache(double value)
        {
            isCommitted = false;
            cachedOutputVariable = value;
        }
        public void Commit()
        {
            if (!isCommitted)
            {
                OutputVariable = cachedOutputVariable;
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
            if (Stepped != null) Stepped(); 
        }
        public static void OnStopped()
        {
            if (ObservationPool.Count > 0)
            {
                Storage table = new Storage();
                table.AddColumn("Time", Observation.TimeList);
                foreach (var observation in ObservationPool)
                {
                    table.AddColumn(observation.Name, observation.ObservationList);
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

    public class ObservationHandle : Handle<Observation>
    {
        public ObservationHandle(string name, int rowNo, int colNo) : base(name, rowNo, colNo, Observation.CreateObservation)
        { }
        public void Cache(Signal signal)
        {
            SetSignal(signal, (observation, value) => observation.Cache(value));
        }
        public Signal GetObservation()
        {
            return GetSignal(observation => observation.OutputVariable);
        }
    }
}