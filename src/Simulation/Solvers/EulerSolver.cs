/* Copyright 2019-2020 Junruoyu Zheng. All rights reserved.

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using MathNet.Numerics.LinearAlgebra;

namespace Ligral.Simulation.Solvers
{
    class EulerSolver : FixedStepSolver
    {
        protected override Matrix<double> Step(Problem problem, double stepSize, Matrix<double> states)
        {
            Matrix<double> stateDerivatives = problem.SystemDynamicFunction(states);
            problem.ObservationFunction();
            return states + stateDerivatives * stepSize;
        }
    }
}