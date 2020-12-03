using System.Collections.Generic;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Component.Parameter>;
using Ligral.Component;

namespace Ligral.Component.Models
{
    class Deadzone : Model
    {
        private double left = -1;
        private double right = 1;
        protected override string DocString
        {
            get
            {
                return "This model generates zero output within a specified region [left, right].";
            }
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"left", new Parameter(ParameterType.Signal , value=>
                {
                    left = (double)value;
                })},
                {"right", new Parameter(ParameterType.Signal , value=>
                {
                    right = (double)value;
                })},
            };
        }
        protected override List<Signal> DefaultCalculate(List<Signal> values)
        {
            Signal inputSignal = values[0];
            Signal outputSignal = Results[0];
            outputSignal.Clone(inputSignal.Apply((item) =>
            {
                if (item < right && item > left)
                {
                    return 0;
                }
                else if (item <= left)
                {
                    return item - left;
                }
                else //item>=right
                {
                    return item - right;
                }
            }));
            return Results;
        }
    }
}