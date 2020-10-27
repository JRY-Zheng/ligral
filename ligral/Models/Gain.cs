using System.Collections.Generic;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Block.Parameter>;
using Ligral.Block;

namespace Ligral.Models
{
    class Gain : Model
    {
        private Signal gain;
        private bool leftProduct = false;
        protected override string DocString
        {
            get
            {
                return "This model amplifies the input by a given constant.";
            }
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"value", new Parameter(value=>
                {
                    gain = new Signal(value);
                })},
                {"prod", new Parameter(value=>
                {
                    string prod = (string) value;
                    switch (prod)
                    {
                        case "left":
                            leftProduct = true; break;
                        case "right":
                            leftProduct = false; break;
                        default:
                            throw new ModelException(this, $"Invalid enum prod {prod}");
                    }
                }, ()=>{})}
            };
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            // Results.Clear();
            Signal inputSignal = values[0];
            Signal outputSignal = Results[0];
            if (leftProduct)
                outputSignal.Clone(gain * inputSignal);
            else
                outputSignal.Clone(inputSignal * gain);
            // Results.Add(signal); // validation of input is done somewhere else
            return Results;
        }
    }
}