/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using MathNet.Numerics.LinearAlgebra;
using Ligral.Simulation.Solvers;
using Ligral.Extension;

namespace Ligral.Simulation
{
    public abstract class Solver
    {
        public static double Time;
        public Matrix<double> States;
        protected Logger logger;
        private static Logger solverLogger = new Logger("Solver");
        public delegate void StartingHandler();
        public static event StartingHandler Starting;
        public delegate void SteppedHandler();
        public static event SteppedHandler Stepped;
        public delegate void StoppedHandler();
        public static event StoppedHandler Stopped;
        public Solver()
        {
            logger = new Logger(GetType().Name);
        }
        public static Solver GetSolver(string solverName)
        {
            switch (solverName)
            {
            case "ode1":
            case "euler":
                return new EulerSolver();
            case "ode2":
                return new FixedStepRK2Solver();
            case "ode3":
                return new FixedStepRK3Solver();
            case "ode4":
                return new FixedStepRK4Solver();
            case "ode5":
                return new FixedStepRK5Solver();
            case "ode8":
                return new FixedStepRK8Solver();
            case "ode1be":
                return new BackwardEulerSolver();
            case "ode45":
                return new VarStepODE45Solver();
            default:
                if (solverName.Contains('.'))
                {
                    int sep = solverName.IndexOf('.');
                    string pluginName = solverName.Substring(0, sep);
                    solverName = solverName.Substring(sep+1);
                    return PluginManager.GetSolver(solverName, pluginName);
                }
                else
                {
                    return PluginManager.GetSolver(solverName);
                }
                // throw new SettingException("solver", solverName, "No such solver.");
            }
        }
        public abstract void Solve(Problem problem);
        public static void OnStarting()
        {
            if (Starting != null) Starting();
            solverLogger.Info("Simulation started.");
        }
        public static void OnStepped(double time)
        {
            Time = time;
            if (Stepped != null) Stepped();
        }
        public static void OnStopped()
        {
            if (Stopped != null) Stopped();
            solverLogger.Info("Simulation stoped.");
        }
    }
}