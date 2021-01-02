using System.Collections.Generic;
using System.Linq;

namespace Ligral.Simulation.Solvers
{
    class EulerSolver : FixedStepSolver
    {
        protected override List<double> Step(Problem problem, double stepSize, List<double> states)
        {
            List<double> stateDerivatives = problem.SystemDynamicFunction(states);
            return States.Zip(stateDerivatives).ToList().ConvertAll(pair => 
            {
                var state = pair.First;
                var deriv = pair.Second;
                state += deriv*stepSize;
                return state;
            });
        }
    }
}