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
        public static List<State> StatePool = new List<State>();
        public string Name;
        public static State CreateState(string name)
        {
            State state = new State();
            StatePool.Add(state);
            state.Name = name??$"State{StatePool.Count}";
            return state;
        }
        private State() {}
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
        public Signal GetState()
        {
            return GetSignal(state => state.StateVariable);
        }
    }
}