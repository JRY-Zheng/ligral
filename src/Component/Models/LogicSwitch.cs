/* Copyright 2019-2020 Junruoyu Zheng. All rights reserved.

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using Ligral.Component;

namespace Ligral.Component.Models
{
    class LogicSwitch : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model accepts one condition and two values, returns first if the condition is met (non-zero) otherwise second.";
            }
        }
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("condition", this));
            InPortList.Add(new InPort("first", this));
            InPortList.Add(new InPort("second", this));
            OutPortList.Add(new OutPort("result", this));
        }
        protected override List<Signal> DefaultCalculate(List<Signal> values)
        {
            // Results.Clear();
            Signal conditionSignal = values[0].Apply(item => item == 0 ? 0 : 1);
            Signal firstSignal = values[1];
            Signal secondSignal = values[2];
            Signal resultSignal = Results[0];
            resultSignal.Clone((conditionSignal & firstSignal) + ((1 - conditionSignal) & secondSignal));
            return Results;
        }
    }
}