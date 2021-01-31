/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using Ligral.Component;

namespace Ligral.Simulation
{
    public class State
    {
        public double StateVariable;
        public double InitialValue;
        public double Derivative;
        private static Logger logger = new Logger("State");
        public static List<State> StatePool = new List<State>();
        public static Dictionary<string, StateHandle> StateHandles = new Dictionary<string, StateHandle>();
        public string Name;
        public static State CreateState(string name)
        {
            name = name??$"State{StatePool.Count}";
            if (StatePool.Exists(state => state.Name == name))
            {
                throw logger.Error(new LigralException($"State {name} has already existed."));
            }
            else
            {
                State state = new State(name);
                StatePool.Add(state);
                return state;
            }
        }
        public static StateHandle CreateState(string name, int rowNo, int colNo, Signal initial)
        {
            name = name??$"State{StatePool.Count}";
            if (StateHandles.ContainsKey(name))
            {
                throw logger.Error(new LigralException($"State handle {name} has already existed."));
            }
            else
            {
                var handle = new StateHandle(name, rowNo, colNo, initial);
                StateHandles.Add(name, handle);
                return handle;
            }
        }
        private State(string name) 
        {
            Name = name;
        }
    }
    public class StateHandle : Handle<State>
    {
        public StateHandle(string name, int rowNo, int colNo, Signal initialSignal) : 
        base(name, rowNo, colNo, name => State.CreateState(name)) 
        {
            SetSignal(initialSignal, (state, initial) => 
            {
                state.InitialValue = initial;
                state.StateVariable = initial;
            });
        }

        public void SetDerivative(Signal signal)
        {
            SetSignal(signal, (state, deriv) => state.Derivative = deriv);
        }
        public void SetStateVariable(Signal signal)
        {
            SetSignal(signal, (state, var) => state.StateVariable = var);
        }
        public Signal GetState()
        {
            return GetSignal(state => state.StateVariable);
        }
    }
}