/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using MathNet.Numerics.LinearAlgebra;
using Ligral.Component;

namespace Ligral.Simulation.Solvers
{
    class VarStepODE45Solver : VariableStepSolver
    {
        private double[] A = {1/5, 3/10, 4/5, 8/9};
        private double[,] B = 
        {
            {1/5, 3/40, 44/45, 19372/6561, 9017/3168},
            {0, 9/40, -56/15, -25360/2187, -355/33},
            {0, 0, 32/9, 64448/6561, 46732/5247},
            {0, 0, 0, -212/729, 49/176},
            {0, 0, 0, 0, -5103/18656},
        };
        private Matrix<double> C = Matrix<double>.Build.DenseOfColumnMajor(6, 1, new double[]{35/384, 0, 500/1113, 125/192, -2187/6784, 11/84});
        private Matrix<double> E = Matrix<double>.Build.DenseOfColumnMajor(7, 1, new double[]{71/57600, 0, -71/16695, 71/1920, -17253/339200, 22/525, -1/40});
        protected override Matrix<double> StepDerivatives(Problem problem, double stepSize, Matrix<double> states, Matrix<double> f0)
        {
            double ts0 = Time;
            var x1 = states + stepSize * (B[0,0] * f0);
            Time = ts0 + stepSize * A[0];
            var f1 = problem.SystemDynamicFunction(x1);
            var x2 = states + stepSize * (B[0,1] * f0 + B[1,1] * f1);
            Time = ts0 + stepSize * A[1];
            var f2 = problem.SystemDynamicFunction(x2);
            var x3 = states + stepSize * (B[0,2] * f0 + B[1,2] * f1 + B[2,2] * f2);
            Time = ts0 + stepSize * A[2];
            var f3 = problem.SystemDynamicFunction(x3);
            var x4 = states + stepSize * (B[0,3] * f0 + B[1,3] * f1 + B[2,3] * f2 + B[3,3] * f3);
            Time = ts0 + stepSize * A[3];
            var f4 = problem.SystemDynamicFunction(x4);
            var x5 = states + stepSize * (B[0,4] * f0 + B[1,4] * f1 + B[2,4] * f2 + B[3,4] * f3 + B[4,4] * f4);
            Time = ts0 + stepSize;
            var f5 = problem.SystemDynamicFunction(x5);
            return SignalUtils.Append(f0, f1, f2, f3, f4, f5);
        }
        protected override Matrix<double> StepStates(double stepSize, Matrix<double> states, Matrix<double> stateDerivatives)
        {
            return states + stepSize * (stateDerivatives * C);
        }
        protected override Matrix<double> CalculateError(Matrix<double> stateDerivatives)
        {
            return stateDerivatives * E;
        }
    }
}