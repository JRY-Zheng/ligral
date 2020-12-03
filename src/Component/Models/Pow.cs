using System.Collections.Generic;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Component.Parameter>;
using System;
using Ligral.Component;

namespace Ligral.Component.Models
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
                {"power", new Parameter(ParameterType.Signal , value=>
                {
                    power = (double)value;
                })}
            };
        }
        protected override List<Signal> DefaultCalculate(List<Signal> values)
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