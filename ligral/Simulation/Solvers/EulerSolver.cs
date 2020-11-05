using System.Collections.Generic;
using System.Linq;

namespace Ligral.Simulation.Solvers
{
    class EulerSolver : Solver
    {
        public override void Solve(Problem problem)
        {
            List<double> states = problem.InitialValues();
            Settings settings = Settings.GetInstance();
            for (Time=0; Time<=settings.StopTime; Time+=settings.StepSize)
            {
                List<double> stateDerivatives = problem.SystemDynamicFunction(states);
                states = states.Zip(stateDerivatives).ToList().ConvertAll(pair => 
                {
                    var state = pair.First;
                    var deriv = pair.Second;
                    state += deriv*settings.StepSize;
                    return state;
                });
            }
        }
    }
}