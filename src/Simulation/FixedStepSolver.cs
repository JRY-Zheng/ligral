using System.Collections.Generic;
using System;
using MathNet.Numerics.LinearAlgebra;

namespace Ligral.Simulation
{
    class Timer
    {
        private DateTime start;
        public double Interval {get; set;}
        public void Start()
        {
            start = DateTime.Now;
        }
        public void Wait()
        {
            while((DateTime.Now - start).TotalSeconds < Interval);
        }
    }
    class FixedStepSolver : Solver
    {
        public override void Solve(Problem problem)
        {
            Solver.OnStarting();
            Settings settings = Settings.GetInstance();
            States = problem.InitialValues();
            if (settings.RealTimeSimulation)
            {
                bool keepRunning = true;
                Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e)
                {
                    e.Cancel = true;
                    keepRunning = false;
                    logger.Warn("User stop the simulation before the demanded stop time.");
                };
                Timer timer = new Timer(){ Interval=settings.StepSize};
                DateTime StartTime = DateTime.Now;
                Time = 0;
                DateTime lastTime = StartTime;
                DateTime thisTime;
                double stepSize = settings.StepSize;
                double actualStepSize = stepSize;
                while (keepRunning && Time < settings.StopTime)
                {
                    timer.Start();
                    States = Step(problem, actualStepSize, States);
                    Solver.OnStepped();
                    thisTime = DateTime.Now;
                    actualStepSize = (thisTime - lastTime).TotalSeconds;
                    logger.Debug($"Calculation comsumed {timer.Interval} second.");
                    if (actualStepSize > stepSize)
                    {
                        logger.Warn($"Calculation consumes more time ({actualStepSize}) than the demanded step size.");
                        timer.Interval = 2 * actualStepSize - stepSize;
                        stepSize = actualStepSize;
                    }
                    else
                    {
                        if (actualStepSize < settings.StepSize)
                        {
                            stepSize = settings.StepSize;
                        }
                        if (timer.Interval > stepSize)
                        {
                            timer.Interval = stepSize;
                        }
                        timer.Wait();
                        thisTime = DateTime.Now;
                        actualStepSize = (thisTime - lastTime).TotalSeconds;
                    }
                    logger.Debug($"Time interval is {timer.Interval} second");
                    lastTime = thisTime;
                    Time = (thisTime - StartTime).TotalSeconds;
                    timer.Start();
                }
            }
            else
            {
                DateTime LastTime = DateTime.Now;
                for (Time=0; Time<=settings.StopTime; Time+=settings.StepSize)
                {
                    States = Step(problem, settings.StepSize, States);
                    Solver.OnStepped();
                    logger.Debug($"Calculation comsumed {(DateTime.Now - LastTime).TotalSeconds} seconds.");
                    LastTime = DateTime.Now;
                }
            }
            Solver.OnStopped();
            logger.Info("Simulation stoped.");
        }
        protected virtual Matrix<double> Step(Problem problem, double stepSize, Matrix<double> states)
        {
            throw new System.NotImplementedException();
        }
    }
}