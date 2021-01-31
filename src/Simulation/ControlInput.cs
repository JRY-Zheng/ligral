/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using Ligral.Component;

namespace Ligral.Simulation
{
    public class ControlInput
    {
        public static bool IsOpenLoop {get; set;} = false;
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
        public static Dictionary<string, ControlInputHandle> InputHandles = new Dictionary<string, ControlInputHandle>();
        private static Logger logger = new Logger("ControlInput");
        public static ControlInput CreateInput(string name)
        {
            name = name ?? "Input"+InputPool.Count;
            if (InputPool.Exists(input => input.Name == name))
            {
                throw logger.Error(new LigralException($"Control input {name} has already existed."));
            }
            else
            {
                ControlInput controlInput = new ControlInput(name);
                InputPool.Add(controlInput);
                return controlInput;
            }
        }
        public static ControlInputHandle CreateInput(string name, int rowNo, int colNo)
        {
            name = name??$"Input{InputPool.Count}";
            if (InputHandles.ContainsKey(name))
            {
                throw logger.Error(new LigralException($"Control input handle {name} has already existed."));
            }
            else
            {
                var handle = new ControlInputHandle(name, rowNo, colNo);
                InputHandles.Add(name, handle);
                return handle;
            }
        }
        private ControlInput(string name) 
        {
            Name = name;
        }
    }

    public class ControlInputHandle : Handle<ControlInput>
    {
        public ControlInputHandle(string name, int rowNo, int colNo) : base(name, rowNo, colNo, ControlInput.CreateInput)
        {}

        public void SetOpenLoopInput(Signal inputSignal)
        {
            SetSignal(inputSignal, (control, input) => control.OpenLoopInput = input);
        }

        public void SetClosedLoopInput(Signal inputSignal)
        {
            SetSignal(inputSignal, (control, input) => control.ClosedLoopInput = input);
        }
        public Signal GetInput()
        {
            return GetSignal(control => control.Input);
        }
    }
}