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
        public override void Check()
        {
            int rowNo = InPortList[0].RowNo;
            int colNo = InPortList[0].ColNo;
            if (rowNo == 0 || colNo == 0)
            {
                throw logger.Error(new ModelException(this, $"The input should be a matrix"));
            }
            else if (rowNo != OutPortList.Count)
            {
                throw logger.Error(new ModelException(this, $"The input matrix has {rowNo} rows, but there are {OutPortList.Count} out ports."));
            }
            foreach (OutPort outPort in OutPortList)
            {
                outPort.SetShape(1, colNo);
            }
        }
        protected override List<Matrix<double>> Calculate(List<Matrix<double>> values)
        {
            Matrix<double> matrix = values[0];
            if (matrix.RowCount != Results.Count)
            {
                throw logger.Error(new ModelException(this, "Row count inconsistency."));
            }
            else
            {
                var m = Matrix<double>.Build;
                for (int i = 0; i < matrix.RowCount; i++)
                {
                    Results[i] = m.Dense(matrix.ColumnCount, 1, matrix.Row(i).ToArray());
                }
            }
            return Results;
        }
    }
}