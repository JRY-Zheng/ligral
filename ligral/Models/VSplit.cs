using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Block;

namespace Ligral.Models
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
        protected override List<Signal> Calculate(List<Signal> values)
        {
            Signal inputSignal = values[0];
            Matrix<double> matrix = inputSignal.Unpack() as Matrix<double>;
            if (matrix == null)
            {
                throw new ModelException(this, "Double cannot be splitted.");
            }
            else if (matrix.RowCount != Results.Count)
            {
                throw new ModelException(this, "Row count inconsistency.");
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