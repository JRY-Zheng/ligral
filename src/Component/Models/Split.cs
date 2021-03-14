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
    class Split : OutPortVariableModel
    {
        protected override string DocString
        {
            get
            {
                return "This model splits inputs item-wise to doubles";
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
            else if (rowNo * colNo != OutPortList.Count)
            {
                throw logger.Error(new ModelException(this, $"The input matrix has {rowNo} rows and {colNo} columns, but there are {OutPortList.Count} out ports."));
            }
            foreach (OutPort outPort in OutPortList)
            {
                outPort.SetShape(0, 0);
            }
        }
        protected override List<Matrix<double>> Calculate(List<Matrix<double>> values)
        {
            if (values[0].RowCount*values[0].ColumnCount != Results.Count)
            {
                throw logger.Error(new ModelException(this, "Item count inconsistency."));
            }
            else
            {
                for (int r=0; r < values[0].RowCount; r++)
                {
                    for (int c=0; c < values[0].ColumnCount; c++)
                    {
                        Results[r*values[0].ColumnCount] = values[0][r,c].ToMatrix();
                    }
                }
            }
            return Results;
        }
    }
}