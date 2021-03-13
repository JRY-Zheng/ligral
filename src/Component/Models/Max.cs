/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;

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
            Results[0] = values[0].Broadcast(values[1], (x, y) => x < y ? y : x);
            return Results;
        }
    }
}