/* Copyright (C) 2019-2020 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using System;
using Ligral.Component;

namespace Ligral.Component.Models
{
    class Atan2 : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model calculates y=atan(sin/cos).";
            }
        }
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("sin", this));
            InPortList.Add(new InPort("cos", this));
            OutPortList.Add(new OutPort("y", this));
        }
        protected override List<Signal> DefaultCalculate(List<Signal> values)
        {
            // Results.Clear();
            // Results.Add(Math.Atan2(values[0], values[1]));
            Signal ySignal = values[0];
            Signal xSignal = values[1];
            Signal outputSignal = Results[0];
            outputSignal.Clone(ySignal.ZipApply(xSignal, Math.Atan2));
            return Results;
        }
    }
}