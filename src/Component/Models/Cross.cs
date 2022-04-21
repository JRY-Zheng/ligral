/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using System;

namespace Ligral.Component.Models
{
    class Cross : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model outputs the cross product of the two inputs";
            }
        }
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("left", this));
            InPortList.Add(new InPort("right", this));
            OutPortList.Add(new OutPort("result", this));
        }
        public override void Check()
        {
            if (InPortList[1].RowNo == 1 && InPortList[1].ColNo == 3 && InPortList[0].RowNo == 1 && InPortList[0].ColNo == 3)
            {
                OutPortList[0].SetShape(1, 3);
            }
            else
            {
                throw logger.Error(new ModelException(this, $"shape of (1, 3) expected in cross product but received shapes {InPortList[0].RowNo}x{InPortList[0].ColNo} and {InPortList[1].RowNo}x{InPortList[1].ColNo}"));
            }
        }
        protected override List<Matrix<double>> Calculate(List<Matrix<double>> values)
        {
            if (Results[0] == null)
            {
                Results[0] = Matrix<double>.Build.Dense(1, 3);
            }
            Results[0][0,0] = values[0][0,1]*values[1][0,2]-values[0][0,2]*values[1][0,1];
            Results[0][0,1] = values[0][0,2]*values[1][0,0]-values[0][0,0]*values[1][0,2];
            Results[0][0,2] = values[0][0,0]*values[1][0,1]-values[0][0,1]*values[1][0,0];
            return Results;
        }
    }
}