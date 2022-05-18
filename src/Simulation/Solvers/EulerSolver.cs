/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

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