using MathNet.Numerics.LinearAlgebra;
using Ligral.Simulation.Solvers;

namespace Ligral.Simulation
{
    abstract class Solver
    {
        public static double Time;
        public Matrix<double> States;
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
        public static Solver GetSolver(string solverName)
        {
            switch (solverName)
            {
            case "ode1":
            case "euler":
                return new EulerSolver();
            case "ode4":
                return new FixedStepRK4Solver();
            default:
                throw new SettingException("solver", solverName, "No such solver.");
            }
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