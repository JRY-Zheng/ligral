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
            for (Time=settings.StepSize; Time<=settings.StopTime; Time+=settings.StepSize)
            {
                List<double> stateDerivatives = problem.SystemDynamicFunction(states);
                states = states.Zip(stateDerivatives).ToList().ConvertAll(pair => 
                {
                    var state = pair.First;
                    var deriv = pair.Second;
                    state += deriv*(Time - lastTime);
                    return state;
                });
                lastTime = Time;
            }
        }
    }
}