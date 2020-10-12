using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;

namespace Ligral.Models
{
    class VStack : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model mixes input vectors to a matrix";
            }
        }
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("in0", this));
            OutPortList.Add(new OutPort("matrix", this));
        }
        public override InPort Expose(int inPortNO)
        {
            if (inPortNO == InPortCount())
            {
                InPort inPort = new InPort($"in{inPortNO}", this);
                InPortList.Add(inPort);
                return inPort;
            }
            else
            {
                return base.Expose(inPortNO);
            }
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
                        firstMatrix = firstMatrix.Stack(matrix);
                        break;
                    case double value:
                        firstMatrix = firstMatrix.Stack(m.Dense(1, 1, value));
                        break;
                }
            }
            Results[0].Pack(firstMatrix);
            return Results;
        }
    }
}