using System.Collections.Generic;
using System;
using Ligral.Block;

namespace Ligral.Models
{
    class Abs : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model calculates the absolute value.";
            }
        }
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("x", this));
            OutPortList.Add(new OutPort("y", this));
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            // Results.Clear();
            Signal inputSignal = values[0];
            Signal outputSignal = Results[0];
            outputSignal.Clone(inputSignal.Apply(Math.Abs));
            // Results.Add(Math.Abs(values[0]));
            return Results;
        }
    }
}