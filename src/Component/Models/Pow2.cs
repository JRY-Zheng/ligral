using System.Collections.Generic;
using System;
using Ligral.Component;

namespace Ligral.Component.Models
{
    class Pow2 : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model calculates y=base^index.";
            }
        }
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("base", this));
            InPortList.Add(new InPort("index", this));
            OutPortList.Add(new OutPort("y", this));
        }
        protected override List<Signal> DefaultCalculate(List<Signal> values)
        {
            // Results.Clear();
            // Results.Add(Math.Pow(values[0], values[1]));
            Signal baseSignal = values[0];
            Signal indexSignal = values[1];
            Signal outputSignal = Results[0];
            outputSignal.Clone(baseSignal.ZipApply(indexSignal, Math.Pow));
            return Results;
        }
    }
}