using System.Collections.Generic;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Component.Parameter>;
using Ligral.Component;

namespace Ligral.Component.Models
{
    class ThresholdSwitch : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model accepts one condition and two values, returns first if the condition reaches threshold otherwise second.";
            }
        }
        private double threshold;
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("condition", this));
            InPortList.Add(new InPort("first", this));
            InPortList.Add(new InPort("second", this));
            OutPortList.Add(new OutPort("result", this));
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"threshold", new Parameter(value=>
                {
                    threshold = (double)value;
                }, ()=>
                {
                    threshold = 0;
                })}
            };
        }
        protected override List<Signal> DefaultCalculate(List<Signal> values)
        {
            // Results.Clear();
            Signal conditionSignal = values[0].Apply(item => item >= threshold ? 1 : 0);
            Signal firstSignal = values[1];
            Signal secondSignal = values[2];
            Signal resultSignal = Results[0];
            resultSignal.Clone(conditionSignal & firstSignal + (1 - conditionSignal) & secondSignal);
            return Results;
        }
    }
}