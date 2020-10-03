using System.Collections.Generic;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Parameter>;
using System;
using MathNet.Numerics.LinearAlgebra;

namespace Ligral.Models
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
            Parameters["delay"] = new Parameter(value =>
            {
                int delayedFrame = Convert.ToInt32(value);
                if (delayedFrame < 1)
                {
                    throw new ModelException(this, "Delay should be greater than 1");
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
            stack.RemoveAt(0);
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            // Results.Clear();
            Signal inputSignal = values[0];
            Signal outputSignal = Results[0];
            Signal stackTop = stack[0];
            stack.Remove(stackTop);
            stackTop.Clone(inputSignal);
            stack.Add(stackTop);
            Signal newStackTop = stack[0];
            outputSignal.Clone(newStackTop);
            return Results;
        }
    }
}