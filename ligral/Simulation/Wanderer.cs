using System.Collections.Generic;
using Ligral.Component;

namespace Ligral.Simulation
{
    class Wanderer
    {
        private double stepSize = 0.01;
        private double stopTime = 100;
        public static double Time = 0;
        public Wanderer()
        {
            Settings settings = Settings.GetInstance();
            Configure(settings.StepSize, settings.StopTime);
        }
        public void Configure(double stepSize, double stopTime)
        {
            if (stepSize>0 && stopTime>stepSize)
            {
                this.stepSize = stepSize;
                this.stopTime = stopTime;
            }
            else
            {
                throw new LigralException($"Configuration Error: Invalid step size {stepSize} or stop time {stopTime}");
            }
        }
        public void Wander(List<Model> routine)
        {
            for (Time=stepSize; Time<=stopTime; Time+=stepSize)
            {
                foreach(Model node in routine)
                {
                    node.Propagate();
                }
                foreach (State state in State.StatePool)
                {
                    state.EulerPropagate();
                }
            }
            foreach(Model node in routine)
            {
                node.Release();
            }
        }
    }
}