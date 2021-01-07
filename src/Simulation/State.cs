/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;

namespace Ligral.Simulation
{
    public class State
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