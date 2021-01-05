/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using System;
using Ligral.Component;

namespace Ligral.Component.Models
{
    class Log2 : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model calculates y=log(x)/log(base).";
            }
        }
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("x", this));
            InPortList.Add(new InPort("base", this));
            OutPortList.Add(new OutPort("y", this));
        }
        protected override List<Signal> DefaultCalculate(List<Signal> values)
        {
            // Results.Clear();
            // Results.Add(Math.Log(values[0], values[1]));
            Signal xSignal = values[0];
            Signal baseSignal = values[1];
            Signal outputSignal = Results[0];
            outputSignal.Clone(xSignal.ZipApply(baseSignal, Math.Log));
            return Results;
        }
    }
}