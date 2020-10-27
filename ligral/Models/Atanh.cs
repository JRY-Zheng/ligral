using System.Collections.Generic;
using System;
using Ligral.Block;

namespace Ligral.Models
{
    class Atanh : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model calculates y=atanh(x).";
            }
        }
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("x", this));
            OutPortList.Add(new OutPort("y", this));
        }
        protected override List<Signal> DefaultCalculate(List<Signal> values)
        {
            // Results.Clear();
            // Results.Add(Math.Atanh(values[0]));
            Signal inputSignal = values[0];
            Signal outputSignal = Results[0];
            outputSignal.Clone(inputSignal.Apply(Math.Atanh));
            return Results;
        }
    }
}