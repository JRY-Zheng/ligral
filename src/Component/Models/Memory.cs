/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Component.Parameter>;
using System;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Component;
using Ligral.Simulation;

namespace Ligral.Component.Models
{
    class Memory : InitializeableModel
    {
        protected override string DocString
        {
            get
            {
                return "This model outputs its input from the previous time step.";
            }
        }
        private List<Signal> stack = new List<Signal>();
        protected override void SetUpParameters()
        {
            base.SetUpParameters();
            Parameters["delay"] = new Parameter(ParameterType.Signal , value =>
            {
                int delayedFrame = Convert.ToInt32(value);
                if (delayedFrame < 1)
                {
                    throw logger.Error(new ModelException(this, "Delay should be greater than 1"));
                }
                for (int i = 0; i <= delayedFrame; i++)
                {
                    stack.Add(new Signal());
                }
            }, ()=>
            {
                stack.Add(new Signal());
                stack.Add(new Signal());
            });
        }
        protected override void AfterConfigured()
        {
            base.AfterConfigured();
            stack.ForEach(signal => signal.Clone(initial));
        }
        public override void Initialize()
        {
            base.Initialize();
            Signal inputSignal = new Signal();
            if (isMatrix)
            {
                MatrixBuilder<double> m = Matrix<double>.Build;
                Matrix<double> matrix = m.Dense(rowNo, colNo, 0);
                inputSignal.Pack(matrix);
            }
            else
            {
                inputSignal.Pack(0);
            }
            InPortList.FindAll(inPort=>!inPort.Visited).ForEach(inPort=>inPort.Input(inputSignal));
            stack.RemoveAt(0);
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            // Results.Clear();
            Signal inputSignal = values[0];
            Signal outputSignal = Results[0];
            if (!ControlInput.IsOpenLoop)
            {
                Signal stackTop = stack[0];
                stack.Remove(stackTop);
                stackTop.Clone(inputSignal);
                stack.Add(stackTop);
            }
            Signal newStackTop = stack[0];
            outputSignal.Clone(newStackTop);
            return Results;
        }
    }
}