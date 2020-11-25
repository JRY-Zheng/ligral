namespace Ligral.Simulation
{
    abstract class Solver
    {
        public static double Time;
        public abstract void Solve(Problem problem);
    }
}