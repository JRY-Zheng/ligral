using System.Collections.Generic;
using System;

namespace Ligral.Models
{
    class Asinh : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model calculates y=asinh(x).";
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
            // Results.Add(Math.Asinh(values[0]));
            Signal inputSignal = values[0];
            Signal outputSignal = Results[0];
            outputSignal.Clone(inputSignal.Apply(Math.Asinh));
            return Results;
        }
    }
}