using System.Collections.Generic;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Block.Parameter>;
using System;
using Ligral.Block;

namespace Ligral.Models
{
    class SineWave : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model outputs ampl*sin(omega*time+phi).";
            }
        }
        private double ampl = 1;
        private double omega = 1;
        private double phi = 0;
        protected override void SetUpPorts()
        {
            OutPortList.Add(new OutPort("source", this));
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"ampl", new Parameter(value=>
                {
                    ampl = (double)value;
                }, ()=>
                {
                    ampl = 1;
                })},
                {"omega", new Parameter(value=>
                {
                    omega = (double)value;
                }, ()=>
                {
                    omega = 1;
                })},
                {"phi", new Parameter(value=>
                {
                    phi = (double)value;
                }, ()=>
                {
                    phi = 0;
                })},
            };
        }
        protected override List<Signal> DefaultCalculate(List<Signal> values)
        {
            // Results.Clear();
            // Results.Add(ampl*Math.Sin(omega*time+phi));
            Signal outputSignal = Results[0];
            outputSignal.Pack(ampl * Math.Sin(omega * time + phi));
            return Results;
        }
    }
}