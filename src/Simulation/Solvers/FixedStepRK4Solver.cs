/* Copyright 2019-2020 Junruoyu Zheng. All rights reserved.

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;

namespace Ligral.Simulation.Solvers
{
    class FixedStepRK4Solver : FixedStepSolver
    {
        protected override Matrix<double> Step(Problem problem, double stepSize, Matrix<double> states)
        {
            double tn = Time;
            var c1 = problem.SystemDynamicFunction(states) * stepSize;
            problem.ObservationFunction();
            Time = tn + stepSize/2;
            var c2 = problem.SystemDynamicFunction(states + c1/2) * stepSize;
            var c3 = problem.SystemDynamicFunction(states + c2/2) * stepSize;
            Time = tn + stepSize;
            var c4 = problem.SystemDynamicFunction(states + c3) * stepSize;
            // clear calculation cache for output
            Time = tn;
            problem.SystemDynamicFunction(states);
            return states + (c1+2*c2+2*c3+c4)/6;
        }
    }
}