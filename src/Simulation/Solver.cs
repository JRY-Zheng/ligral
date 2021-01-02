using System.Collections.Generic;

namespace Ligral.Simulation
{
    abstract class Solver
    {
        public static double Time;
        public List<double> States;
        public List<double> Outputs;
        protected Logger logger;
        public Solver()
        {
            logger = new Logger(GetType().Name);
        }
        public abstract void Solve(Problem problem);
    }
}