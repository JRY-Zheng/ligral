/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;

namespace Ligral.Simulation.Solvers
{
    class FixedStepRK8Solver : FixedStepSolver
    {
        protected override Matrix<double> Step(Problem problem, double stepSize, Matrix<double> states)
        {
            double tn = Time;
            var c1 = problem.SystemDynamicFunction(states) * stepSize;
            problem.ObservationFunction();
            Time = tn + stepSize*5.555555555555556E-2;
            var c2 = problem.SystemDynamicFunction(states + c1*5.555555555555556E-2) * stepSize;
            Time = tn + stepSize*8.333333333333333E-2;
            var c3 = problem.SystemDynamicFunction(states + c1*2.083333333333333E-2 + c2*6.25E-2) * stepSize;
            Time = tn + stepSize*1.25E-1;
            var c4 = problem.SystemDynamicFunction(states + c1*3.125E-2 + c3*9.375E-2) * stepSize;
            Time = tn + stepSize*3.125E-1;
            var c5 = problem.SystemDynamicFunction(states + c1*3.125E-1 - c3*1.171875 + c4*1.171875) * stepSize;
            Time = tn + stepSize*3.75E-1;
            var c6 = problem.SystemDynamicFunction(states + c1*3.75E-2 + c4*1.875E-1 + c5*1.5E-1) * stepSize;
            Time = tn + stepSize*1.475E-1;
            var c7 = problem.SystemDynamicFunction(states + c1*4.791013711111111E-2 + c4*1.122487127777778E-1 - c5*2.550567377777778E-2 + c6*1.284682388888889E-2) * stepSize;
            Time = tn + stepSize*4.65E-1;
            var c8 = problem.SystemDynamicFunction(states + c1*1.691798978729228E-2 + c4*3.878482784860432E-1 + c5*3.597736985150033E-2 + c6*1.969702142156661E-1 - c7*1.727138523405018E-1) * stepSize;
            Time = tn + stepSize*5.648654513822596E-1;
            var c9 = problem.SystemDynamicFunction(states + c1*6.90957533591923E-2 - c4*6.342479767288542E-1 - c5*1.611975752246041E-1 + c6*1.386503094588253E-1 + c7*9.409286140357563E-1 + c8*2.11636326481944E-1) * stepSize;
            Time = tn + stepSize*6.5E-1;
            var c10 = problem.SystemDynamicFunction(states + c1*1.835569968390454E-1 - c4*2.468768084315592 - c5*2.912868878163005E-1 - c6*2.647302023311738E-2 + c7*2.8478387641928 + c8*2.813873314698498E-1 + c9*1.237448998633147E-1) * stepSize;
            Time = tn + stepSize*9.246562776405044E-1;
            var c11 = problem.SystemDynamicFunction(states - c1*1.215424817395888 + c4*1.667260866594577E1 + c5*9.157418284168179E-1 - c6*6.056605804357471 - c7*1.600357359415618E1 + c8*1.484930308629766E1 - c9*1.337157573528985E1 + c10*5.134182648179638) * stepSize;
            Time = tn + stepSize;
            var c12 = problem.SystemDynamicFunction(states + c1*2.588609164382643E-1 - c4*4.774485785489205 - c5*4.350930137770325E-1 - c6*3.049483332072241 + c7*5.577920039936099 + c8*6.155831589861039 - c9*5.062104586736938 + c10*2.193926173180679 + c11*1.346279986593349E-1) * stepSize;
            return states + c1*8.224275996265075E-1 - c4*1.165867325727766E1 - c5*7.576221166909362E-1 + c6*7.139735881595818E-1 + c7*1.207577498689006E1 - c8*2.127659113920403 + c9*1.990166207048956 - c10*2.342864715440405E-1 + c11*1.758985777079423E-1;
        }
    }
}