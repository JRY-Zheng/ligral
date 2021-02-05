/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;

namespace Ligral.Simulation.Solvers
{
    class FixedStepRK5Solver : FixedStepSolver
    {
        protected override Matrix<double> Step(Problem problem, double stepSize, Matrix<double> states)
        {
            double tn = Time;
            var c1 = problem.SystemDynamicFunction(states) * stepSize;
            problem.ObservationFunction();
            Time = tn + stepSize/5;
            var c2 = problem.SystemDynamicFunction(states + c1/5) * stepSize;
            Time = tn + stepSize*3/10;
            var c3 = problem.SystemDynamicFunction(states + c1*3/40 + c2*9/40) * stepSize;
            Time = tn + stepSize*4/5;
            var c4 = problem.SystemDynamicFunction(states + c1*44/45 - c2*56/15 + c3*32/9) * stepSize;
            Time = tn + stepSize*8/9;
            var c5 = problem.SystemDynamicFunction(states + c1*19372/6561 - c2*25360/2187 + c3*64448/6561 - c4*212/729) * stepSize;
            Time = tn + stepSize;
            var c6 = problem.SystemDynamicFunction(states + c1*9017/3168 - c2*355/33 + c3*46732/5247 + c4*49/176 - c5*5103/18656) * stepSize;
            return states + c1*35/384 + c3*500/1113 + c4*125/192 - c5*2187/6784 + c6*11/84;
        }
    }
}