using System.Collections.Generic;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Component.Parameter>;
using Ligral.Component;
using Ligral.Simulation;

namespace Ligral.Component.Models
{
    class Step : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model provides a step between 0 and a definable level at a specified time.";
            }
        }
        private double start = 0;
        private double level = 1;
        protected override void SetUpPorts()
        {
            OutPortList.Add(new OutPort("source", this));
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"start", new Parameter(value=>
                {
                    start = (double)value;
                }, ()=>
                {
                    start = 0;
                })},
                {"level", new Parameter(value=>
                {
                    level = (double)value;
                }, ()=>
                {
                    level = 1;
                })},
            };
        }
        protected override List<Signal> DefaultCalculate(List<Signal> values)
        {
            // Results.Clear();
            Signal outputSignal = Results[0];
            if (Solver.Time >= start)
            {
                outputSignal.Pack(level);
            }
            else
            {
                outputSignal.Pack(0);
            }
            return Results;
        }
    }
}