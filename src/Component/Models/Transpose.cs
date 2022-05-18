/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using System;

namespace Ligral.Component.Models
{
    class Transpose : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model outputs the transpose of the input";
            }
        }
        public override void Check()
        {
            OutPortList[0].SetShape(InPortList[0].ColNo, InPortList[0].RowNo);
        }
        protected override List<Matrix<double>> Calculate(List<Matrix<double>> values)
        {
            Results[0] = values[0].Transpose();
            return Results;
        }
    }
}