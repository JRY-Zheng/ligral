/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using System;
using Ligral.Component;

namespace Ligral.Component.Models
{
    class Terminal : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model does not do anything but hide input.";
            }
        }
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("input", this));
        }
        public override void Check()
        {
            
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            return Results;
        }
    }
}