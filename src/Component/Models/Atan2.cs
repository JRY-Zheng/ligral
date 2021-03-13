/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using System;
using MathNet.Numerics.LinearAlgebra;

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
        protected override List<Matrix<double>> Calculate(List<Matrix<double>> values)
        {
            Results[0] = values[0].Broadcast(values[1], Math.Atan2);
            return Results;
        }
    }
}