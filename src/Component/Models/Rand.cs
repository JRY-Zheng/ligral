using System.Collections.Generic;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Component.Parameter>;
using System;
using Ligral.Component;

namespace Ligral.Component.Models
{
    class Rand : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model generates random value in given range.";
            }
        }
        private Random random;
        private int seed;
        private double upper;
        private double lower;
        protected override void SetUpPorts()
        {
            OutPortList.Add(new OutPort("output", this));
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"seed", new Parameter(ParameterType.Signal , value=>
                {
                    seed = Convert.ToInt32(value);
                    random = new Random(seed);
                }, ()=>
                {
                    seed = System.DateTime.Now.Millisecond;
                    random = new Random(seed);
                })},
                {"upper", new Parameter(ParameterType.Signal , value=>
                {
                    upper = (double)value;
                }, ()=>
                {
                    upper = 1;
                })},
                {"lower", new Parameter(ParameterType.Signal , value=>
                {
                    lower = (double)value;
                }, ()=>
                {
                    lower = 0;
                })},
            };
        }
        protected override List<Signal> DefaultCalculate(List<Signal> values)
        {
            // Results.Clear();
            Signal outputSignal = Results[0];
            outputSignal.Pack(random.NextDouble() * (upper - lower) + lower);
            return Results;
        }
    }
}