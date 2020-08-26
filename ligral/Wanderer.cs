using System.Collections.Generic;

namespace Ligral 
{
    class Wanderer
    {
        private double stepSize = 0.01;
        private double stopTime = 100;
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
            for (double time=stepSize; time<=stopTime; time+=stepSize)
            {
                foreach(Model node in routine)
                {
                    node.Update(time);
                    node.Propagate();
                }
            }
            foreach(Model node in routine)
            {
                node.Release();
            }
        }
    }
}