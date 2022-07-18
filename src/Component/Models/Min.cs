/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;

namespace Ligral.Component.Models
{
    class Min : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model returns minimal value among inputs";
            }
        }
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("first", this));
            InPortList.Add(new InPort("second", this));
            OutPortList.Add(new OutPort("min", this));
        }
        public override void Check()
        {
            try
            {
                (int xRowNo, int xColNo) = MatrixIteration.BroadcastShape(InPortList[0].RowNo, InPortList[0].ColNo, InPortList[1].RowNo, InPortList[1].ColNo);
                OutPortList[0].SetShape(xRowNo, xColNo);
            }
            catch (Exception e)
            {
                throw logger.Error(new ModelException(this, e.Message));
            }
        }
        protected override List<Matrix<double>> Calculate(List<Matrix<double>> values)
        {
            Results[0] = values[0].Broadcast(values[1], (x, y) => x > y ? y : x);
            return Results;
        }
        public override List<int> GetCharacterSize()
        {
            return new List<int>() 
            {
                Math.Max(InPortList[0].RowNo, InPortList[1].RowNo),
                Math.Max(InPortList[0].ColNo, InPortList[1].ColNo)
            };
        }
    }
}