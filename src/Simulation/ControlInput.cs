/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;

namespace Ligral.Simulation
{
    public class ControlInput
    {
        public bool IsOpenLoop {get; set;} = false;
        public double OpenLoopInput {set => openLoopInput = value;}
        public double ClosedLoopInput {set => closedLoopInput = value;}
        private double openLoopInput;
        private double closedLoopInput;
        public string Name {get; private set;}
        public double Input
        {
            get
            {
                if (IsOpenLoop)
                {
                    return openLoopInput;
                }
                else
                {
                    return closedLoopInput;
                }
            }
        }
        public static List<ControlInput> InputPool = new List<ControlInput>();
        public static ControlInput CreateInput(string name=null)
        {
            ControlInput controlInput = new ControlInput();
            InputPool.Add(controlInput);
            controlInput.Name = name ?? "Input"+InputPool.Count;
            return controlInput;
        }
        private ControlInput() {}
    }
}