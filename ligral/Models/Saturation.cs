using System.Collections.Generic;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Block.Parameter>;
using Ligral.Block;

namespace Ligral.Models
{
    class Saturation : Model
    {
        private double upper = 1;
        private double lower = -1;
        protected override string DocString
        {
            get
            {
                return "This model produces an output signal that is the value of the input signal bounded to the upper and lower.";
            }
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"upper", new Parameter(value=>
                {
                    upper = (double)value;
                })},
                {"lower", new Parameter(value=>
                {
                    lower = (double)value;
                })},
            };
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            // Results.Clear();
            Signal inputSignal = values[0];
            Signal outputSignal = Results[0];
            outputSignal.Clone(inputSignal.Apply((item) =>
            {
                if (item < upper && item > lower)
                {
                    return item;
                }
                else if (item <= lower)
                {
                    return lower;
                }
                else //item>=upper
                {
                    return upper;
                }
            }));
            return Results;
        }
    }
}