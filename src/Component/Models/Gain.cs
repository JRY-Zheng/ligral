/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Component.Parameter>;
using Ligral.Component;

namespace Ligral.Component.Models
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
                {"value", new Parameter(ParameterType.Signal , value=>
                {
                    gain = new Signal(value);
                })},
                {"prod", new Parameter(ParameterType.String , value=>
                {
                    string prod = (string) value;
                    switch (prod)
                    {
                    case "left":
                        leftProduct = true; break;
                    case "right":
                        leftProduct = false; break;
                    default:
                        throw logger.Error(new ModelException(this, $"Invalid enum prod {prod}"));
                    }
                }, ()=>{})}
            };
        }
        protected override List<Signal> DefaultCalculate(List<Signal> values)
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