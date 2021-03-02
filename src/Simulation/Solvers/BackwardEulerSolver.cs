/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using MathNet.Numerics.LinearAlgebra;

namespace Ligral.Simulation.Solvers
{
    class BackwardEulerSolver : FixedStepSolver
    {
        private Matrix<double> x0;
        private double h;
        private Problem problem;
        private Optimizer optimizer;
        private string optimizerName = "sqp";
        public BackwardEulerSolver(): base()
        {
            Solver.Starting += Initialize;
        }
        private void Initialize()
        {
            x0 = State.StatePool.ConvertAll(state => state.StateVariable).ToColumnVector();
            optimizer = Optimizer.GetOptimizer(optimizerName);
        }
        private Matrix<double> Cost()
        {
            return x0;
        }
        private Matrix<double> Equal(Matrix<double> x)
        {
            Matrix<double> xdot = problem.SystemDynamicFunction(x);
            return x - x0 - xdot*h;
        }
        private Matrix<double> Bound(Matrix<double> x)
        {
            var build = Matrix<double>.Build;
            return build.Dense(0, 1);
        }
        protected override Matrix<double> Step(Problem problem, double stepSize, Matrix<double> states)
        {
            this.problem = problem;
            h = stepSize;
            problem.SystemDynamicFunction(states);
            problem.ObservationFunction();
            var x0p = Matrix<double>.Build.DenseOfMatrix(x0);
            x0 = optimizer.Optimize(Cost, x0p, Equal, Bound);
            return x0;
        }
    }
}