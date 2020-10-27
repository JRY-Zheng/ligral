using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Block;

namespace Ligral.Models
{
    class HStack : InPortVariableModel
    {
        protected override string DocString
        {
            get
            {
                return "This model stacks inputs horizontally to a matrix";
            }
        }
        protected override void SetUpPorts()
        {
            base.SetUpPorts();
            OutPortList.Add(new OutPort("matrix", this));
        }
        protected override List<Signal> Calculate(List<Signal> values)
        {
            Signal firstSignal = values[0];
            Matrix<double> firstMatrix;
            MatrixBuilder<double> m = Matrix<double>.Build;
            switch (firstSignal.Unpack())
            {
                case Matrix<double> matrix:
                    firstMatrix = matrix;
                    break;
                case double value:
                    firstMatrix = m.Dense(1, 1, value);
                    break;
                default:
                    throw new LigralException("Signal with undefined type");
            }
            foreach(Signal signal in values.Skip(1))
            {
                switch (signal.Unpack())
                {
                    case Matrix<double> matrix:
                        firstMatrix = firstMatrix.Append(matrix);
                        break;
                    case double value:
                        firstMatrix = firstMatrix.Append(m.Dense(1, 1, value));
                        break;
                }
            }
            Results[0].Pack(firstMatrix);
            return Results;
        }
    }
}