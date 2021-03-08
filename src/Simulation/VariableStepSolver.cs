/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Component;

namespace Ligral.Simulation
{
    public class VariableStepSolver : Solver
    {
        protected static double hMin = 1e-5;
        protected static double hMax = 0.1;
        protected static double h0 = 0.01;
        protected static double tDir = 1;
        protected static double xNormMin = 1e-5;
        protected static double rTol = 1e-3;
        protected static double errPower = 1/5;
        protected static double stopTime = 10;
        static VariableStepSolver()
        {
            Settings settings = Settings.GetInstance();
            if (settings.RealTimeSimulation)
            {
                throw solverLogger.Error(new SettingException("realtime", true, "Cannot run the project in realtime when a variable step solver is utilised"));
            }
            h0 = settings.StepSize;
            stopTime = settings.StopTime;
            if (settings.VariableStepSolverConfiguration is Dictionary<string, object> dict)
            foreach (string item in dict.Keys)
            {
                object val = dict[item];
                try
                {
                    switch (item.ToLower())
                    {
                    case "hmin":
                    case "min_step":
                        hMin = System.Convert.ToDouble(val);
                        break;
                    case "hmax":
                    case "max_step":
                        hMax = System.Convert.ToDouble(val);
                        break;
                    case "tdir":
                    case "step_factor":
                        tDir = System.Convert.ToDouble(val);
                        break;
                    case "min_x_norm":
                    case "min_x":
                        xNormMin = System.Convert.ToDouble(val);
                        break;
                    case "rtol":
                    case "relative_tolerant":
                        rTol = System.Convert.ToDouble(val);
                        break;
                    case "err_pow":
                    case "err_index":
                        errPower = System.Convert.ToDouble(val);
                        break;
                    default:
                        throw solverLogger.Error(new SettingException(item, val, "Unsupported setting in variable step solver."));
                    }
                }
                catch (System.InvalidCastException)
                {
                    throw solverLogger.Error(new SettingException(item, val, $"Invalid type {val.GetType()} in variable step solver."));
                }
            }
        }
        public override void Solve(Problem problem)
        {
            Solver.OnStarting();
            States = problem.InitialValues();
            Solver.Time = 0;
            var f0 = problem.SystemDynamicFunction(States);
            problem.ObservationFunction();
            Solver.OnStepped(Time);
            double hAbs = h0;
            bool stopTimeReached = false;
            while (!stopTimeReached)
            {
                double tOld = Solver.Time;
                hAbs = hAbs.UpperBound(hMax).LowerBound(hMin);
                double h = tDir * hAbs;
                double tRemain = stopTime - Solver.Time;
                if (1.1*hAbs > tRemain)
                {
                    h = hAbs = tRemain;
                    stopTimeReached = true;
                }
                bool noFailsInStep = true;
                double xNorm = States.L2Norm();
                double err = 0;
                Matrix<double> xNew;
                double xNewNorm;
                Matrix<double> fp;
                while (true)
                {
                    var f = StepDerivatives(problem, h, States, f0);
                    xNew = StepStates(h, States, f);
                    double tNew = Solver.Time = stopTimeReached ? stopTime : tOld + h;
                    fp = problem.SystemDynamicFunction(xNew);
                    var fe = CalculateError(f.Append(fp));
                    xNewNorm = xNew.L2Norm();
                    err = hAbs * fe.L2Norm() / xNorm.LowerBound(xNewNorm, xNormMin);
                    if (err > rTol)
                    {
                        if (hAbs <= hMin)
                        {
                            throw logger.Error(new LigralException($"Integration tolerant not met at time {tOld} with step {h}"));
                        }
                        if (noFailsInStep)
                        {
                            noFailsInStep = false;
                            hAbs = (Math.Pow(0.8*(rTol/err), errPower).LowerBound(0.1) * hAbs).LowerBound(hMin);
                        }
                        else
                        {
                            hAbs = (0.5 * hAbs).LowerBound(hMin);
                        }
                        h = tDir * hAbs;
                        stopTimeReached = false;
                    }
                    else
                    {
                        break;
                    }
                }
                problem.ObservationFunction();
                Solver.OnStepped(Time);
                if (stopTimeReached)
                {
                    break;
                }
                if (noFailsInStep)
                {
                    hAbs *= 1.25 * Math.Pow(err/rTol, errPower).LowerBound(0.2);
                }
                States = xNew;
                xNorm = xNewNorm;
                f0 = fp;
            }
            Solver.OnStopped();
        }
        protected virtual Matrix<double> StepDerivatives(Problem problem, double stepSize, Matrix<double> states, Matrix<double> f0)
        {
            throw new System.NotImplementedException();
        }
        protected virtual Matrix<double> StepStates(double stepSize, Matrix<double> states, Matrix<double> stateDerivatives)
        {
            throw new System.NotImplementedException();
        }
        protected virtual Matrix<double> CalculateError(Matrix<double> stateDerivatives)
        {
            throw new System.NotImplementedException();
        }
    }
}