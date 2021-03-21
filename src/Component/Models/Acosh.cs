/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Linq;
using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;

namespace Ligral.Component.Models
{
    class Acosh : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model calculates y=acosh(x).";
            }
        }
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("x", this));
            OutPortList.Add(new OutPort("y", this));
        }
        protected override List<Matrix<double>> Calculate(List<Matrix<double>> values)
        {
            Results[0] = values[0].Map(x => Math.Acosh(x));
            if (Results[0].Enumerate().Contains(double.NaN))
            {
                throw logger.Error(new ModelException(this, "The input of acosh shall be greater than 1"));
            }
            return Results;
        }
    }
}