/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Component;

namespace Ligral.Simulation
{
    public class VariableStepSolver : Solver
    {
        protected double hMin = 1e-5;
        protected double hMax = 0.1;
        protected double h0 = 0.01;
        protected double tDir = 1;
        protected double xNormMin = 1e-5;
        protected double rTol = 1e-3;
        protected double errPower = 1/5;
        public override void Solve(Problem problem)
        {
            Solver.OnStarting();
            Settings settings = Settings.GetInstance();
            States = problem.InitialValues();
            if (settings.RealTimeSimulation)
            {
                throw logger.Error(new SettingException("realtime", true, "Cannot run the project in realtime when a variable step solver is utilised"));
            }
            Solver.Time = 0;
            var f0 = problem.SystemDynamicFunction(States);
            problem.ObservationFunction();
            double hAbs = h0;
            bool stopTimeReached = false;
            while (!stopTimeReached)
            {
                double tOld = Solver.Time;
                hAbs = hAbs.UpperBound(hMax).LowerBound(hMin);
                double h = tDir * hAbs;
                double tRemain = settings.StopTime - Solver.Time;
                if (1.1*hAbs > tRemain)
                {
                    h = hAbs = tRemain;
                    stopTimeReached = true;
                }
                bool noFailsInStep = false;
                double xNorm = States.L2Norm();
                double err = 0;
                Matrix<double> xNew;
                double xNewNorm;
                Matrix<double> fp;
                while (true)
                {
                    var f = StepDerivatives(problem, h, States, f0);
                    xNew = StepStates(h, States, f);
                    double tNew = Solver.Time = stopTimeReached ? settings.StopTime : tOld + h;
                    fp = problem.SystemDynamicFunction(States);
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
                problem.ObservationFunction();
                Solver.OnStepped(Time);
            }
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