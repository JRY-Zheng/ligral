/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using System;
using MathNet.Numerics.LinearAlgebra;

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
        protected override List<Matrix<double>> Calculate(List<Matrix<double>> values)
        {
            Results[0] = values[0].Map2(Math.Log, values[1]);
            return Results;
        }
    }
}