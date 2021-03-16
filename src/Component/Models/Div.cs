/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using System;

namespace Ligral.Component.Models
{
    class Div : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model outputs the matrix product of the first input and the inverse of the second input";
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
            if (InPortList[1].RowNo == 1 && InPortList[1].ColNo == 1)
            {
                OutPortList[0].SetShape(InPortList[0].RowNo, InPortList[0].ColNo);
            }
            else if (InPortList[0].RowNo == 1 && InPortList[0].ColNo == 1)
            {
                OutPortList[0].SetShape(InPortList[1].RowNo, InPortList[1].ColNo);
            }
            else if (InPortList[0].ColNo == InPortList[1].RowNo && InPortList[1].RowNo == InPortList[1].ColNo)
            {
                OutPortList[0].SetShape(InPortList[0].RowNo, InPortList[1].ColNo);
            }
            else
            {
                throw logger.Error(new ModelException(this, $"shape in consistency in matrix product with shapes {InPortList[0].RowNo}x{InPortList[0].ColNo} and {InPortList[1].RowNo}x{InPortList[1].ColNo}"));
            }
        }
        protected override List<Matrix<double>> Calculate(List<Matrix<double>> values)
        {
            try
            {
                Results[0] = values[0].RightDiv(values[1]);
            }
            catch (System.ArgumentException e)
            {
                string message = e.Message;
                int indexOfParenthesis = message.IndexOf('(');
                if (indexOfParenthesis>=0)
                {
                    message = message.Substring(0, indexOfParenthesis);
                }
                throw logger.Error(new ModelException(this, message));
            }
            return Results;
        }
    }
}