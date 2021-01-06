/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using MathNet.Numerics.LinearAlgebra;

namespace Ligral.Simulation.Solvers
{
    class FixedStepRK2Solver : FixedStepSolver
    {
        protected override Matrix<double> Step(Problem problem, double stepSize, Matrix<double> states)
        {
            double tn = Time;
            var c1 = problem.SystemDynamicFunction(states) * stepSize;
            problem.ObservationFunction();
            Time = tn + stepSize;
            var c2 = problem.SystemDynamicFunction(states + c1) * stepSize;
            // clear calculation cache for output
            Time = tn;
            problem.SystemDynamicFunction(states);
            return states + (c1+c2)/2;
        }
    }
}