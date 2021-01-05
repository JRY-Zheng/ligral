/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Component;

namespace Ligral.Component.Models
{
    class VSplit : OutPortVariableModel
    {
        protected override string DocString
        {
            get
            {
                return "This model splits inputs vertically to matrices";
            }
        }
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("matrix", this));
            base.SetUpPorts();
        }
        protected override List<Signal> DefaultCalculate(List<Signal> values)
        {
            Signal inputSignal = values[0];
            Matrix<double> matrix = inputSignal.Unpack() as Matrix<double>;
            if (matrix == null)
            {
                throw logger.Error(new ModelException(this, "Double cannot be splitted."));
            }
            else if (matrix.RowCount != Results.Count)
            {
                throw logger.Error(new ModelException(this, "Row count inconsistency."));
            }
            else
            {
                var m = Matrix<double>.Build;
                for (int i = 0; i < matrix.RowCount; i++)
                {
                    Results[i].Pack(m.Dense(matrix.ColumnCount, 1, matrix.Row(i).ToArray()));
                }
            }
            return Results;
        }
    }
}