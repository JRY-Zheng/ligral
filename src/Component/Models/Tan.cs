/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using System;
using Ligral.Component;

namespace Ligral.Component.Models
{
    class Tan : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model calculates y=tan(x).";
            }
        }
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("x", this));
            OutPortList.Add(new OutPort("y", this));
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            // Results.Clear();
            // Results.Add(Math.Tan(values[0]));
            Signal inputSignal = values[0];
            Signal outputSignal = Results[0];
            outputSignal.Clone(inputSignal.Apply(Math.Tan));
            return Results;
        }
    }
}