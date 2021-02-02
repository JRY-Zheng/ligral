/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using MathNet.Numerics.LinearAlgebra;
using Ligral.Simulation.Solvers;
using Ligral.Extension;
using MFunc = System.Func<MathNet.Numerics.LinearAlgebra.Matrix<double>, MathNet.Numerics.LinearAlgebra.Matrix<double>>;

namespace Ligral.Simulation
{
    public abstract class Optimizer
    {
        protected Logger logger;
        public Optimizer()
        {
            logger = new Logger(GetType().Name);
        }
        public static Optimizer GetOptimizer(string optimizerName)
        {
            switch (optimizerName.ToLower())
            {
            case "sqp":
            default:
                // if (solverName.Contains('.'))
                // {
                //     int sep = solverName.IndexOf('.');
                //     string pluginName = solverName.Substring(0, sep);
                //     solverName = solverName.Substring(sep+1);
                //     return PluginManager.GetSolver(solverName, pluginName);
                // }
                // else
                // {
                //     return PluginManager.GetSolver(solverName);
                // }
                throw new SettingException("optimizer", optimizerName, "No such optimizer.");
            }
        }
        public abstract Matrix<double> Optimize(MFunc cost, Matrix<double> x0, MFunc equal, MFunc bound);
    }
}