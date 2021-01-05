/* Copyright 2019-2020 Junruoyu Zheng. All rights reserved.

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;

namespace Ligral.Simulation
{
    class State
    {
        public double StateVariable;
        public double InitialValue;
        public double Derivative;
        public static List<State> StatePool = new List<State>();
        public string Name;
        public static State CreateState(double init, string name=null)
        {
            State state = new State(init);
            StatePool.Add(state);
            state.Name = name??$"State{StatePool.Count}";
            return state;
        }
        private State(double init)
        {
            InitialValue = init;
            StateVariable = init;
        }
    }
}