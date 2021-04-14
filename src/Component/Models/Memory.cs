/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Component.Parameter>;
using System;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Component;
using Ligral.Simulation;

namespace Ligral.Component.Models
{
    class Memory : InitializeableModel
    {
        protected override string DocString
        {
            get
            {
                return "This model outputs the value from the previous input.";
            }
        }
        private Matrix<double> stack;
        public override void Check()
        {
            base.Check();
            stack = Matrix<double>.Build.DenseOfMatrix(initial);
            Results[0] = Matrix<double>.Build.Dense(rowNo, colNo);
        }
        protected override void DerivativeUpdate(Matrix<double> inputSignal)
        {
            inputSignal.CopyTo(stack);
        }
        protected override List<Matrix<double>> Calculate(List<Matrix<double>> values)
        {
            stack.CopyTo(Results[0]);
            return Results;
        }
    }
}