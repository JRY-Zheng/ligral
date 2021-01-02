using System.Collections.Generic;
using Ligral.Component;
using System.Linq;

namespace Ligral.Simulation
{
    class Problem
    {
        private List<Model> routine;
        public Problem(List<Model> routine)
        {
            this.routine = routine;
        }
        public List<double> InitialValues()
        {
            return State.StatePool.ConvertAll(state => state.InitialValue);
        }
        public List<double> SystemDynamicFunction(List<double> states)
        {
            State.StatePool.Zip(states).ToList().ForEach(pair => 
            {
                var state = pair.First;
                var value = pair.Second;
                state.StateVariable = value;
            });
            foreach(Model node in routine)
            {
                node.Propagate();
            }
            return State.StatePool.ConvertAll(state => state.Derivative);
        }
        public List<double> ObservationFunction()
        {
            return Observation.ObservationPool.ConvertAll(item => item.Item2.OutputVariable);
        }
    }
}