/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using System;
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
        protected override List<Signal> Calculate(List<Signal> values)
        {
            Signal inputSignal = values[0];
            Signal outputSignal = Results[0];
            Matrix<double> matrix;
            if (inputSignal.IsMatrix)
            {
                matrix = (Matrix<double>) inputSignal.Unpack();
            }
            else
            {
                throw logger.Error(new ModelException(this));
            }
            try
            {
                outputSignal.Pack(matrix.Inverse());
            }
            catch (Exception e)
            {
                throw logger.Error(new ModelException(this, $"Cannot calculate the inverse of {matrix}: {e}"));
            }
            return Results;
        }
    }
}