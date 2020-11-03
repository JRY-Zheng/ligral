using System.Collections.Generic;
using System.Linq;

namespace Ligral.Simulation.Solvers
{
    class EulerSolver : Solver
    {
        public override void Solve(Problem problem)
        {
            double lastTime = 0;
            List<double> states = problem.InitialValues();
            Settings settings = Settings.GetInstance();
            for (double time=settings.StepSize; time<=settings.StopTime; time+=settings.StepSize)
            {
                List<double> stateDerivatives = problem.SystemDynamicFunction(states);
                states = states.Zip(stateDerivatives).ToList().ConvertAll(pair => 
                {
                    var state = pair.First;
                    var deriv = pair.Second;
                    state += deriv*(time - lastTime);
                    return state;
                });
                lastTime = time;
            }
        }
    }
}