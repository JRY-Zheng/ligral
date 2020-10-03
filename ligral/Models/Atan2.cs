using System.Collections.Generic;
using System;

namespace Ligral.Models
{
    class Atan2 : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model calculates y=atan(sin/cos).";
            }
        }
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("sin", this));
            InPortList.Add(new InPort("cos", this));
            OutPortList.Add(new OutPort("y", this));
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            // Results.Clear();
            // Results.Add(Math.Atan2(values[0], values[1]));
            Signal ySignal = values[0];
            Signal xSignal = values[1];
            Signal outputSignal = Results[0];
            outputSignal.Clone(ySignal.ZipApply(xSignal, Math.Atan2));
            return Results;
        }
    }
}