using System.Collections.Generic;
using System;

namespace Ligral.Models
{
    class Sign : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model calculates y = 1 if x>0;-1 if x<0; 0 if x=0.";
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
            // Results.Add(Math.Sign(values[0]));
            Signal inputSignal = values[0];
            Signal outputSignal = Results[0];
            outputSignal.Clone(inputSignal.Apply((item) => Convert.ToDouble(Math.Sign(item))));
            return Results;
        }
    }
}