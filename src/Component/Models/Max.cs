/* Copyright (C) 2019-2020 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using System.Linq;
using Ligral.Component;

namespace Ligral.Component.Models
{
    class Max : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model returns maximal value among inputs";
            }
        }
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("first", this));
            InPortList.Add(new InPort("second", this));
            OutPortList.Add(new OutPort("max", this));
        }
        protected override List<Signal> DefaultCalculate(List<Signal> values)
        {
            // Results.Clear();
            Results[0].Clone(values.Max());
            return Results;
        }
    }
}