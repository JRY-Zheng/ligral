/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using Ligral.Component;

namespace LigralPlugins.Control.Models
{
    public class TestModel : Model
    {

        protected override string DocString
        {
            get
            {
                return "This model outputs twice the input.";
            }
        }
        protected override List<Signal> DefaultCalculate(List<Signal> values)
        {
            Signal inputSignal = values[0];
            Signal outputSignal = Results[0];
            outputSignal.Clone(2*inputSignal);
            return Results;
        }
    }
}