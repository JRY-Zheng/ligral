/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using System.Linq;
using Ligral.Component;

namespace Ligral.Simulation
{
    public class State
    {
        public double StateVariable;
        public double InitialValue;
        public double Derivative;
        public double StateUpperBound = double.PositiveInfinity;
        public double StateLowerBound = double.NegativeInfinity;
        public double DerivativeUpperBound = double.PositiveInfinity;
        public double DerivativeLowerBound = double.NegativeInfinity;
        public bool IsConstrained {get; set;} = false;
        public bool IsDerivativeConstrained {get; set;} = false;
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
        public static string GetNames()
        {
            return $"[{string.Join(", ", StatePool.Select((state, index)=>state.Name))}]";
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
        public void SetDerivativeUpperBound(Signal signal)
        {
            SetSignal(signal, (state, deriv) => state.DerivativeUpperBound = deriv);
        }
        public void SetDerivativeLowerBound(Signal signal)
        {
            SetSignal(signal, (state, deriv) => state.DerivativeLowerBound = deriv);
        }
        public void SetStateVariable(Signal signal)
        {
            SetSignal(signal, (state, var) => state.StateVariable = var);
        }
        public void SetStateUpperBound(Signal signal)
        {
            SetSignal(signal, (state, var) => state.StateUpperBound = var);
        }
        public void SetStateLowerBound(Signal signal)
        {
            SetSignal(signal, (state, var) => state.StateLowerBound = var);
        }
        public void SetStateConstrain(Signal stateSignal)
        {
            SetSignal(stateSignal, (state, var) => 
            {
                if (var == 1)
                {
                    state.IsConstrained = true;
                }
                else if (var == 0)
                {
                    state.IsConstrained = false;
                }
                else
                {
                    throw logger.Error(new LigralException($"Constrain should be either 0 or 1, but {var} got."));
                }
            });
        }
        public void SetDerivativeConstrain(Signal stateSignal)
        {
            SetSignal(stateSignal, (state, var) => 
            {
                if (var == 1)
                {
                    state.IsDerivativeConstrained = true;
                }
                else if (var == 0)
                {
                    state.IsDerivativeConstrained = false;
                }
                else
                {
                    throw logger.Error(new LigralException($"Constrain should be either 0 or 1, but {var} got."));
                }
            });
        }
        public Signal GetState()
        {
            return GetSignal(state => state.StateVariable);
        }
    }
}