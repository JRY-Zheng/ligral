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
    class VStack : InPortVariableModel
    {
        protected override string DocString
        {
            get
            {
                return "This model stacks inputs vertically to a matrix";
            }
        }
        protected override void SetUpPorts()
        {
            base.SetUpPorts();
            OutPortList.Add(new OutPort("matrix", this));
        }
        protected override List<Signal> DefaultCalculate(List<Signal> values)
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
                throw logger.Error(new LigralException("Signal with undefined type"));
            }
            foreach(Signal signal in values.Skip(1))
            {
                try
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
            }
            Results[0].Pack(firstMatrix);
            return Results;
        }
    }
}