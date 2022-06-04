/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using System;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;

namespace Ligral.Component.Models
{
    class Inverse : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model calculates the inverse of a matrix.";
            }
        }
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("x", this));
            OutPortList.Add(new OutPort("y", this));
        }
        public override void Check()
        {
            if (InPortList[0].RowNo != InPortList[0].ColNo)
            {
                throw logger.Error(new ModelException(this, $"Only square matrix can be inverted but the matrix is of shape ({InPortList[0].RowNo}, {InPortList[0].ColNo})"));
            }
            base.Check();
        }
        protected override List<Matrix<double>> Calculate(List<Matrix<double>> values)
        {
            Matrix<double> matrix = values[0];
            try
            {
                Results[0] = matrix.Inverse();
                if (Results[0].Enumerate().Contains(double.NaN))
                {
                    throw logger.Error(new ModelException(this, "The matrix shall be invertable (nonsingular)"));
                }
            }
            catch (Exception e)
            {
                throw logger.Error(new ModelException(this, $"Cannot calculate the inverse of {matrix}: {e}"));
            }
            return Results;
        }
        public override List<int> GetCharacterSize()
        {
            return new List<int>() {InPortList[0].RowNo};
        }
    }
}