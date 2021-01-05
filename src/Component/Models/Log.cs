/* Copyright (C) 2019-2020 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Component.Parameter>;
using System;
using Ligral.Component;

namespace Ligral.Component.Models
{
    class Log : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model calculates y=log(x)/log(base), where default base is e.";
            }
        }
        private double newBase;
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("x", this));
            OutPortList.Add(new OutPort("y", this));
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"base", new Parameter(ParameterType.Signal , value=>
                {
                    newBase = (double)value;
                }, ()=>
                {
                    newBase = Math.E;
                })}
            };
        }
        protected override List<Signal> DefaultCalculate(List<Signal> values)
        {
            // Results.Clear();
            // Results.Add(Math.Log(values[0], newBase));
            Signal inputSignal = values[0];
            Signal outputSignal = Results[0];
            outputSignal.Clone(inputSignal.Apply((item) => Math.Log(item, newBase)));
            return Results;
        }
    }
}