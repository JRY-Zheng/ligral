using System.Collections.Generic;

namespace Ligral.Simulation
{
    abstract class Solver
    {
        public static double Time;
        public List<double> States;
        public List<double> Outputs;
        protected Logger logger;
        public delegate void StartingHandler();
        public static event StartingHandler Starting;
        public delegate void SteppedHandler();
        public static event SteppedHandler Stepped;
        public delegate void StoppedHandler();
        public static event StoppedHandler Stopped;
        public Solver()
        {
            logger = new Logger(GetType().Name);
        }
        public abstract void Solve(Problem problem);
        public static void OnStarting()
        {
            if (Starting != null) Starting();
        }
        public static void OnStepped()
        {
            if (Stepped != null) Stepped();
        }
        public static void OnStopped()
        {
            if (Stopped != null) Stopped();
        }
    }
}