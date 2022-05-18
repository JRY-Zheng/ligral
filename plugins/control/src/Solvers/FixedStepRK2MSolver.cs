/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using MathNet.Numerics.LinearAlgebra;
using Ligral.Simulation;

namespace LigralPlugins.Control.Solvers
{
    class FixedStepRK2MSolver : FixedStepSolver
    {
        protected override Matrix<double> Step(Problem problem, double stepSize, Matrix<double> states)
        {
            double tn = Time;
            var c1 = problem.SystemDynamicFunction(states) * stepSize;
            problem.ObservationFunction();
            Time = tn + stepSize/2;
            var c2 = problem.SystemDynamicFunction(states + c1/2) * stepSize;
            return states + c2;
        }
    }
}