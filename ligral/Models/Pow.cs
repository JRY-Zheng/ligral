using System.Collections.Generic;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Block.Parameter>;
using System;
using Ligral.Block;

namespace Ligral.Models
{
    class Pow : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model calculates y=x^power.";
            }
        }
        private double power;
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("x", this));
            OutPortList.Add(new OutPort("y", this));
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"power", new Parameter(value=>
                {
                    power = (double)value;
                })}
            };
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            // Results.Clear();
            // Results.Add(Math.Pow(values[0], power));
            Signal inputSignal = values[0];
            Signal outputSignal = Results[0];
            outputSignal.Clone(inputSignal.Apply((item) => Math.Pow(item, power)));
            return Results;
        }
    }
}