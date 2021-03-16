/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using System;
using MathNet.Numerics.LinearAlgebra;

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
            if (InPortList[1].RowNo != 1 || InPortList[1].ColNo != 1)
            {
                throw logger.Error(new ModelException(this, $"The exponent of a matrix should be a scalar, but a matrix with shape {InPortList[1].RowNo}x{InPortList[1].ColNo} received"));
            }
            else if (InPortList[0].RowNo != InPortList[0].ColNo)
            {
                throw logger.Error(new ModelException(this, $"The base matrix should be a square matrix, but a matrix with shape {InPortList[0].RowNo}x{InPortList[0].ColNo} received"));
            }
            else
            {
                OutPortList[0].SetShape(InPortList[0].RowNo, InPortList[0].ColNo);
            }
        }
        protected override List<Matrix<double>> Calculate(List<Matrix<double>> values)
        {
            try
            {
                Results[0] = values[0].MatPow(values[1]);
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