using System.Collections.Generic;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Parameter>;
using System;

namespace Ligral.Models
{
    class Log : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model calculates y=log(x)/log(base), where default base is e.";
            }
        }
        private double newBase;
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("x", this));
            OutPortList.Add(new OutPort("y", this));
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"base", new Parameter(value=>
                {
                    newBase = (double)value;
                }, ()=>
                {
                    newBase = Math.E;
                })}
            };
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            // Results.Clear();
            // Results.Add(Math.Log(values[0], newBase));
            Signal inputSignal = values[0];
            Signal outputSignal = Results[0];
            outputSignal.Clone(inputSignal.Apply((item) => Math.Log(item, newBase)));
            return Results;
        }
    }
}