/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using System;
using Ligral.Component;

namespace Ligral.Component.Models
{
    class Pow2 : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model calculates y=base^index.";
            }
        }
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("base", this));
            InPortList.Add(new InPort("index", this));
            OutPortList.Add(new OutPort("y", this));
        }
        public override void Check()
        {
            int xRowNo = InPortList[0].ColNo;
            int xColNo = InPortList[0].RowNo;
            int baseRowNo = InPortList[0].RowNo;
            int baseColNo = InPortList[0].ColNo;
            if (xRowNo != baseRowNo || xColNo != baseColNo)
            {
                throw logger.Error(new ModelException(this, $"Two in put must have the same shape but ({xRowNo}, {xColNo}) and ({baseRowNo}, {baseColNo}) got"));
            }
            OutPortList[0].SetShape(xRowNo, xColNo);
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            // Results.Clear();
            // Results.Add(Math.Pow(values[0], values[1]));
            Signal baseSignal = values[0];
            Signal indexSignal = values[1];
            Signal outputSignal = Results[0];
            outputSignal.Clone(baseSignal.ZipApply(indexSignal, Math.Pow));
            return Results;
        }
    }
}